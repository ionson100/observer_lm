using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace observerLm.controls;

public partial class LogControl : UserControl ,IDisposable
{
    private ObservableCollection<string> LogLines1 { get; } = new();
    private ObservableCollection<string> LogLines2 { get; } = new();


    public LogControl()
    {
        InitializeComponent();
         var mySettings = MySettings.GetSettings();
        ListBox1.ItemsSource=LogLines1;
        ListBox2.ItemsSource=LogLines2;
        if (mySettings==null)
        {
            return;
        }

        bool isRun = true;
        string filePath1 = Path.Combine(mySettings.FolderLog, "regime.log");
        string filePath2 = Path.Combine(mySettings.FolderLog, "yenisei.log");
        if (!File.Exists(filePath1))
        {
            LogLines1.Add($"Файл логов: {filePath1} не найден");
            isRun = false;
        }
        if (!File.Exists(filePath2))
        {
            LogLines2.Add($"Файл логов: {filePath2} не найден");
            isRun = false;
        }
        if(isRun==false)return;
        DispatcherTimer.Run(() =>
        {
            // Запускаем мониторинг
            {
                UpdateLog(filePath1,this.LogLines1,this.ListBox1);
                UpdateLog(filePath2,this.LogLines2,this.ListBox2);
            }
            return true;
        }, TimeSpan.FromSeconds(2));
        
        
      
        
    }
    
    private void UpdateLog(string path, ObservableCollection<string> targetCollection,ListBox listbox)
    {
        if (!File.Exists(path)) return;

        try
        {
            // Читаем последние 100 строк
            // Используем FileStream с FileShare.ReadWrite, чтобы не блокировать запись в лог
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fs, Encoding.UTF8);

            var lines = File.ReadLines(path).TakeLast(100).ToList();

            // Обновляем коллекцию только если данные изменились
            if (!lines.SequenceEqual(targetCollection))
            {
                targetCollection.Clear();
                foreach (var line in lines) targetCollection.Add(line);
                listbox.ScrollIntoView(lines.Last());
            }
            listbox.ScrollIntoView(lines.Last());
        }
        catch (Exception ex)
        {
            MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Ошибка доступа к файлу.{Environment.NewLine}{ex.Message}").ShowAsync();
            /* Ошибка доступа к файлу */
        }
    }
    
    
    
   

    
  

    private async void CopyMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            
            var lifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            var clipboard = lifetime?.MainWindow?.Clipboard;

            if (ListBox1.SelectedItems is { Count: > 0 } && clipboard!=null)
            { 
                var lines = ListBox1.SelectedItems.Cast<object>().Select(item => item.ToString()); 
                string textToCopy = string.Join(Environment.NewLine, lines); 
                await clipboard.SetTextAsync(textToCopy);
            }
            if (ListBox2.SelectedItems is { Count: > 0 } && clipboard!=null)
            {
             
                var lines = ListBox2.SelectedItems.Cast<object>().Select(item => item.ToString());
                string textToCopy = string.Join(Environment.NewLine, lines);
                await clipboard.SetTextAsync(textToCopy);
             
            }
        }
        catch (Exception ex)
        {
            MessageBoxManager.GetMessageBoxStandard("Ошибка копирования", ex.Message,  ButtonEnum.Ok);
        }
    }

    public void Dispose()
    {
        LogLines1.Clear();
        LogLines2.Clear();
    }
}