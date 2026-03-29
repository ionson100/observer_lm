using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using observerLm.controls.dialogs;

namespace observerLm.controls;

public partial class CheckingControl : UserControl
{
    private MySettings? _settings;

    public CheckingControl()
    {
        InitializeComponent();
        
        Loaded += (_, _) => InputTextBox.Focus();
        
        // Фильтрация ввода: только цифры
        InputTextBoxGroup.AddHandler(TextInputEvent, OnTextInput, RoutingStrategies.Tunnel);
        InputTextBoxGroup.PastingFromClipboard += OnPastingFromClipboard;
    }

    private void OnTextInput(object? sender, TextInputEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Text) && !char.IsDigit(e.Text[0]))
        {
            e.Handled = true; // Блокируем нецифровой ввод
        }
    }

    private async void OnPastingFromClipboard(object? sender, RoutedEventArgs e)
    {
        try
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard == null) return;

            e.Handled = true;
#pragma warning disable CS0618 // Type or member is obsolete
            var text = await clipboard.GetTextAsync();
#pragma warning restore CS0618 // Type or member is obsolete
            if (string.IsNullOrEmpty(text)) return;

            var filteredText = new string(text.Where(char.IsDigit).ToArray());
            var textBox = (TextBox)sender!;
            textBox.Text += filteredText;
            textBox.CaretIndex = textBox.Text.Length; // Курсор в конец
        }
        catch (Exception ex)
        {
            await MessageDialog.Show("Ошибка", ex.Message);
        }
    }

    private async void CheckButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_settings == null)
        {
            await MessageDialog.Show("Ошибка", "Настройки приложения не загружены.");
            return;
        }

        string? code = InputTextBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(code))
        {
            await MessageDialog.Show("Ошибка", "Пожалуйста, введите код для проверки.");
            InputTextBox.Focus();
            return;
        }

        string? groupText = InputTextBoxGroup?.Text?.Trim();

        if (!string.IsNullOrWhiteSpace(groupText))
        {
            if (!int.TryParse(groupText, out int groupValue) || groupValue <= 0)
            {
                await MessageDialog.Show("Ошибка", "Введите корректную группу товара (целое число > 0).");
                InputTextBoxGroup?.Focus();
                return;
            }

            LoadingBar.IsVisible = true;
            await RequestCodeCheckAsync(
                code,
                groupValue,
                (response, curl) =>
                {
                    OutputTextBox.Text = response;
                    CurrentControlCore.SetCurlText(curl);
                });
        }
        else
        {
            LoadingBar.IsVisible = true;
            await RequestCodeCheckAsync(
                code,
                null,
                (response, curl) =>
                {
                    OutputTextBox.Text = response;
                    CurrentControlCore.SetCurlText(curl);
                });
        }
    }

    //0104670540176099215'W9Um
    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        await LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            _settings = await MySettings.GetSettings();
            if (_settings == null)
            {
                await MessageDialog.Show("Ошибка", "Не удалось загрузить настройки приложения.");
            }
        }
        catch (Exception ex)
        {
            await MessageDialog.Show("Ошибка", $"Критическая ошибка при загрузке настроек: {ex.Message}");
        }
    }

    private async Task RequestCodeCheckAsync(string code, int? group, Action<string, string>? action)
    {
        if (_settings == null)
        {
            action?.Invoke("Ошибка: настройки не загружены.", "");
            return;
        }

        string requestLog = "";
        string? url = null;
        string? json = null;

        try
        {
            using var handler = new CurlLoggingHandler(new HttpClientHandler(), s => requestLog = s);
            using var httpClient = new HttpClient(handler);
            httpClient.Timeout = TimeSpan.FromMilliseconds(3000);

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(App.ApplicationJson));
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {_settings.Auth}");

            url = $"{_settings.Url.TrimEnd('/')}/cis/outCheck";
            var payload = new LmListCode
            {
                CisList = new List<LmItemCode>
                {
                    new() { Cis = code, Pg = group }
                }
            };
            json = JsonConvert.SerializeObject(payload, Formatting.None);
            var content = new StringContent(json, Encoding.UTF8, App.ApplicationJson);

            using var response = await httpClient.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            var statusCode = (int)response.StatusCode;

            if (statusCode == 200)
            {
                var prettyJson = JToken.Parse(responseBody).ToString(Formatting.Indented);
                action?.Invoke(prettyJson, requestLog);
            }
            else
            {
                var error = $"Ошибка при запросе к API.{Environment.NewLine}" +
                            $"Код статуса: {statusCode}{Environment.NewLine}" +
                            $"{responseBody}{Environment.NewLine}" +
                            $"Url: {url}{Environment.NewLine}" +
                            $"JSON: {json}";
                action?.Invoke(error, requestLog);
            }
        }
        catch (Exception ex)
        {
            var error = $"Ошибка подключения к API.{Environment.NewLine}" +
                        $"{ex.Message}{Environment.NewLine}" +
                        (!string.IsNullOrEmpty(url) ? $"Url: {url}{Environment.NewLine}" : "") +
                        (!string.IsNullOrEmpty(json) ? $"JSON: {json}{Environment.NewLine}" : "");
            action?.Invoke(error, requestLog);
        }
        finally
        {
            LoadingBar.IsVisible = false;
        }
    }

    #region Внутренние классы

    private class LmListCode
    {
        [JsonProperty("cis_list")]
        public List<LmItemCode> CisList { get; set; } = new();
    }

    private class LmItemCode
    {
        [JsonProperty("cis")]
        public string Cis { get; set; } = null!;

        [JsonProperty("pg", NullValueHandling = NullValueHandling.Ignore)]
        public int? Pg { get; set; }
    }

    #endregion
}