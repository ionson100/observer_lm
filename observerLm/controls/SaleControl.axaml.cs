using System;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using observerLm.controls.controlSale;
using observerLm.controls.dialogs;
using static Avalonia.Application;

namespace observerLm.controls;

public partial class SaleControl : UserControl
{


    public SaleControl()
    {
       
        InitializeComponent();
        ContentControlHost.Content = new SaleInnerControl(SalesType.Sales);
    }

   

   

    [Obsolete("Obsolete")]
    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            Button button = (Button)sender!;
            if (button.Tag != null)
                switch (button.Tag.ToString())
                {
                    case "bsale":
                    {
                        ContentControlHost.Content = new SaleInnerControl(SalesType.Sales);
                        break;
                    }
                    case "bsalereturn":
                    {
                        ContentControlHost.Content = new SaleInnerControl(SalesType.ReturnSales);
                        break;
                    }
                    case "bsalecheck":
                    {
                        ContentControlHost.Content = new SaleInnerControl(SalesType.CheckSales);
                        break;
                    }
                    case "bsalelist":
                    {
                        CodeDialog dialog = new CodeDialog();
                        var topLevel = TopLevel.GetTopLevel(this);
                        if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                        {
                            var result = await dialog.ShowDialog<bool?>(desktop.MainWindow!);
                            if (result == true)
                            {
                                LoadingBar.IsVisible = true;
                                try
                                {
                                    await new MyStatusInit().RequestPiotAsync(
                                        $"cis/sold?skip={dialog.Skip}&limit={dialog.Limit}",
                                        (s, s1) => { ContentControlHost.Content = new StatusControl(s, s1); });
                                }
                                finally
                                {
                                    LoadingBar.IsVisible = false;
                                }
                            }
                        }
                       

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