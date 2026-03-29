using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using observerLm.controls.dialogs;


namespace observerLm.controls
{
    /// <summary>
    /// Логика взаимодействия с ControlPanel.xaml
    /// </summary>
    public partial class ServiceControlView : UserControl, INotifyPropertyChanged
    {
          private string _regimeStatus = "Проверка...";
        private IBrush _regimeStatusColor = Brushes.Gray;

        // Поля для YENISEI
        private string _yeniseiStatus = "Проверка...";
        private IBrush _yeniseiStatusColor = Brushes.Gray;

        // Свойства для REGIME (привязка в XAML)
        public string RegimeStatus { get => _regimeStatus; set => SetField(ref _regimeStatus, value); }
        public IBrush RegimeStatusColor { get => _regimeStatusColor; set => SetField(ref _regimeStatusColor, value); }

        // Свойства для YENISEI (привязка в XAML)
        public string YeniseiStatus { get => _yeniseiStatus; set => SetField(ref _yeniseiStatus, value); }
        public IBrush YeniseiStatusColor { get => _yeniseiStatusColor; set => SetField(ref _yeniseiStatusColor, value); }

       

        public ServiceControlView()
        {
            InitializeComponent();
            DataContext = this;
            _ = UpdateLoop();
        }

        private async Task HandleServiceAction(string action, string serviceName)
        {
            var result = await ServiceManager.SendCommandAsync(action, serviceName);
            if (!result.Success)
            {
                await MessageDialog.Show("Ошибка",
                    $"Действие: '{action}' для '{serviceName}' не удалось:\n{result.Error} Возможно не хватает прав.");

            }
            await UpdateStatuses();
        }
       

        private async Task UpdateStatuses()
        {
            // Обновляем Regime
            var reg = await ServiceManager.GetStatusAsync("regime");
            RegimeStatus = reg.Status;
            RegimeStatusColor = reg.IsRunning ? Brushes.LimeGreen : Brushes.Red;

            // Обновляем Yenisei
            var yen = await ServiceManager.GetStatusAsync("yenisei");
            YeniseiStatus = yen.Status;
            YeniseiStatusColor = yen.IsRunning ? Brushes.LimeGreen : Brushes.Red;
        }

        private async Task UpdateLoop()
        {
            while (true)
            {
                await UpdateStatuses();
                await Task.Delay(3000); // Опрос раз в 3 секунды
            }
            // ReSharper disable once FunctionNeverReturns
        }

    
        public new event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private void SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (Equals(field, value)) return;
            field = value;
            OnPropertyChanged(name);
        }

        private async void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            try
            {
                var button = ((Button)sender!);
                if (button.Tag != null)
                    switch (button.Tag.ToString())
                    {
                        case "ystart":
                        {
                            await HandleServiceAction("start", "yenisei");
                            break;
                        }
                        case "ystop":
                        {
                            await HandleServiceAction("stop", "yenisei");
                            break;
                        }
                        case "rstart":
                        {
                            await HandleServiceAction("start", "regime");
                            break;
                        }
                        case "rstop":
                        {
                            await HandleServiceAction("stop", "regime");
                            break;
                        }
                    }
            }
            catch (Exception ex)
            {
                await MessageDialog.Show("Ошибка", ex.Message);
            }
        }
    }
    
}

/// <summary>
/// Класс управления службами
/// </summary>

public static class ServiceManager
{
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    /// <summary>
    /// Получить статус службы
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    public static async Task<(string Status, bool IsRunning)> GetStatusAsync(string serviceName)
    {
        var cmd = IsWindows ? "sc" : "systemctl";
        var args = IsWindows ? $"query {serviceName}" : $"is-active {serviceName}";

        var output = await RunProcessAsync(cmd, args);

        if (IsWindows)
        {
            if (output.Contains("RUNNING")) return ("Запущен", true);
            if (output.Contains("STOPPED")) return ("Остановлен", false);
            if (output.Contains("1060")) return ("Не найден", false);
            return ("Остановлен", false);
        }
        else // Linux
        {
            var isActive = output.Trim() == "active";
            return (isActive ? "Запущен" : "Остановлен", isActive);
        }
    }

    /// <summary>
    /// Отправляет комманду сервиса.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    public static async Task<(bool Success, string Error)> SendCommandAsync(string action, string serviceName)
    {
        // Для Windows используем 'net', для Linux 'systemctl'
        var cmd = IsWindows ? "net" : "systemctl";
        var args = $"{action} {serviceName}";

        return await RunProcessWithResultAsync(cmd, args);
    }

    /// <summary>
    /// Выполнение команды с выводом результатов на консоль
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    private static async Task<string> RunProcessAsync(string fileName, string args)
    {
        try
        {
            var psi = new ProcessStartInfo(fileName, args)
            {
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };
            using var proc = Process.Start(psi);
            return proc != null ? await proc.StandardOutput.ReadToEndAsync() : "";
        }
        catch { return ""; }
    }

    /// <summary>
    /// Выполняет команду и возвращает результат
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    private static async Task<(bool Success, string Error)> RunProcessWithResultAsync(string fileName, string args)
    {
        try
        {
            var psi = new ProcessStartInfo(fileName, args)
            {
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };
            using var proc = Process.Start(psi);
            if (proc == null) return (false, "Не удалось запустить процесс");
            
            await proc.WaitForExitAsync();
            var error = await proc.StandardError.ReadToEndAsync();
            return (proc.ExitCode == 0, error);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }
}
