using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace observerLm.controls;

public partial class NotificationControl : UserControl
{
    private DispatcherTimer? _timer;

    public static readonly StyledProperty<string> MessageProperty =
        AvaloniaProperty.Register<NotificationControl, string>(nameof(Message), string.Empty);

    public string Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public NotificationControl()
    {
        InitializeComponent();
        this.DataContext = this;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public void Show(string message)
    {
        Message = message;
        IsVisible = true;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsVisibleProperty && IsVisible)
        {
            _timer?.Stop();
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        _timer?.Stop();
        _timer = null; 
        IsVisible = false;
    }

    private void Close_Click(object? sender, RoutedEventArgs e)
    {
        _timer?.Stop();
        _timer = null;
        IsVisible = false;
    }
}