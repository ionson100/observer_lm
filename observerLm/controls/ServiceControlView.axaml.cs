using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.ViewModels.Commands;

namespace observerLm.controls
{
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
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Ошибка", 
                    $"Действие '{action}' для '{serviceName}' не удалось:\n{result.Error}",
                    ButtonEnum.Ok, Icon.Error);
                await box.ShowAsync();
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
        }

        // Стандартная реализация уведомления интерфейса об изменениях
        public new event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        protected void SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (Equals(field, value)) return;
            field = value;
            OnPropertyChanged(name);
        }

        private async void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            Button button = ((Button)sender)!;
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
    }
    
}


public static class ServiceManager
{
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public static async Task<(string Status, bool IsRunning)> GetStatusAsync(string serviceName)
    {
        string cmd = IsWindows ? "sc" : "systemctl";
        string args = IsWindows ? $"query {serviceName}" : $"is-active {serviceName}";

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
            bool isActive = output.Trim() == "active";
            return (isActive ? "Запущен" : "Остановлен", isActive);
        }
    }

    public static async Task<(bool Success, string Error)> SendCommandAsync(string action, string serviceName)
    {
        // Для Windows используем 'net', для Linux 'systemctl'
        string cmd = IsWindows ? "net" : "systemctl";
        string args = $"{action} {serviceName}";

        return await RunProcessWithResultAsync(cmd, args);
    }

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
            string error = await proc.StandardError.ReadToEndAsync();
            return (proc.ExitCode == 0, error);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }
}
