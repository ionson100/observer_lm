using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using obserberLm.controls;

namespace obserberLm;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ContentControlHost.Content = new LogControl();
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        Button? button = (Button)sender!;
        if(button==null)return;
        if(button.Tag==null)return;
        switch (button.Tag.ToString())
        {
            case "b1":
            { 
                var d= await MessageBoxManager.GetMessageBoxStandard("Инициализация", "Произвести инициализацию локального модуля?",ButtonEnum.OkCancel).ShowAsync();
                if (d == ButtonResult.Ok)
                {
                    LoadingBar.IsVisible=true;
                    Dispose();
                    try
                    {
                        await new MyStatusInit().RequestInitAsync((s,sr) =>
                        {
                            ContentControlHost.Content = new StatusControl(s,sr);
                        });
                    }
                    finally
                    {
                        LoadingBar.IsVisible=false;

                    }
                }
                
                break;
            }
            case "b2":
            { 
                LoadingBar.IsVisible = true;
                Dispose();
                try
                {
                    await new MyStatusInit().RequestPiotAsync("status",(s,sr) =>
                    {
                        ContentControlHost.Content = new StatusControl(s,sr);
                    });
                }
                finally
                {
                    LoadingBar.IsVisible = false;

                }
                break;
            }
            case "bst":
            {
                LoadingBar.IsVisible = true;
                Dispose();
                try
                {
                    await new MyStatusInit().RequestPiotAsync("stats", (s, sr) =>
                    {
                        ContentControlHost.Content = new StatusControl(s, sr);
                    });
                }
                finally
                {
                    LoadingBar.IsVisible = false;

                }

                break;
            }
            case "bconfig":
            {
                LoadingBar.IsVisible = true;
                Dispose();
                try
                {
                    await new MyStatusInit().RequestPiotAsync("config", (s, sr) =>
                    {
                        ContentControlHost.Content = new StatusControl(s, sr);
                    });
                }
                finally
                {
                    LoadingBar.IsVisible = false;

                }

                break;
            }
            case "b3":
            {
                Dispose();
                ContentControlHost.Content=new LogControl();
                break;
            }
            case "b4":
            {
                Dispose();
                ContentControlHost.Content = new SettingsControl();
                break;
            }
        }
        
    }

    void Dispose()
    {
        if (ContentControlHost.Content is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}