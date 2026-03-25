using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace observerLm.controls.dialogs;

public partial class ErrorWindow_ : Window
{
    public ErrorWindow_() => InitializeComponent();

    public ErrorWindow_(string message) : this()
    {
        ErrorText.Text = message;
    }

    private void Close_Click(object? sender, RoutedEventArgs e) => Close();

    private async void CopyError_Click(object? sender, RoutedEventArgs e)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard != null)
        {
            await clipboard.SetTextAsync(ErrorText.Text);
        }
    }
}