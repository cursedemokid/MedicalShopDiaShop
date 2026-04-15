using MedicalShopDiaShop.Database;
using MedicalShopDiaShop.AppData;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using static MedicalShopDiaShop.AppData.Status;

namespace MedicalShopDiaShop.MainView
{
    public partial class OrderDetailsWindow : Window
    {
        private readonly int _orderId;

        public OrderDetailsWindow(int orderId)
        {
            InitializeComponent();
            _orderId = orderId;
            LoadOrderDetails();
        }

        private void LoadOrderDetails()
        {
            using (var context = new DiaShopEntities3())
            {
                var order = context.Order.FirstOrDefault(o => o.Id == _orderId);
                if (order == null)
                {
                    FeedbackService.Error("Заказ не найден.");
                    Close();
                    return;
                }

                // Заголовок
                TitleText.Text = $"Заказ №{order.Id}";

                // Основная информация
                DateText.Text = order.DateTime.ToString("dd.MM.yyyy HH:mm");

                var client = context.User.FirstOrDefault(u => u.Id == order.ClientId);
                ClientText.Text = client != null ? $"{client.LastName} {client.FirstName}" : "Неизвестно";

                var worker = context.User.FirstOrDefault(u => u.Id == order.WorkerId);
                WorkerText.Text = worker != null ? $"{worker.LastName} {worker.FirstName}" : "Неизвестно";

                string courierName = "Не назначен";
                if (order.DeliveryId.HasValue)
                {
                    var delivery = context.Delivery.FirstOrDefault(d => d.Id == order.DeliveryId);
                    if (delivery != null)
                    {
                        var courier = context.User.FirstOrDefault(u => u.Id == delivery.CourierId);
                        courierName = courier != null ? $"{courier.LastName} {courier.FirstName}" : "Неизвестен";
                    }
                }
                CourierText.Text = courierName;

                StatusText.Text = GetOrderStatusName(order.Status);
                TotalText.Text = $"{order.TotalCost:N2} ₽";

                // Товары
                var products = context.ProductOrder
                    .Where(po => po.OrderId == order.Id)
                    .Select(po => new OrderProductItem
                    {
                        Name = po.Product.Name,
                        Quantity = po.Quantity,
                        PricePerUnit = po.Price,
                        TotalPrice = po.Quantity * po.Price,
                        Image = string.IsNullOrEmpty(po.Product.Image) ? "/Resources/placeholder.png" : po.Product.Image
                    }).ToList();

                ProductsItemsControl.ItemsSource = new ObservableCollection<OrderProductItem>(products);

                // История статусов
                var history = context.OrderHistory
                    .Where(oh => oh.OrderId == order.Id)
                    .OrderBy(oh => oh.UpdateAt)
                    .ToList();

                var historyItems = new ObservableCollection<OrderHistoryItem>();
                foreach (var item in history)
                {
                    string fromUser = "Система";
                    string toUser = "Система";
                    string transferText = "";

                    if (item.FromUserId.HasValue)
                    {
                        var from = context.User.FirstOrDefault(u => u.Id == item.FromUserId);
                        fromUser = from != null ? $"{from.LastName} {from.FirstName}" : "Неизвестно";
                    }
                    if (item.ToUserId.HasValue)
                    {
                        var to = context.User.FirstOrDefault(u => u.Id == item.ToUserId);
                        toUser = to != null ? $"{to.LastName} {to.FirstName}" : "Неизвестно";
                    }

                    if (item.FromUserId.HasValue && item.ToUserId.HasValue)
                        transferText = $"({fromUser} → {toUser})";
                    else if (item.FromUserId.HasValue)
                        transferText = $"(от {fromUser})";
                    else if (item.ToUserId.HasValue)
                        transferText = $"(к {toUser})";

                    historyItems.Add(new OrderHistoryItem
                    {
                        UpdateTime = item.UpdateAt,
                        OldStatusName = GetOrderStatusName(item.OldStatus),
                        NewStatusName = GetOrderStatusName(item.NewStatus),
                        UserTransferText = transferText
                    });
                }
                HistoryListBox.ItemsSource = historyItems;
            }
        }

        private string GetOrderStatusName(int statusId)
        {
            switch (statusId)
            {
                case (int)OrderStatus.InProcess: return "В обработке";
                case (int)OrderStatus.WaitCourier: return "Ожидает курьера";
                case (int)OrderStatus.Delivered: return "Доставлен";
                case (int)OrderStatus.WaitPayment: return "Ожидает оплаты";
                case (int)OrderStatus.InHistory: return "В истории";
                default: return "Неизвестно";
            }
        }
    }

    public class OrderProductItem
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal PricePerUnit { get; set; }
        public decimal TotalPrice { get; set; }
        public string Image { get; set; }
    }

    public class OrderHistoryItem
    {
        public DateTime UpdateTime { get; set; }
        public string OldStatusName { get; set; }
        public string NewStatusName { get; set; }
        public string UserTransferText { get; set; }
    }
}