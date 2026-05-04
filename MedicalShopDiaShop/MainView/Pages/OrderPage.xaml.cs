using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using MedicalShopDiaShop.AppData;
using MedicalShopDiaShop.Database;
using MedicalShopDiaShop.MainView.Dto;
using static MedicalShopDiaShop.AppData.Status;

namespace MedicalShopDiaShop.MainView.Pages
{
    public partial class OrdersPage : Page
    {
        private ObservableCollection<OrderDto> _allOrders;
        private ObservableCollection<OrderDto> _filteredOrders;

        public OrdersPage()
        {
            InitializeComponent();
            Loaded += OrdersPage_Loaded;
        }

        private void OrdersPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadOrdersFromDatabase();

            OrdersListView.ItemsSource = _filteredOrders;
            OrdersListBox.ItemsSource = _filteredOrders;

            OrdersListView.Visibility = Visibility.Visible;
            OrdersListBox.Visibility = Visibility.Collapsed;
            SetActiveButton(ListViewOnBtn);
        }

        #region Загрузка данных из БД

        private void LoadOrdersFromDatabase()
        {
            using (var context = new DiaShopEntities())
            {
                var orders = context.Order
                    .OrderByDescending(o => o.DateTime)
                    .ToList();

                _allOrders = new ObservableCollection<OrderDto>(orders.Select(o => MapToOrderDto(o, context)));
                _filteredOrders = new ObservableCollection<OrderDto>(_allOrders);
                SubscribeToOrderChanges(_filteredOrders);
            }
        }

        private OrderDto MapToOrderDto(Order order, DiaShopEntities context)
        {
            var client = context.User.FirstOrDefault(u => u.Id == order.ClientId);
            var worker = context.User.FirstOrDefault(u => u.Id == order.WorkerId);
            string courierName = null;
            DateTime? deliveryStart = null;
            DateTime? deliveryEnd = null;
            string deliveryDescription = null;

            if (order.DeliveryId.HasValue)
            {
                var delivery = context.Delivery.FirstOrDefault(d => d.Id == order.DeliveryId);
                if (delivery != null)
                {
                    var courier = context.User.FirstOrDefault(u => u.Id == delivery.CourierId);
                    courierName = courier != null ? $"{courier.LastName} {courier.FirstName}" : null;
                    deliveryStart = delivery.StartDate;
                    deliveryEnd = delivery.EndDate;
                    deliveryDescription = delivery.Description;
                }
            }

            var items = context.ProductOrder
                .Where(po => po.OrderId == order.Id)
                .Select(po => new OrderItemDto
                {
                    ProductName = po.Product.Name,
                    Quantity = po.Quantity,
                    PricePerUnit = po.Price,
                    ImagePath = string.IsNullOrEmpty(po.Product.Image) ? "/Resources/placeholder.png" : po.Product.Image
                }).ToList();

            return new OrderDto
            {
                Id = order.Id,
                OrderDate = order.DateTime,
                ClientFullName = client != null ? $"{client.LastName} {client.FirstName}" : "Неизвестно",
                WorkerFullName = worker != null ? $"{worker.LastName} {worker.FirstName}" : "Неизвестно",
                CourierFullName = courierName,
                DeliveryStartDate = deliveryStart,
                DeliveryEndDate = deliveryEnd,
                DeliveryDescription = deliveryDescription,
                Status = order.Status ?? 1, // если NULL, считаем "В обработке"
                TotalCost = order.TotalCost,
                DeliveryType = order.DeliveryType ?? 1,
                Items = items,
                IsSelected = false
            };
        }

        #endregion

        #region Единичное выделение

        private void SubscribeToOrderChanges(ObservableCollection<OrderDto> orders)
        {
            foreach (var o in orders)
                o.PropertyChanged += OnOrderPropertyChanged;
            orders.CollectionChanged += OnOrdersCollectionChanged;
        }

