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
using MsBox.Avalonia;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace observerLm.controls;

public partial class CheckingControl : UserControl
{
    private readonly MySettings? _settings = MySettings.GetSettings();
    [Obsolete("Obsolete")]
    public CheckingControl()
    {
        InitializeComponent();
        
        Loaded += (_, _) => InputTextBox.Focus();
        InputTextBoxGroup.AddHandler(TextInputEvent, OnTextInput, RoutingStrategies.Tunnel);
        InputTextBoxGroup.PastingFromClipboard += OnPastingFromClipboard;
    }
    private void OnTextInput(object? sender, TextInputEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Text) && !char.IsDigit(e.Text[0]))
        {
            e.Handled = true;
        }
    }
    [Obsolete("Obsolete")]
    private async void OnPastingFromClipboard(object? sender, RoutedEventArgs e)
    {
        try
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard == null) return;
            e.Handled = true;
           
            var text = await clipboard.GetTextAsync();
            if (text != null)
            { 
                var filteredText = new string(text.Where(char.IsDigit).ToArray()); 
                ((TextBox)sender!).Text += filteredText;
            }
         
        }
        catch (Exception ex)
        {
            await MessageBoxManager.GetMessageBoxStandard("Ошибка", ex.Message).ShowAsync();
           
        }
    }

    private async void CheckButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            string? code = InputTextBox.Text;
            if (string.IsNullOrWhiteSpace(code))
            {
                await MessageBoxManager.GetMessageBoxStandard("Ошибка", "Пожалуйста, введите код для проверки.").ShowAsync();
           
                return;
            }

            string? group = InputTextBoxGroup?.Text; 

            await RequestCodeCheckAsync(code,group, (s,sr) =>
            {
                OutputTextBox.Text = s;
                CurrentControlCore.SetCurlText(sr);
            });
        }
        catch (Exception ex)
        {
            await MessageBoxManager.GetMessageBoxStandard("Ошибка", ex.Message).ShowAsync();
        }
    }
    
     private class LmListCode
        {
            [JsonProperty("cis_list")] public List<LmItemCode> CisList { get; set; } = new();
        }

        private class LmItemCode
        {
            [JsonProperty("cis")] 
            public String Cis { get; set; } = null!;
            [JsonProperty("pg",NullValueHandling = NullValueHandling.Ignore)]
            public int? Pg { get; set; }
        }
        //0104670540176099215'W9Um

        public async Task RequestCodeCheckAsync(string code,string? group, Action<string,string> action)
        {
            if (_settings == null)
            {
                await MessageBoxManager.GetMessageBoxStandard("Ошибка", "Настройки приложения не найдены.").ShowAsync();
                return;
            }

            if (!string.IsNullOrWhiteSpace(group))
            {
                if(!int.TryParse(group, out int result))
                {
                    await MessageBoxManager.GetMessageBoxStandard("Ошибка", "Введите правильно группу товара.").ShowAsync();
                    return;
                }

                if (result <= 0)
                {
                    await MessageBoxManager.GetMessageBoxStandard("Ошибка", "Введите правильно группу товара. {0} не допускается").ShowAsync();
                    return; 
                }
            }
            string request = "";
            LmListCode lmListCode = new LmListCode();
            lmListCode.CisList.Add(new LmItemCode { Cis = code,Pg = string.IsNullOrWhiteSpace(group)?null:int.Parse(group)});
            string? url = null;
            string? json = null;
            LoadingBar.IsVisible = true;
            try
            {
                
                using var httpClient = new HttpClient(new CurlLoggingHandler(
                    new HttpClientHandler(),
                    s => request=s
                ));
                httpClient.Timeout = TimeSpan.FromMilliseconds(3000);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(App.ApplicationJson));
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {_settings.Auth}");
                // Отправка POST-запроса
                url = _settings.Url + "cis/outCheck";
                json = JsonConvert.SerializeObject(lmListCode);
                using var response = await httpClient.PostAsync(url,
                    new StringContent(json, Encoding.UTF8, App.ApplicationJson));

                int status = (int)response.StatusCode;
                string responseBody = await response.Content.ReadAsStringAsync();
                if (status == 200)
                {
                    string prettyJson = JToken.Parse(responseBody).ToString(Formatting.Indented);
                    action.Invoke(prettyJson,request);
                }
                else
                {
                    string error = "Ошибка при запросе к API. Код статуса: " + status + Environment.NewLine +
                                   responseBody + Environment.NewLine +
                                   "Url: " + url + Environment.NewLine + json;
                    action.Invoke(error,request);
                }



            }
            catch (Exception ex)
            {
                string error = "Ошибка при запросе к API. Exception: " + Environment.NewLine + ex.Message +
                               Environment.NewLine + url +
                               Environment.NewLine + json;
                action.Invoke(error,request);
            }
            finally
            {
                LoadingBar.IsVisible = false;

            }
        }
}