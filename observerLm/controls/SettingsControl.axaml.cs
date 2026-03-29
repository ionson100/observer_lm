using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Newtonsoft.Json;
using observerLm.controls.dialogs;

namespace observerLm.controls;

public partial class SettingsControl : UserControl
{
    private MySettings? _settings;

    public SettingsControl()
    {
        InitializeComponent();
        LoadSettingsAsync();
    }
  
    private async void LoadSettingsAsync()
    {
        try
        {
            _settings = MySettings.Settings;
            if (_settings == null) return;

            // Привязка данных через свойства (или можно использовать MVVM)
            TxtUrl.Text = _settings.Url;
            TxtBasic.Text = _settings.Auth;
            TxtFolderPath.Text = _settings.FolderLog;
            TxtToken.Text = _settings.Token;
            TxtTail.Text = _settings.Tail.ToString();

            // Ограничение ввода: только цифры
            TxtTail.AddHandler(TextInputEvent, OnTextInput, RoutingStrategies.Tunnel);
#pragma warning disable CS0618 // Type or member is obsolete
            TxtTail.PastingFromClipboard += OnPastingFromClipboard;
#pragma warning restore CS0618 // Type or member is obsolete
        }
        catch (Exception ex)
        {
            await MessageDialog.Show("Ошибка", $"Не удалось загрузить настройки:\n{ex.Message}");
        }
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
            await MessageDialog.Show("Ошибка", ex.Message);
        }
    }
    
  
   

    private async void ButtonBase_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if(_settings==null) return;
            var res= await Validate();
            if (res)
            {
                _settings.FolderLog= TxtFolderPath.Text!.Trim();
                _settings.Auth= TxtBasic.Text!.Trim();
                _settings.Token= TxtToken.Text!.Trim();
                _settings.Url= TxtUrl.Text!.Trim();
                _settings.Tail= int.Parse(TxtTail.Text!.Trim());

                MySettings.Save();

                MainWindow.Instance!.MyNotification.Show("Настройки успешно сохранены.");
               
               

            }
        }
        catch (Exception exception)
        {
            await MessageDialog.Show("Сохранение настроек Ошибка", exception.Message);
            Console.WriteLine(exception);
        }
    
    }

    private async Task<bool> Validate()
    {
        (TextBox field, string message)[] validations = new[]
        {
            (TxtBasic, "Элемент Basic пуст."),
            (TxtUrl, "Элемент URL пуст."),
            (TxtTail, "Элемент TAIL пуст."),
            (TxtToken, "Элемент Token пуст."),
            (TxtFolderPath, "Элемент FOLDER пуст.")
        };

        foreach (var (field, msg) in validations)
        {
            if (string.IsNullOrWhiteSpace(field.Text))
            {
                await ShowWarning(msg, field);
                return false;
            }
        }

        if (!Uri.TryCreate(TxtUrl.Text!.Trim(), UriKind.Absolute, out var uriResult) ||
            (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
        {
            await ShowWarning("URL строка некорректна.", TxtUrl);
            return false;
        }

        if (!IsBase64String(TxtBasic.Text!.Trim()))
        {
            await ShowWarning("Basic должен быть в формате Base64.", TxtBasic);
            return false;
        }

        if (!Directory.Exists(TxtFolderPath.Text!.Trim()))
        {
            await ShowWarning("Данная папка отсутствует в системе.", TxtFolderPath);
            return false;
        }

        if (!int.TryParse(TxtTail.Text!.Trim(), out var tailValue) || tailValue <= 0)
        {
            await ShowWarning("Tail должно содержать целое число больше нуля.", TxtTail);
            return false;
        }

        return true;
    }

    private async Task ShowWarning(string message, TextBox field)
    {
        await MessageDialog.Show("Внимание", message, DialogType.Warning);
        field.Focus();
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