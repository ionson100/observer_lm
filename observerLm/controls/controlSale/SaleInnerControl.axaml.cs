using Avalonia.Controls;
using Avalonia.Interactivity;
using observerLm.controls.dialogs;

namespace observerLm.controls.controlSale;

public partial class SaleInnerControl : UserControl
{
    private readonly SalesType _salesType;

    public SaleInnerControl(SalesType salesType)
    {
        _salesType = salesType;
        InitializeComponent();
        CheckButton.Content = salesType switch
        {
            SalesType.Sales => "Продать товар",
            SalesType.ReturnSales => "Вернуть из продажи",
            SalesType.CheckSales => "Проверить на проданность",
            _ => CheckButton.Content
        };

        Loaded += (_, _) =>
        {
            InputTextBox.Focus();
        };
    }

   

    private async void CheckButton_OnClick(object? sender, RoutedEventArgs e)
    {
          if (string.IsNullOrWhiteSpace(InputTextBox.Text))
          {
              var message = _salesType switch
              {
                  SalesType.Sales => "Введите код для продажи",
                  SalesType.ReturnSales => "Введите код для возврата",
                  SalesType.CheckSales => "Введите код для проверки",
                  _ => ""
              };

              await MessageDialog.Show("Ошибка", "Внимание! " + message);
            InputTextBox.Focus();
          
              return;

          }
          switch (_salesType)
          {
              case SalesType.Sales:
              {
                  LoadingBar.IsVisible=true;
                  try
                  {
                      await new MyStatusInit().RequestSellAsync("cis/sell", InputTextBox.Text.Trim(), (s, s1) =>
                      {
                          OutputTextBox.Text = s;
                          CurlControlCore.SetCurlText(s1);
                      });
                  }
                  finally
                  {
                      LoadingBar.IsVisible=false;
                  }
                   
                  break;
              }
              case SalesType.ReturnSales:
              {
                  LoadingBar.IsVisible=true;
                  try
                  {
                      await new MyStatusInit().RequestSellAsync("cis/return", InputTextBox.Text.Trim(), (s, s1) =>
                      {
                          OutputTextBox.Text = s;
                          CurlControlCore.SetCurlText(s1);
                      });
                  }
                  finally
                  {
                      LoadingBar.IsVisible=false;
                  }
                    
                  break;
              }//
              case SalesType.CheckSales:
              {
                  LoadingBar.IsVisible=true;
                  try
                  {
                      await new MyStatusInit().RequestSellAsync("cis/sold/check", InputTextBox.Text.Trim(), (s, s1) =>
                      {
                          OutputTextBox.Text = s;
                          CurlControlCore.SetCurlText(s1);
                      });

                  }
                  finally
                  {
                      LoadingBar.IsVisible=false;
                  }
                    
                  break;
              }
          }

    }
}