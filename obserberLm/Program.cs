using Avalonia;
using System;
using System.Threading.Tasks;
using obserberLm.controls.dialogs;

namespace obserberLm;

class Program
{
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

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            LogException(ex, "Main Loop");
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
                var errorWin = new ErrorWindow_(fullMessage);
            
                // Пытаемся найти главное окно, чтобы заблокировать его (Modal)
                var lifetime = Avalonia.Application.Current?.ApplicationLifetime 
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
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}