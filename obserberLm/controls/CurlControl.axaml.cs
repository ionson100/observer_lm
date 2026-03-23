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

}