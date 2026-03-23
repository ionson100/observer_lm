using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DynamicData;

namespace obserberLm.controls;

public partial class LogControl2 : UserControl,IDisposable
{
    private static readonly StyledProperty<string?> FilePathProperty =
        AvaloniaProperty.Register<LogControl2, string?>(nameof(FilePath));

    public string? FilePath
    {
        get => GetValue(FilePathProperty);
        set => SetValue(FilePathProperty, value);
    }

    public ObservableCollection<string> Lines { get; } = new();

    private LogTailService? _service;
    private CancellationTokenSource? _cts;
    private bool _autoScroll = true;
    private bool _paused;
    public LogControl2()
    {
        InitializeComponent();
        FilePath = "C:\\Program Files\\Regime\\var\\log\\yenisei.log";//"regime/yenisei.log";
        List.ItemsSource = Lines;
        DataContext = this;

        this.GetObservable(FilePathProperty).Subscribe(path =>
        {
            if (!string.IsNullOrEmpty(path))
                Start(path);
        });
    }
    private void Start(string path)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        _service = new LogTailService(path);

        _service.OnLines += line =>
        {
            if (_paused) return;

            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                Lines.AddRange(line);

                if (Lines.Count > 2000)
                    Lines.RemoveAt(0);

                if (_autoScroll && List.ItemCount > 0)
                    List.ScrollIntoView(Lines[^1]);
            });
        };

        _service.Start(_cts.Token);
    }


    private void OnPauseClick(object? sender, RoutedEventArgs e)
    {
        _paused = !_paused;
    }

    private void OnClearClick(object? sender, RoutedEventArgs e)
    {
        Lines.Clear();
    }

    public void Dispose()
    {
        _cts?.Cancel();
     
        _cts?.Dispose();
      
    }
}
public class LogTailService(string filePath)
{
    private long _position;

    public event Action<List<string>>? OnLines;

    public void Start(CancellationToken token)
    {
        Task.Run(async () =>
        {
            _position = GetPositionForLastLines(filePath, 100);

            while (!token.IsCancellationRequested)
            {
                await Task.Delay(300, token);

                var fi = new FileInfo(filePath);
                if (fi.Length < _position)
                    _position = 0;
                

                if (fi.Length > _position)
                {
                    var newLines = new List<string>(); // Буфер для пачки строк

                    await using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    fs.Seek(_position, SeekOrigin.Begin);
                    using var sr = new StreamReader(fs);

                    while (await sr.ReadLineAsync(token) is { } line)
                    {
                        newLines.Add(line);
                    }

                    // 2. Вызываем событие один раз для всего списка (если есть данные)
                    if (newLines.Count > 0)
                    {
                        OnLines?.Invoke(newLines);
                    }

                    _position = fs.Position;

                    _position = fs.Position;
                }
            }
        }, token);
    }
    private long GetPositionForLastLines(string path, int lineCount)
    {
        var fi = new FileInfo(path);
        if (!fi.Exists || fi.Length == 0) return 0;

        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var sr = new StreamReader(fs);
    
        // Простая стратегия: читаем файл с конца по частям (буферами), считая переносы строк
        var lines = new List<long>();
        fs.Seek(0, SeekOrigin.Begin);
    
        // Чтобы не перегружать память, если файл огромный, 
        // в идеале нужно читать файл с конца. 
        // Но для логов проще всего быстро пробежаться по индексам строк:
        long pos = 0;
        while (sr.ReadLine() != null)
        {
            lines.Add(pos);
            pos = fs.Position;
        }

        return lines.Count <= lineCount ? 0 : lines[^lineCount];
    }
}