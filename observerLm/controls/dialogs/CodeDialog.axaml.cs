using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using System;
using System.Linq;

namespace observerLm.controls.dialogs;

public partial class CodeDialog : Window
{
    public int Skip { get; private set; }
    public int Limit { get; private set; }
    [Obsolete("Obsolete")]
    public CodeDialog()
    {
        InitializeComponent();
        TxtSkip.Text= "0";
        TxtLimit.Text= "1000";

        Loaded += (_, _) =>
        {
            TxtSkip.Focus();
            if (!string.IsNullOrEmpty(TxtSkip.Text))
            {
                TxtSkip.CaretIndex = TxtSkip.Text.Length;
            }
        };
        TxtSkip.AddHandler(TextInputEvent, OnTextInput, RoutingStrategies.Tunnel);
        TxtLimit.AddHandler(TextInputEvent, OnTextInput, RoutingStrategies.Tunnel);
        TxtSkip.PastingFromClipboard += OnPastingFromClipboard;
        TxtLimit.PastingFromClipboard += OnPastingFromClipboard;
       
      
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
        // Получаем доступ к буферу обмена
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard == null) return;

        // Блокируем стандартную вставку
        e.Handled = true;

        try
        {
            var text = await clipboard.GetTextAsync();
            if (text != null)
            {
                // Фильтруем: оставляем только цифры
                var filteredText = new string(text.Where(char.IsDigit).ToArray());
                ((TextBox)sender!).Text += filteredText;
            }
        }
        catch (TimeoutException)
        {
            // Игнорируем тайм-аут
        }
    }

    private void GetCodes_Click(object? sender, RoutedEventArgs e)
    {
        if (int.TryParse(TxtSkip.Text, out int skip) && int.TryParse(TxtLimit.Text, out int limit))
        {
            // Проверка диапазона для Limit
            if (limit < 1 || limit > 1000)
            {
                MessageBoxManager
                    .GetMessageBoxStandard("Ошибка", "Лимит должен быть в диапазоне от 1 до 1000.")
                    .ShowAsync();
                
                return;
            }

            Skip = skip;
            Limit = limit;
            
            this.Close(true);
        }
        else
        {
            MessageBoxManager
                .GetMessageBoxStandard("Ошибка", "Введите корректные числа.")
                .ShowAsync();
         
        }
    }
}