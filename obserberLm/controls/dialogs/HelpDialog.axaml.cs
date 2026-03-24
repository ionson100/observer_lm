using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace obserberLm.controls.dialogs;

public partial class HelpDialog : Window
{
    public HelpDialog()
    {
        InitializeComponent();
    }

    private async void Copy_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.CommandParameter is TextBox textBox)
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard != null && !string.IsNullOrEmpty(textBox.Text))
            {
                await clipboard.SetTextAsync(textBox.Text);
            
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