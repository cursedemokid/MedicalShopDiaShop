using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using MedicalShopDiaShop.AppData;
using MedicalShopDiaShop.Database;
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

        private void LoadOrdersFromDatabase()
        {
            using (var context = new DiaShopEntities3())
            {
                var orders = context.Order
                    .OrderByDescending(o => o.DateTime)
                    .ToList();

                _allOrders = new ObservableCollection<OrderDto>(orders.Select(o => MapToOrderDto(o, context)));
                _filteredOrders = new ObservableCollection<OrderDto>(_allOrders);
            }
        }

        private OrderDto MapToOrderDto(Order order, DiaShopEntities3 context)
        {
            var client = context.User.FirstOrDefault(u => u.Id == order.ClientId);
            var worker = context.User.FirstOrDefault(u => u.Id == order.WorkerId);
            string courierName = null;
            if (order.DeliveryId.HasValue)
            {
                var delivery = context.Delivery.FirstOrDefault(d => d.Id == order.DeliveryId);
                if (delivery != null)
                {
                    var courier = context.User.FirstOrDefault(u => u.Id == delivery.CourierId);
                    courierName = courier != null ? $"{courier.LastName} {courier.FirstName}" : null;
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
                Status = order.Status,
                TotalCost = order.TotalCost,
                DeliveryType = order.DeliveryType,
                Items = items,
                IsSelected = false
            };
        }

        private void RefreshOrdersList()
        {
            OrdersListView.ItemsSource = null;
            OrdersListView.ItemsSource = _filteredOrders;
            OrdersListBox.ItemsSource = null;
            OrdersListBox.ItemsSource = _filteredOrders;
        }

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
            RefreshOrdersList();
        }

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
                using (var context = new DiaShopEntities3())
                {
                    var order = context.Order.FirstOrDefault(o => o.Id == selected.Id);
                    if (order != null)
                    {
                        var productOrders = context.ProductOrder.Where(po => po.OrderId == order.Id);
                        context.ProductOrder.RemoveRange(productOrders);
                        var history = context.OrderHistory.Where(oh => oh.OrderId == order.Id);
                        context.OrderHistory.RemoveRange(history);
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
    }
}