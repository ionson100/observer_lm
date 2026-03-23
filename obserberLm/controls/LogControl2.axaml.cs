using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace obserberLm.controls;

public partial class LogControl2 : UserControl,IDisposable
{
    public static readonly StyledProperty<string?> FilePathProperty =
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
        FilePath = "/var/log/syslog";//"regime/yenisei.log";
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

        _service.OnLine += line =>
        {
            if (_paused) return;

            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                Lines.Add(line);

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

    public event Action<string>? OnLine;

    public void Start(CancellationToken token)
    {
        Task.Run(async () =>
        {
            _position = new FileInfo(filePath).Length;

            while (!token.IsCancellationRequested)
            {
                await Task.Delay(300, token);

                var fi = new FileInfo(filePath);
                if (fi.Length < _position)
                    _position = 0;

                if (fi.Length > _position)
                {
                    await using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    fs.Seek(_position, SeekOrigin.Begin);

                    using var sr = new StreamReader(fs);

                    while (await sr.ReadLineAsync(token) is { } line)
                        OnLine?.Invoke(line);

                    _position = fs.Position;
                }
            }
        }, token);
    }
}