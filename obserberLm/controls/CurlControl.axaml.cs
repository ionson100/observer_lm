using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace obserberLm.controls;

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
        if (sender is Button button)
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard != null && !string.IsNullOrEmpty(OutputTextBoxR.Text))
            {
                await clipboard.SetTextAsync(OutputTextBoxR.Text.Trim());
            }
        }
        
    }
}