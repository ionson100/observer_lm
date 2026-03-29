using Avalonia;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using observerLm.controls.dialogs;
using Newtonsoft.Json;

namespace observerLm;

class Program
{
    //dotnet publish -r linux-x64 -c Release --self-contained true -p:PublishSingleFile=true
    
// # Очистка и восстановление
//     dotnet restore -r linux-x64
//
// # Сборка DEB-пакета
//         dotnet msbuild ./observerLm/observerLm.csproj /t:CreateDeb /p:TargetFramework=net9.0 /p:RuntimeIdentifier=linux-x64 /p:Configuration=Release
//         Пакет будет создан в папке:
//
//     text
//         bin/Release/net9.0/linux-x64/yourapp_1.0.0_amd64.deb
//dpkg-deb --build observerlm-deb "observerlm-deb_$(grep '^Version:' observerlm-deb/DEBIAN/control | awk '{print $2}').deb"
// 
//dpkg-deb --build observerlm-deb "observerlm-deb_$(grep '^Version:' observerlm-deb/DEBIAN/control | awk '{print $2}').deb"
    
    
    private static bool _isShowingError = false;
    [STAThread]
    public static void Main(string[] args)
    {
        // 1. Ошибки в синхронном коде и основном потоке
        AppDomain.CurrentDomain.UnhandledException += (sender, e) => 
        {
            LogException(e.ExceptionObject as Exception, "AppDomain");
        };

        // 2. Ошибки в асинхронных задачах (Task)
        TaskScheduler.UnobservedTaskException += (sender, e) => 
        {
            LogException(e.Exception, "TaskScheduler");
            e.SetObserved(); // предотвращает падение приложения, если это возможно
        };
        SettingsCreator();
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            LogException(ex, "Main Loop");
        }
        
    }

    private static void SettingsCreator()
    {
        string configDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "observerLm" // Имя вашей программы
        );
        if (!Directory.Exists(configDir))
        {
            Directory.CreateDirectory(configDir);
        }
        string filePath = Path.Combine(configDir, "settings.json");
        if (!File.Exists(filePath))
        {
            MySettings mySettings = new MySettings
            {
                Auth = "YWRtaW46YWRtaW4=",
                Url = "http://localhost:5995/api/v2/",
                Token = "97486293-646b-463e-8199-48c37e36d605",
                Tail = 100
            };
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
               mySettings.FolderLog ="C:\\Program Files\\Regime\\var\\log";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                mySettings.FolderLog ="/var/log/regime";
            }
            string jsonString = JsonConvert.SerializeObject(mySettings, Formatting.Indented);
            File.WriteAllText(filePath, jsonString);
        }
    }

    private static void LogException(Exception? ex, string source)
    {
        string fullMessage = $"[{source}] {ex?.GetType().Name}: {ex?.Message}\n{ex?.StackTrace}";
        Console.WriteLine(fullMessage);

        // Вызываем UI поток
        Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
        {
            if (_isShowingError) return;
            _isShowingError = true;

            try
            {
                var errorWin = new ErrorWindow(fullMessage);

                // Пытаемся найти главное окно, чтобы заблокировать его (Modal)
                var lifetime = Application.Current?.ApplicationLifetime
                    as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;

                if (lifetime?.MainWindow != null)
                    await errorWin.ShowDialog(lifetime.MainWindow);
                else
                    errorWin.Show(); // Если главного окна еще нет (ошибка при старте)
            }
            catch (Exception fatal)
            {
                // Если даже окно ошибки упало, просто пишем в консоль
                Console.WriteLine("Fatal error displaying error window: " + fatal.Message);
            }
            finally
            {
                _isShowingError = false;
            }
        });
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            
            .WithInterFont()
            .With(new X11PlatformOptions 
            { 
                // Отключаем использование DBus для меню
                UseDBusMenu = false 
            })
            .UsePlatformDetect()
            .LogToTrace();
}