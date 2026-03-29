using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using observerLm.controls.dialogs;

namespace observerLm;

public class MyStatusInit
{
    private const int TimeoutMs = 3000;

    /// <summary>
    /// Выполняет GET-запрос к LM с авторизацией.
    /// </summary>
    public async Task RequestPiotAsync(string append, Action<string?, string?> action)
    {
        await ExecuteRequestAsync(
            (client, url, _) => client.GetAsync(url),
            append,
            null,
            HandleSuccessResponse,
            (status, body, url, _) => $"Ошибка при запросе к LM. Статус ответа: {status}{Environment.NewLine}{body}{Environment.NewLine}Url: {url}",
            action);
    }

    /// <summary>
    /// Выполняет POST /init запрос для инициализации.
    /// </summary>
    public async Task RequestInitAsync(Action<string?, string?>? action)
    {
        var tempInit = new TempInit { Token = (await GetSettingsOrWarn())?.Token };
        if (tempInit.Token == null) return;

        await ExecuteRequestAsync(
            (client, url, content) => client.PostAsync(url, content),
            "init",
            () => JsonConvert.SerializeObject(tempInit),
            _ => $"Инициализация успешно. Http status:200{Environment.NewLine}Смотри вкладку Status, наблюдай за логами.",
            (status, body, url, json) => $"Ошибка при запросе к LM. Статус ответа: {status}{Environment.NewLine}{body}{Environment.NewLine}Url: {url}{Environment.NewLine}JSON: {json}",
            action);
    }

    /// <summary>
    /// Выполняет POST-запрос для операции продажи.
    /// </summary>
    public async Task RequestSellAsync(string appendUrl, string code, Action<string, string> action)
    {
        var tempSell = new TempSell { CisList = new List<string> { code } };
        await ExecuteRequestAsync(
            (client, url, content) => client.PostAsync(url, content),
            appendUrl,
            () => JsonConvert.SerializeObject(tempSell),
            body => JToken.Parse(body).ToString(Formatting.Indented),
            (status, body, url, json) => $"Ошибка при запросе к LM. Статус ответа: {status}{Environment.NewLine}{body}{Environment.NewLine}Url: {url}{Environment.NewLine}JSON: {json}",
            (result, request) =>
            {
                if (request != null) action(result!, request);
            });
    }

    #region Вспомогательные методы

    /// <summary>
    /// Универсальный метод выполнения HTTP-запросов.
    /// </summary>
    private async Task ExecuteRequestAsync(
        Func<HttpClient, string, HttpContent?, Task<HttpResponseMessage>> requestFactory,
        string endpoint,
        Func<string>? getJson,
        Func<string, string> onSuccess,
        Func<int, string, string, string?, string> onError,
        Action<string?, string?>? callback)
    {
        string requestLog = "";
        string? url = null;
        string? json = null;

        try
        {
            var settings = await GetSettingsOrWarn();
            if (settings == null)
            {
                callback?.Invoke(null, null);
                return;
            }

            using var handler = new CurlLoggingHandler(new HttpClientHandler(), s => requestLog = s);
            using var httpClient = new HttpClient(handler);
            httpClient.Timeout = TimeSpan.FromMilliseconds(TimeoutMs);

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(App.ApplicationJson));
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {settings.Auth}");

            url = $"{settings.Url.TrimEnd('/')}/{endpoint}";
            HttpContent? content = getJson != null ? new StringContent(getJson(), Encoding.UTF8, App.ApplicationJson) : null;

            var response = await requestFactory(httpClient, url, content);
            json = content?.ReadAsStringAsync().Result; // Логируем отправленный JSON
            var responseBody = await response.Content.ReadAsStringAsync();
            var statusCode = (int)response.StatusCode;

            string result = statusCode == 200
                ? onSuccess(responseBody)
                : onError(statusCode, responseBody, url, json);

            callback?.Invoke(result, requestLog);
        }
        catch (Exception ex)
        {
            string error = $"Ошибка при запросе к LM. Exception:{Environment.NewLine}{ex.Message}";
            if (!string.IsNullOrEmpty(url)) error += $"{Environment.NewLine}Url: {url}";
            if (!string.IsNullOrEmpty(json)) error += $"{Environment.NewLine}JSON: {json}";

            callback?.Invoke(error, requestLog);
        }
    }

    /// <summary>
    /// Загружает настройки или показывает ошибку.
    /// </summary>
    private async Task<MySettings?> GetSettingsOrWarn()
    {
        var settings = await MySettings.GetSettings();
        if (settings == null)
        {
            await MessageDialog.Show("Ошибка", "Настройки приложения не заданы");
        }
        return settings;
    }

    /// <summary>
    /// Обработка успешного JSON-ответа — форматирование.
    /// </summary>
    private static string HandleSuccessResponse(string body)
    {
        return JToken.Parse(body).ToString(Formatting.Indented);
    }

    #endregion

    #region Внутренние классы

    class TempInit
    {
        [JsonProperty("token")]
        public string? Token { get; set; }
    }

    class TempSell
    {
        [JsonProperty("cis_list")]
        public List<string>? CisList { get; set; }
    }

    #endregion
}