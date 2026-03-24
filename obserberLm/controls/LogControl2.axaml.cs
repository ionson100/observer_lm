using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Threading;
using DynamicData;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace obserberLm.controls;

public partial class LogControl2 : UserControl,IDisposable
{
    private static readonly StyledProperty<string?> FilePathProperty1 = AvaloniaProperty.Register<LogControl2, string?>(nameof(FilePath1));
    private static readonly StyledProperty<string?> FilePathProperty2 = AvaloniaProperty.Register<LogControl2, string?>(nameof(FilePath2));

    public string? FilePath1
    {
        get => GetValue(FilePathProperty1);
        set => SetValue(FilePathProperty1, value);
    }
    public string? FilePath2
    {
        get => GetValue(FilePathProperty2);
        set => SetValue(FilePathProperty2, value);
    }

    private ObservableCollection<string> Lines1 { get; } = new();
    private ObservableCollection<string> Lines2 { get; } = new();

    private LogTailService? _service1;
    private LogTailService? _service2;
    private CancellationTokenSource? _cts1;
    private CancellationTokenSource? _cts2;
    private readonly MySettings? _mySettings = MySettings.GetSettings();
    
   
    public LogControl2()
    {
        InitializeComponent();
        if(_mySettings==null) return;
       
        FilePath1 = System.IO.Path.Combine(_mySettings.FolderLog,"regime.log");
        FilePath2 = System.IO.Path.Combine(_mySettings.FolderLog,"yenisei.log");
        
        ListBox1.ItemsSource = Lines1;
        ListBox2.ItemsSource = Lines2;
        DataContext = this;
        this.GetObservable(FilePathProperty1).Subscribe(path =>
        {
            if (!string.IsNullOrEmpty(path))
                Start(path,_cts1! ,Lines1,_service1,ListBox1,_mySettings.Tail);
        });

        this.GetObservable(FilePathProperty2).Subscribe(path =>
        {
            if (!string.IsNullOrEmpty(path))
                Start(path,_cts2! ,Lines2,_service2,ListBox2,_mySettings.Tail);
        });
    }
    private static void Start(string path,CancellationTokenSource?  cts,ObservableCollection<string> lines,LogTailService? service,ListBox listBox,int tail)
    {
        // Task.Run(async () =>
        // {
        //     await Task.Delay(3000); // Ждем 3 секунды
        //
        //     // Возвращаемся в UI-поток для выполнения кода
        //     await Dispatcher.UIThread.InvokeAsync(() =>
        //     {
        //         listBox.ScrollIntoView(lines[^1]);
        //        
        //     });
        // });
        cts?.Cancel();
        cts = new CancellationTokenSource();

        service = new LogTailService(path);

        service.OnLines += line =>
        {
            

            Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
            {
                lines.AddRange(line);

                if (lines.Count > 2000)
                    lines.RemoveAt(0);

                if (listBox.ItemCount > 0)
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        listBox.ScrollIntoView(lines[^1]);
               
                    });
                    //listBox.ScrollIntoView(lines[^1]);
              
             

            });
        };

        service.Start(cts.Token,tail);
    }




    public void Dispose()
    {
        
        _cts1?.Cancel();
        _cts2?.Cancel();
     
        _cts1?.Dispose();
        _cts2?.Dispose();
        
      
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
}