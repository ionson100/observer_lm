using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace observerLm.controls;

public partial class CurlControl : UserControl
{
    public CurlControl()
    {
        InitializeComponent();
    }
    public void SetCurlText(string curlCommand)
    {
        OutputTextBoxR.Text = curlCommand;
    }

    private async void Copy_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Button _) return;
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard != null && !string.IsNullOrEmpty(OutputTextBoxR.Text))
            {
                await clipboard.SetTextAsync(OutputTextBoxR.Text.Trim());
            }
        }
        catch (Exception ex)
        {
          await MessageBoxManager.GetMessageBoxStandard("Ошибка", ex.Message,ButtonEnum.Ok,Icon.Error).ShowAsync();
        }
    }
}