using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json;

namespace observerLm.controls;

public partial class SettingsControl : UserControl
{
    private MySettings? _settings;
    [Obsolete("Obsolete")]
    public SettingsControl()
    {
        InitializeComponent();
        _settings = MySettings.GetSettings();
        if(_settings==null)
            return;
        TxtUrl.Text=_settings.Url;
        TxtBasic.Text = _settings.Auth;
        TxtFolderPath.Text = _settings.FolderLog;
        TxtToken.Text=_settings.Token;
        TxtTail.Text = _settings.Tail.ToString();
        
        TxtTail.AddHandler(TextInputEvent, OnTextInput, RoutingStrategies.Tunnel);
        TxtTail.PastingFromClipboard += OnPastingFromClipboard;
    }
    
    private void OnTextInput(object? sender, TextInputEventArgs e)
    {
        // Оставляем только цифры
        if (!string.IsNullOrEmpty(e.Text) && !char.IsDigit(e.Text[0]))
        {
            // Блокируем ввод, если символ не цифра
            e.Handled = true;
        }
    }
    [Obsolete("Obsolete")]
    private async void OnPastingFromClipboard(object? sender, RoutedEventArgs e)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard == null) return;
        e.Handled = true;
        try
        {
            var text = await clipboard.GetTextAsync();
            if (text != null)
            {
                var filteredText = new string(text.Where(char.IsDigit).ToArray());
                ((TextBox)sender!).Text += filteredText;
            }
        }
        catch (TimeoutException)
        {
        }
    }
    
  
   

    private void ButtonBase_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if(_settings==null) return;
            bool res= Validate();
            if (res)
            {
                _settings.FolderLog= TxtFolderPath.Text.Trim();
                _settings.Auth= TxtBasic.Text.Trim();
                _settings.Token= TxtToken.Text.Trim();
                _settings.Url= TxtUrl.Text.Trim();
                _settings.Tail= int.Parse(TxtTail.Text.Trim());
                File.WriteAllText("settings/settings.json",JsonConvert.SerializeObject(_settings, Formatting.Indented));

                MessageBoxManager.GetMessageBoxStandard("warning", "Save Settings",ButtonEnum.Ok).ShowAsync();

            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            MessageBoxManager.GetMessageBoxStandard("Error", "Save Settings",ButtonEnum.Ok).ShowAsync();
        }
    
    }
    
     bool Validate()
        {
            if (string.IsNullOrWhiteSpace(TxtBasic.Text))
            {
                MessageBoxManager.GetMessageBoxStandard("warning", "Basic is empty",ButtonEnum.Ok).ShowAsync();
                return false;
            }
            if (string.IsNullOrWhiteSpace(TxtUrl.Text))
            {
                MessageBoxManager.GetMessageBoxStandard("warning", "Url is empty",ButtonEnum.Ok).ShowAsync();
                return false;
            }
            if (string.IsNullOrWhiteSpace(TxtTail.Text))
            {
                MessageBoxManager.GetMessageBoxStandard("warning", "Tail is empty",ButtonEnum.Ok).ShowAsync();
                return false;
            }
            if (string.IsNullOrWhiteSpace(TxtToken.Text))
            {
                MessageBoxManager.GetMessageBoxStandard("warning", "Token is empty",ButtonEnum.Ok).ShowAsync();
                return false;
            }
            if (string.IsNullOrWhiteSpace(TxtFolderPath.Text))
            {
                MessageBoxManager.GetMessageBoxStandard("warning", "Folder logs is empty",ButtonEnum.Ok).ShowAsync();
                return false;
            }
            bool isValid = Uri.TryCreate(TxtUrl.Text.Trim(), UriKind.Absolute, out Uri? uriResult)
                           && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (!isValid)
            {
                MessageBoxManager.GetMessageBoxStandard("warning", "Url string not valid",ButtonEnum.Ok).ShowAsync();
                return false;
            }

            if (!IsBase64String(TxtBasic.Text.Trim()))
            {
                MessageBoxManager.GetMessageBoxStandard("warning", "Basic is not BASE64",ButtonEnum.Ok).ShowAsync();
                return false;
            }
          
            if (!Directory.Exists(TxtFolderPath.Text.Trim()))
            {
                MessageBoxManager.GetMessageBoxStandard("warning", "Директория логов не найдена.",ButtonEnum.Ok).ShowAsync();
                return false;
            }

            if (int.TryParse(TxtTail.Text.Trim(), out int result) && result > 0)
            {
                
            }
            else
            {
                MessageBoxManager.GetMessageBoxStandard("warning", "The tail string must not be empty and greater than 0",ButtonEnum.Ok).ShowAsync();
             
                return false;
            }

            return true;
        }

        private bool IsBase64String(string base64)
        {
            if (string.IsNullOrWhiteSpace(base64) || base64.Length % 4 != 0)
                return false;

            try
            {
                _ = Convert.FromBase64String(base64);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }


        
}