using MedicalShopDiaShop.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MedicalShopDiaShop.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для OrderBasketPage.xaml
    /// </summary>
    public partial class OrderBasketPage : Page
    {
        List<Order> _orders = App.context.Order.ToList();
        List<ProductOrder> _productOrders = App.context.ProductOrder.ToList();
        Order order = new Order();
        public OrderBasketPage()
        {
            InitializeComponent();

            order = _orders.FirstOrDefault(o => o.UserId == App.currentUser.Id);

            ProductLb.ItemsSource = _productOrders.Where(pO => pO.OrderId == order.Id);
            TotalCostTbl.DataContext = order;

        }

        private void DeletBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PlusBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MinusBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
