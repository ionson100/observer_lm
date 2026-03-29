using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using observerLm.controls.dialogs;

namespace observerLm.controls;

public partial class CurlControl : UserControl
{
    public CurlControl()
    {
        InitializeComponent();
    }

    public void SetCurlText(string curlCommand)
    {
        OutputTextBoxR.Text = curlCommand?.Trim() ?? string.Empty;
    }

    private async void Copy_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var text = OutputTextBoxR.Text?.Trim();
            if (string.IsNullOrEmpty(text))
            {
                MainWindow.Instance?.MyNotification.Show("Нечего копировать");
                return;
            }

            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard == null) return;

            await clipboard.SetTextAsync(text);
            MainWindow.Instance?.MyNotification.Show("Скопировано в буфер обмена");
        }
        catch (Exception ex)
        {
            await MessageDialog.Show("Ошибка копирования", ex.Message);
        }
    }
}