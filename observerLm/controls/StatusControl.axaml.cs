using Avalonia.Controls;

namespace observerLm.controls;

public partial class StatusControl : UserControl
{
    public StatusControl(string s, string sr)
    {
        InitializeComponent();
        TextBoxStatus.Text = s;
        CurrentControlCore.SetCurlText(sr);
    }
}