        private void OnOrdersCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (OrderDto o in e.NewItems)
                    o.PropertyChanged += OnOrderPropertyChanged;
            if (e.OldItems != null)
                foreach (OrderDto o in e.OldItems)
                    o.PropertyChanged -= OnOrderPropertyChanged;
        }

        private void OnOrderPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OrderDto.IsSelected))
            {
                var changed = sender as OrderDto;
                if (changed != null && changed.IsSelected)
                {
                    foreach (var o in _filteredOrders)
                        if (o != changed && o.IsSelected)
                            o.IsSelected = false;
                }
            }
        }

        #endregion

        #region Изменение статуса заказа

        private async void OrderStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo == null || combo.SelectedItem == null) return;

            int orderId = (int)combo.Tag;
            int newStatus = int.Parse(((ComboBoxItem)combo.SelectedItem).Tag.ToString());

            using (var context = new DiaShopEntities())
            {
                var order = context.Order.FirstOrDefault(o => o.Id == orderId);
                if (order == null) return;

                int oldStatus = order.Status ?? 1;
                if (oldStatus == newStatus) return;

                order.Status = newStatus;

                context.OrderHistory.Add(new OrderHistory
                {
                    OrderId = order.Id,
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    UpdateAt = DateTime.Now,
                    FromUserId = App.currentUser.Id,
                    ToUserId = null
                });

                if (newStatus == (int)OrderStatus.WaitCourier && !order.DeliveryId.HasValue)
                {
                    var delivery = new Delivery
                    {
                        StartDate = DateTime.Now,
                        EndDate = DateTime.Now.AddDays(3),
                        CourierId = 8, // позже можно назначить
                        Description = $"Доставка заказа №{order.Id}"
                    };
                    context.Delivery.Add(delivery);
                    await context.SaveChangesAsync();
                    order.DeliveryId = delivery.Id;
                }

                await context.SaveChangesAsync();
            }

            var updatedOrder = _allOrders.FirstOrDefault(o => o.Id == orderId);
            if (updatedOrder != null) updatedOrder.Status = newStatus;
            RefreshOrdersList();

            FeedbackService.Information($"Статус заказа №{orderId} изменён на {GetStatusName(newStatus)}");
        }

        private string GetStatusName(int statusId)
        {
            switch (statusId)
            {
                case (int)OrderStatus.InProcess: return "В обработке";
                case (int)OrderStatus.WaitCourier: return "Ожидает курьера";
                case (int)OrderStatus.Delivered: return "Доставлен";
                case (int)OrderStatus.WaitPayment: return "Ожидает оплаты";
                case (int)OrderStatus.InHistory: return "В истории";
                case (int)OrderStatus.InDelive: return "У курьера";
                default: return "Неизвестно";
            }
        }

        #endregion

        #region Фильтр, поиск, обновление

        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StatusFilterComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string tag && int.TryParse(tag, out int selectedStatus))
            {
                _filteredOrders = new ObservableCollection<OrderDto>(_allOrders.Where(o => o.Status == selectedStatus));
            }
            else
            {
                _filteredOrders = new ObservableCollection<OrderDto>(_allOrders);
            }
            SubscribeToOrderChanges(_filteredOrders);
            RefreshOrdersList();
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchBox.Text?.Trim().ToLower();
            if (string.IsNullOrEmpty(searchText))
                _filteredOrders = new ObservableCollection<OrderDto>(_allOrders);
            else
            {
                _filteredOrders = new ObservableCollection<OrderDto>(_allOrders.Where(o =>
                    o.Id.ToString().Contains(searchText) ||
                    (o.ClientFullName?.ToLower().Contains(searchText) ?? false) ||
                    (o.WorkerFullName?.ToLower().Contains(searchText) ?? false) ||
                    (o.CourierFullName?.ToLower().Contains(searchText) ?? false)));
            }
            SubscribeToOrderChanges(_filteredOrders);
            RefreshOrdersList();
        }

        private void RefreshOrdersList()
        {
            OrdersListView.ItemsSource = null;
            OrdersListView.ItemsSource = _filteredOrders;
            OrdersListBox.ItemsSource = null;
            OrdersListBox.ItemsSource = _filteredOrders;
        }

        #endregion

        #region CRUD операции

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditOrderWindow();
            if (window.ShowDialog() == true)
                LoadOrdersFromDatabase();
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelectedOrder();
            if (selected == null)
            {
                FeedbackService.Warning("Выберите заказ для редактирования.");
                return;
            }
            var window = new AddEditOrderWindow(selected.Id);
            if (window.ShowDialog() == true)
                LoadOrdersFromDatabase();
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelectedOrder();
            if (selected == null)
            {
                FeedbackService.Warning("Выберите заказ для удаления.");
                return;
            }

            if (FeedbackService.Question($"Удалить заказ №{selected.Id}?") == MessageBoxResult.Yes)
            {
                using (var context = new DiaShopEntities())
                {
                    var order = context.Order.FirstOrDefault(o => o.Id == selected.Id);
                    if (order != null)
                    {
                        var productOrders = context.ProductOrder.Where(po => po.OrderId == order.Id);
                        context.ProductOrder.RemoveRange(productOrders);
                        var history = context.OrderHistory.Where(oh => oh.OrderId == order.Id);
                        context.OrderHistory.RemoveRange(history);
                        if (order.DeliveryId.HasValue)
                        {
                            var delivery = context.Delivery.Find(order.DeliveryId);
                            if (delivery != null) context.Delivery.Remove(delivery);
                        }
                        context.Order.Remove(order);
                        context.SaveChanges();
                    }
                }
                LoadOrdersFromDatabase();
                FeedbackService.Information("Заказ удалён.");
            }
        }

        private OrderDto GetSelectedOrder()
        {
            return _filteredOrders?.FirstOrDefault(o => o.IsSelected);
        }

        #endregion

        #region Переключение вида

        private void ListViewOnBtn_Click(object sender, RoutedEventArgs e)
        {
            OrdersListView.Visibility = Visibility.Visible;
            OrdersListBox.Visibility = Visibility.Collapsed;
            SetActiveButton(ListViewOnBtn);
        }

        private void ListBoxOnBtn_Click(object sender, RoutedEventArgs e)
        {
            OrdersListView.Visibility = Visibility.Collapsed;
            OrdersListBox.Visibility = Visibility.Visible;
            SetActiveButton(ListBoxOnBtn);
        }

        private void SetActiveButton(Button activeButton)
        {
            ListViewOnBtn.BorderBrush = Brushes.Transparent;
            ListViewOnBtn.BorderThickness = new Thickness(1);
            ListBoxOnBtn.BorderBrush = Brushes.Transparent;
            ListBoxOnBtn.BorderThickness = new Thickness(1);
            activeButton.BorderBrush = Brushes.Black;
            activeButton.BorderThickness = new Thickness(2);
        }

        #endregion

        #region Детали заказа

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var order = btn?.Tag as OrderDto;
            if (order != null)
            {
                var window = new OrderDetailsWindow(order.Id);
                window.ShowDialog();
            }
        }

        #endregion
    }
}