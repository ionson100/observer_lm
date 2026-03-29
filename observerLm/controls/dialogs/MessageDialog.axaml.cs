using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace observerLm.controls.dialogs;

public partial class MessageDialog : Window
{

    private DialogType _dialogType;

    public static async Task<bool> Show(string title, string message, DialogType type = DialogType.Error)
    {
        var res= await new MessageDialog(title, message, type).ShowDialog<bool>(Utils.GetActiveWindow()!);
        return res;
    }

    private MessageDialog(string title, string message, DialogType typ)
    {
        InitializeComponent();

        Loaded += (_, _) =>
        {
            TextBlockTitle.Text = title;
            _dialogType = typ;
            TextBlockMessage.Text = message;
            
            PathCore.Data=Geometry.Parse(GetIconData());
            PathCore.Classes.Clear();
            PathCore.Classes.Add(GetClassStyle());
            
            ButtonClose.Focus(NavigationMethod.Tab);
            if (this._dialogType == DialogType.Confirmation)
            {
                ButtonClose1.IsVisible = true;
                ButtonClose.Content = "Нет";
            }
            MainWindow.Instance!.PanelBlur.IsVisible = true;
           


        };
        Closed+=(_,_)=>MainWindow.Instance!.PanelBlur.IsVisible=false;
        TextBlockMessage.SizeChanged += OnMessageTextBlockSizeChanged;
       

    }
    private void OnMessageTextBlockSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        // Получаем новую высоту текста
        var textHeight = e.NewSize.Height;
        
        // Вычисляем новую высоту окна
        var newHeight = CalculateWindowHeight(textHeight);
        
        // Изменяем высоту окна
        Height = newHeight;
       
        this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }
    private double CalculateWindowHeight(double textHeight)
    {
        // Базовые отступы: заголовок + иконка + кнопка + отступы
        const double titleHeight = 40;      // Высота заголовка
        const double buttonHeight = 50;     // Высота кнопки + отступы
        const double padding = 50;          // Внутренние отступы
        
        var totalHeight = titleHeight + textHeight + buttonHeight + padding;
        
        // Ограничиваем минимальную и максимальную высоту
        return  Math.Min(Math.Max(totalHeight, MinHeight), MaxHeight);
    }








    private string GetIconData()
    {
        return _dialogType switch
        {
            DialogType.Error =>
                "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 4c1.1 0 2 .9 2 2v5c0 1.1-.9 2-2 2s-2-.9-2-2V8c0-1.1.9-2 2-2zm0 11c-.83 0-1.5-.67-1.5-1.5S11.17 14 12 14s1.5.67 1.5 1.5S12.83 17 12 17z",
            DialogType.Info =>
                "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-6h2v6zm0-8h-2V7h2v2z",
            DialogType.Success =>
                "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15h-2v-2h2v2zm0-4h-2V7h2v6z",
            DialogType.Warning => "M1 21h22L12 2 1 21zm12-3h-2v-2h2v2zm0-4h-2v-4h2v4z",
            DialogType.Confirmation =>
                "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 17h-2v-2h2v2zm2-7.5c0 .58-.29 1.08-.76 1.36-.31.19-.57.44-.74.74-.15.26-.24.55-.24.9h-2c0-.55.13-1.03.39-1.44.26-.41.63-.74 1.09-.98.32-.16.55-.44.67-.78.12-.34.09-.7-.1-1.02-.19-.32-.5-.55-.88-.66-.44-.12-.87-.05-1.23.21-.33.24-.57.61-.66 1.03l-1.86-.4c.17-.83.66-1.55 1.38-2.01.77-.49 1.73-.69 2.67-.53.9.15 1.68.63 2.2 1.32.52.69.78 1.53.74 2.38.01.84-.28 1.64-.8 2.26z",
            _ => ""
        };
    }

    private string GetClassStyle()
    {
        return _dialogType switch
        {
            DialogType.Error => "error",
            DialogType.Info => "info",
            DialogType.Success => "success",
            DialogType.Warning => "warning",
            DialogType.Confirmation => "confirm",
            _ => "error"
        };
    }


    private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
           
                
            var button = (Button)sender!;
            switch (button!)
            {
                case { CommandParameter: "bt1" }:
                {
                    this.Close(true);
                    break;
                }
                case { CommandParameter: "bt2" }:
                {
                    this.Close(false);
                    break;
                }
                default:
                {
                    this.Close(false);
                    break;
                }
            }
           
           
        }

    private void Button_OnClick2(object? sender, RoutedEventArgs e)
    {
        this.Close(false);
    }
}


