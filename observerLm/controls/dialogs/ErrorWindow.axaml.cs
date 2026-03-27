using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace observerLm.controls.dialogs;

public partial class ErrorWindow : Window
{
    private ErrorWindow() => InitializeComponent();

    public ErrorWindow(string message) : this()
    {
        ErrorText.Text = message;
    }

    private void Close_Click(object? sender, RoutedEventArgs e) => Close();

    private async void CopyError_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard != null)
            {
                await clipboard.SetTextAsync(ErrorText.Text);
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }
}