using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace observerLm;

internal static class Utils
{
    public static Window GetThis(Visual? visual)
    {
        var topLevel = TopLevel.GetTopLevel(visual);
        return (topLevel as Window)!;
    }
    public static Window? GetActiveWindow()
    {
        // Получаем активное окно
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }
}