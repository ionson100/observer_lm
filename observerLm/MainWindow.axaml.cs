using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using observerLm.controls;
using observerLm.controls.dialogs;

namespace observerLm;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ContentControlHost.Content = new LogControl2();
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
                ContentControlHost.Content=new LogControl2();
                break;
            }
            case "b4":
            {
                Dispose();
                ContentControlHost.Content = new SettingsControl();
                break;
            }
            case "bhelp":
            {
                string ?path=GetHelpFilePath();
                if (!string.IsNullOrEmpty(path))
                {
                    string url = "file://" + path;
                    BrowserHelper.OpenUrl(url);
                }
                break;
            }
            case "bsale":
            {
                Dispose();
                ContentControlHost.Content = new SaleControl();
                break;
            }
            case "bservice":
            {
                Dispose();
                ContentControlHost.Content = new ServiceControlView();
                break;
            }
            case "bchecking":
            {
                Dispose();
                ContentControlHost.Content = new CheckingControl();
                break;
            }
        }
        
    }

    private string? GetHelpFilePath()
    {
        string path="";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Путь для установленного .deb пакета
            path= "/usr/share/observerLm/help.html";
        }
        else
        {
            // Для Windows (файл лежит рядом с .exe)
            path= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "help.html");
        }

        if (File.Exists(path))
        {
            return path;
        }
        else
        {
            MessageBoxManager.GetMessageBoxStandard("Ошибка", $"Файл справки не найден: '{path}'").ShowAsync();
            return null;
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