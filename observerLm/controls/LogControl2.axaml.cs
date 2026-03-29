using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using DynamicData;
using observerLm.controls.dialogs;


namespace observerLm.controls;

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

 
    private readonly CancellationTokenSource _cts1=new CancellationTokenSource();
    private readonly CancellationTokenSource _cts2=new CancellationTokenSource();

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
       Dispose();
        base.OnDetachedFromLogicalTree(e);
    }

    public LogControl2()
    {
        InitializeComponent();
        var mySettings = MySettings.Settings;
        ListBox1.ItemsSource = Lines1;
        ListBox2.ItemsSource = Lines2;
        

        if (string.IsNullOrWhiteSpace(mySettings.FolderLog))
        {
            Lines1.Add("Путь к папке логов пустой или отсутствует в настройках.");
            Lines2.Add(Lines1[0]);
            return;
        }
        
       
        FilePath1 = Path.Combine(mySettings.FolderLog,"regime.log");
        FilePath2 = Path.Combine(mySettings.FolderLog,"yenisei.log");
        
       
        DataContext = this;
        this.GetObservable(FilePathProperty1).Subscribe(path =>
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (File.Exists(path))
                {
                    Start(path,_cts1 ,Lines1,ListBox1,mySettings.Tail);
                }
                else
                {
                    Lines1.Add($"Файл лога не найден {path}");
                }
                
            }
                
        });

        this.GetObservable(FilePathProperty2).Subscribe(path =>
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (File.Exists(path))
                {
                    Start(path,_cts2 ,Lines2,ListBox2,mySettings.Tail);
                }
                else
                {
                    Lines2.Add($"Файл лога не найден {path}");
                }
            }
        });
    }
    private static void Start(string path,CancellationTokenSource?  cts,ObservableCollection<string> lines,ListBox listBox,int tail)
    {
        var service = new LogTailService(path);
       
        service.OnLines += line =>
        {
            Dispatcher.UIThread.Post(async void () =>
            {
                try
                {
                    lines.AddRange(line);

                    if (lines.Count > 2000)
                        lines.RemoveAt(0);

                    if (listBox.ItemCount > 0)
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            listBox.ScrollIntoView(lines[^1]);
               
                        });

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
               
              
             

            });
        };

        if (cts != null) service.Start(cts.Token, tail);
    }




    public void Dispose()
    {
        
        _cts1.Cancel();
        _cts2.Cancel();
     
        _cts1.Dispose();
        _cts2.Dispose();
        
      
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
                var textToCopy = string.Join(Environment.NewLine, lines); 
                await clipboard.SetTextAsync(textToCopy);
            }
            if (ListBox2.SelectedItems is { Count: > 0 } && clipboard!=null)
            {
             
                var lines = ListBox2.SelectedItems.Cast<object>().Select(item => item.ToString());
                var textToCopy = string.Join(Environment.NewLine, lines);
                await clipboard.SetTextAsync(textToCopy);
             
            }
        }
        catch (Exception ex)
        {
            await MessageDialog.Show("Ошибка", ex.Message);

        }
    }

    private void Thumb_OnDragCompleted(object? sender, VectorEventArgs e)
    {
        ListBox1.ScrollIntoView(Lines1[^1]);
        ListBox2.ScrollIntoView(Lines2[^1]);
    }
}