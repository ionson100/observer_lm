using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

using observerLm.controls;
using observerLm.controls.dialogs;

namespace observerLm;

public partial class MainWindow : Window
{
    public static MainWindow? Instance { get; private set; }
    public MainWindow()
    {
        InitializeComponent();
        Instance = this;
        ContentControlHost.Content = new LogControl2();
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        Button button = (Button)sender!;
        if(button.Tag==null)return;
        button.Focus(NavigationMethod.Tab);
        switch (button.Tag.ToString())
        {
            case "b1":
            {
                var dialog = await MessageDialog.Show("Внимание", "Произвести инициализацию локального модуля?",
                    DialogType.Confirmation);
              
                if (dialog)
                {
                    LoadingBar.IsVisible=true;
                  
                    try
                    {
                        await new MyStatusInit().RequestInitAsync((s,sr) =>
                        {
                            if (s != null && sr != null)
                            { 
                                ContentControlHost.Content = new StatusControl(s,sr);
                            }
                            
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
               
                try
                {
                    await new MyStatusInit().RequestPiotAsync("status",(s,sr) =>
                    {
                        if (s != null && sr != null)
                        { 
                            ContentControlHost.Content = new StatusControl(s,sr);
                        }
                       
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
                try
                {
                    await new MyStatusInit().RequestPiotAsync("stats", (s, sr) =>
                    {
                        if (s != null && sr != null)
                        { 
                            ContentControlHost.Content = new StatusControl(s, sr);
                        }
                       
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
             
                try
                {
                    await new MyStatusInit().RequestPiotAsync("config", (s, sr) =>
                    {
                        if (s != null && sr != null)
                        { 
                            ContentControlHost.Content = new StatusControl(s, sr);
                        }
                       
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
                ContentControlHost.Content=new LogControl2();
                break;
            }
            case "b4":
            {
#pragma warning disable CS0618 // Type or member is obsolete
                ContentControlHost.Content = new SettingsControl();
#pragma warning restore CS0618 // Type or member is obsolete
                break;
            }
            case "bhelp":
            {
                string ?path=await GetHelpFilePath();
                if (!string.IsNullOrEmpty(path))
                {
                    string url = "file://" + path;
                    BrowserHelper.OpenUrl(url);
                }
                break;
            }
            case "bsale":
            {
                ContentControlHost.Content = new SaleControl();
                break;
            }
            case "bservice":
            {
                ContentControlHost.Content = new ServiceControlView();
                break;
            }
            case "bchecking":
            {
              ContentControlHost.Content = new CheckingControl();
                break;
            }
        }
        
    }

    private async Task<string?> GetHelpFilePath()
    {
        string path;
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
            await MessageDialog.Show("Ошибка", $"Файл справки не найден: '{path}'");
         
            return null;
        }
    }

  
}