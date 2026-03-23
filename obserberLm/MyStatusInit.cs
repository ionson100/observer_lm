using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace obserberLm;


     class MyStatusInit
    {
        public async Task RequestPiotAsync(string append,Action<string,string> action)
        {
            string request = "";

            try
            {
               
                MySettings settings  = MySettings.GetSettings();
                using var httpClient = new HttpClient(new CurlLoggingHandler(
                    new HttpClientHandler(),
                    s => request = s
                ));
                httpClient.Timeout = TimeSpan.FromMilliseconds(3000);
              
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(App.ApplicationJson));
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {settings.Auth}");
                // Отправка POST-запроса
                string url = settings.Url + append;
                using var response = await httpClient.GetAsync(url);

                int status = (int)response.StatusCode;
                string responseBody = await response.Content.ReadAsStringAsync();
                if (status == 200)
                {
                    string prettyJson = JToken.Parse(responseBody).ToString(Formatting.Indented);
                    action.Invoke(prettyJson, request);
                }
                else
                {
                    string error="Ошибка при запросе к API. Код статуса: " + status+Environment.NewLine+ responseBody+Environment.NewLine+
                                 "Url: "+url;
                    action.Invoke(error, request);
                }
                    

                
            }
            catch (Exception ex)
            {
                string error = "Ошибка при запросе к API. Exception: " + Environment.NewLine + ex.Message;
                action.Invoke(error, request);
            }

        }

        class TempInit
        {
            [JsonProperty("token")] 
            public string Token { get; set; } = null!;
        }
        public async Task RequestInitAsync( Action<string,string> action)
        {
            string request = "";
            string? url=null;
            string? json=null;
            try
            {
                
                MySettings settings  = MySettings.GetSettings();
                using var httpClient = new HttpClient(new CurlLoggingHandler(
                    new HttpClientHandler(),
                    s => request = s
                ));
                httpClient.Timeout = TimeSpan.FromMilliseconds(3000);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(App.ApplicationJson));
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {settings.Auth}");
                // Отправка POST-запроса
                url = settings.Url + "init"; 
                json=JsonConvert.SerializeObject(new TempInit() { Token = settings.Token });
                using var response = await httpClient.PostAsync(url,
                    new StringContent(json, Encoding.UTF8, App.ApplicationJson));
                int status = (int)response.StatusCode;
                string responseBody = await response.Content.ReadAsStringAsync();
                if (status == 200)
                {
                    string prettyJson = $"Инициализация успешно.Http status:200{Environment.NewLine}Смотри вкладку Status, наблюдай за логами.";
                    action.Invoke(prettyJson, request);
                }
                else
                {
                    string error = "Ошибка при запросе к API. Код статуса: " + status + Environment.NewLine + responseBody + Environment.NewLine +
                                   "Url: " + url+Environment.NewLine+json;
                    action.Invoke(error, request);
                }



            }
            catch (Exception ex)
            {
                string error = "Ошибка при запросе к API. Exception: " + Environment.NewLine + ex.Message+Environment.NewLine+url+
                               Environment.NewLine+json;
                action.Invoke(error, request);
            }

        }

       
        class TempSell
        {
            [JsonProperty("cis_list")] public List<string>? CisList { get; set; }
        }

        public async Task RequestSellAsync(string appendUrl,string code,Action<string, string> action)
        {
            string request = "";
            string? url = null;
            string? json = null;
            TempSell tempSell = new TempSell { CisList = new List<string>() { code } };
            try
            {

                MySettings settings = MySettings.GetSettings();
                using var httpClient = new HttpClient(new CurlLoggingHandler(
                    new HttpClientHandler(),
                    s => request = s
                ));
                httpClient.Timeout = TimeSpan.FromMilliseconds(3000);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(App.ApplicationJson));
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {settings.Auth}");
                // Отправка POST-запроса
                url = settings.Url + appendUrl;
                json = JsonConvert.SerializeObject(tempSell);
                using var response = await httpClient.PostAsync(url,
                    new StringContent(json, Encoding.UTF8, App.ApplicationJson));
                int status = (int)response.StatusCode;
                string responseBody = await response.Content.ReadAsStringAsync();
                if (status == 200)
                {
                    string prettyJson = JToken.Parse(responseBody).ToString(Formatting.Indented);
                    action.Invoke(prettyJson, request);
                   
                }
                else
                {
                    string error = "Ошибка при запросе к API. Код статуса: " + status + Environment.NewLine + responseBody + Environment.NewLine +
                                   "Url: " + url + Environment.NewLine + json;
                    action.Invoke(error, request);
                }
            }
            catch (Exception ex)
            {
                string error = "Ошибка при запросе к API. Exception: " + Environment.NewLine + ex.Message + Environment.NewLine + url +
                               Environment.NewLine + json;
                action.Invoke(error, request);
            }
        }
}