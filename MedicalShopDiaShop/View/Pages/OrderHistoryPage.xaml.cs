using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using MedicalShopDiaShop.AppData;
using MedicalShopDiaShop.Model;
using static MedicalShopDiaShop.AppData.Status;

namespace MedicalShopDiaShop.View.Pages
{
    public partial class OrderHistoryPage : Page
    {
        public OrderHistoryPage()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void LoadOrders()
        {
            var historyOrders = App.context.Order
                .Where(o => o.UserId == App.currentUser.Id && o.Status == (int)OrderStatus.History)
                .ToList();

            var orderHistoryItems = new List<OrderHistoryItem>();

            foreach (var order in historyOrders)
            {
                var productOrders = App.context.ProductOrder
                    .Where(po => po.OrderId == order.Id)
                    .Select(po => new OrderProductItem
                    {
                        Product = po.Product,
                        Quantity = po.Quantity ?? 0
                    })
                    .ToList();

                orderHistoryItems.Add(new OrderHistoryItem
                {
                    Order = order,
                    Products = productOrders
                });
            }

            OrdersListBox.ItemsSource = orderHistoryItems;
        }
    }

    public class OrderProductItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public string ImagePath => Product?.Image;
        public string ProductName => Product?.Name;
    }

    public class OrderHistoryItem
    {
        public Order Order { get; set; }
        public List<OrderProductItem> Products { get; set; }

        public string OrderDate => Order?.Date.ToString("dd.MM.yyyy");
        public string DeliveryType => Order?.DeliveryType?.Name;
        public string PaymentType => Order?.PaymentType?.Name;
        public decimal TotalCost => Order?.TotalCost ?? 0;
    }
}