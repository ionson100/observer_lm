using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace observerLm.controls.dialogs;

public partial class HelpDialog : Window
{
    public HelpDialog()
    {
        InitializeComponent();
    }

    private async void Copy_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { CommandParameter: Label label })
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard != null && label.Content != null && !string.IsNullOrEmpty(label.Content.ToString()))
            {
                await clipboard.SetTextAsync(label.Content.ToString());
            
                // Опционально: можно визуально мигнуть иконкой или изменить цвет
                // чтобы пользователь понял, что текст скопирован
            }
        }
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }
}