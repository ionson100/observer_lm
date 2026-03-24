using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using obserberLm.controls.controlSale;

namespace obserberLm.controls;

public partial class SaleControl : UserControl
{


    public SaleControl()
    {
       
        InitializeComponent();
        ContentControlHost.Content = new SaleInnerControl(SalesType.Sales);
    }

   

   

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        Button button = (Button)sender;
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
        }
        
    
    }
}