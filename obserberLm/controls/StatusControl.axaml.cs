using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace obserberLm.controls;

public partial class StatusControl : UserControl
{
    public StatusControl(string s, string sr)
    {
        InitializeComponent();
        TextBoxStatus.Text = s;
        CurrentControlCore.SetCurlText(sr);
    }
}