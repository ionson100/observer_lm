using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace observerLm.controls.controlSale;

public partial class SaleInnerControl : UserControl
{
    private readonly SalesType _salesType;

    public SaleInnerControl(SalesType salesType)
    {
        _salesType = salesType;
        InitializeComponent();
        switch (salesType)
        {
            case SalesType.Sales:
            {
                CheckButton.Content = "Продать товар";
                break;
            }
            case SalesType.ReturnSales:
            {
                CheckButton.Content = "Вернуть из продажи";
                break;
            }
            case SalesType.CheckSales:
            {
                CheckButton.Content = "Проверить на проданность";
                break;
            }
        }

        Loaded += (sender, args) =>
        {
            InputTextBox.Focus();
        };
    }

    private async void CheckButton_OnClick(object? sender, RoutedEventArgs e)
    {
          if (string.IsNullOrWhiteSpace(InputTextBox.Text))
          {
              string message = "";
              switch (_salesType)
              {
                  case SalesType.Sales:
                  {
                      message = "Введите код для продажи";
                      break;
                  }
                  case SalesType.ReturnSales:
                  {
                      message = "Введите код для возврата";
                      break;
                  }
                  case SalesType.CheckSales:
                  {
                      message = "Введите код для проверки";
                      break;
                  }
              }
              await MessageBoxManager.GetMessageBoxStandard("Предупреждение",   message).ShowAsync();
               
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