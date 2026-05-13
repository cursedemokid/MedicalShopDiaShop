using System;
using System.Collections.Generic;
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
            Unloaded += OrdersPage_Unloaded;
        }

        private void OrdersPage_Loaded(object sender, RoutedEventArgs e)
        {
            DataRefreshHub.DataChanged -= OrdersPage_OnDataRefresh;
            DataRefreshHub.DataChanged += OrdersPage_OnDataRefresh;

            LoadOrdersFromDatabase();

            OrdersListView.ItemsSource = _filteredOrders;
            OrdersListBox.ItemsSource = _filteredOrders;

            OrdersListView.Visibility = Visibility.Visible;
            OrdersListBox.Visibility = Visibility.Collapsed;
            SetActiveButton(ListViewOnBtn);
        }

        private void OrdersPage_Unloaded(object sender, RoutedEventArgs e)
        {
            DataRefreshHub.DataChanged -= OrdersPage_OnDataRefresh;
        }

        private void OrdersPage_OnDataRefresh(object sender, EventArgs e)
        {
            if (!IsLoaded) return;
            LoadOrdersFromDatabase();
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
            }

            ApplyOrderFiltersFromUi();
        }

        private void ApplyOrderFiltersFromUi()
        {
            if (_allOrders == null) return;

            IEnumerable<OrderDto> query = _allOrders;

            if (StatusFilterComboBox.SelectedItem is ComboBoxItem selectedItem &&
                selectedItem.Tag is string tag &&
                int.TryParse(tag, out int selectedStatus) &&
                selectedStatus > 0)
            {
                query = query.Where(o => o.Status == selectedStatus);
            }

            string searchText = PageSearchBar.Text?.Trim().ToLower();
            if (!string.IsNullOrEmpty(searchText))
            {
                query = query.Where(o =>
                    o.Id.ToString().Contains(searchText) ||
                    (o.ClientFullName?.ToLower().Contains(searchText) ?? false) ||
                    (o.WorkerFullName?.ToLower().Contains(searchText) ?? false) ||
                    (o.CourierFullName?.ToLower().Contains(searchText) ?? false));
            }

            _filteredOrders = new ObservableCollection<OrderDto>(query);
            SubscribeToOrderChanges(_filteredOrders);
            RefreshOrdersList();
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
            if (combo?.SelectedItem == null || e.AddedItems == null || e.AddedItems.Count == 0) return;
            if (!(combo.Tag is int orderId)) return;

            // ItemsSource — строки из StatusList; не использовать orderDto.Status как «старое»:
            // при TwoWay к StatusString привязка успевает обновить DTO до этого обработчика,
            // из‑за чего oldStatus == newStatus и сохранение в БД не выполнялось.
            string newStatusText = combo.SelectedItem as string ?? combo.SelectedItem.ToString();
            int newStatus = GetStatusValue(newStatusText);

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
                        CourierId = 8,
                        Description = $"Доставка заказа №{order.Id}"
                    };
                    context.Delivery.Add(delivery);
                    await context.SaveChangesAsync();
                    order.DeliveryId = delivery.Id;
                }

                await context.SaveChangesAsync();
            }

            DataRefreshHub.Notify();
            FeedbackService.Information($"Статус заказа №{orderId} изменён на {newStatusText}");
        }

        private int GetStatusValue(string statusText)
        {
            switch (statusText)
            {
                case "В обработке": return (int)OrderStatus.InProcess;
                case "Ожидает курьера": return (int)OrderStatus.WaitCourier;
                case "Доставлен": return (int)OrderStatus.Delivered;
                case "Ожидает оплаты": return (int)OrderStatus.WaitPayment;
                case "В истории": return (int)OrderStatus.InHistory;
                case "У курьера": return (int)OrderStatus.InDelive;
                default: return (int)OrderStatus.InProcess;
            }
        }

        #endregion

        #region Фильтр, поиск, обновление

        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyOrderFiltersFromUi();
        }

        private void PageSearchBar_FilterTextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyOrderFiltersFromUi();
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            ApplyOrderFiltersFromUi();
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
            new AddEditOrderWindow().ShowDialog();
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelectedOrder();
            if (selected == null)
            {
                FeedbackService.Warning("Выберите заказ для редактирования.");
                return;
            }
            new AddEditOrderWindow(selected.Id).ShowDialog();
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
                DataRefreshHub.Notify();
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