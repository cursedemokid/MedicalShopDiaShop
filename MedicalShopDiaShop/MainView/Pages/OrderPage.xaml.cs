using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

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
            _allOrders = new ObservableCollection<OrderDto>(GetStaticOrders());
            _filteredOrders = new ObservableCollection<OrderDto>(_allOrders);

            OrdersListView.ItemsSource = _filteredOrders;
            OrdersListBox.ItemsSource = _filteredOrders;

            OrdersListView.Visibility = Visibility.Visible;
            OrdersListBox.Visibility = Visibility.Collapsed;
            SetActiveButton(ListViewOnBtn);
        }

        private List<OrderDto> GetStaticOrders()
        {
            // Статические данные пользователей (клиенты, работники, курьеры)
            var clients = new Dictionary<int, string> { { 3, "Кузнецов Алексей" }, { 6, "Морозова Екатерина" } };
            var workers = new Dictionary<int, string> { { 1, "Иванов Иван" }, { 2, "Петрова Анна" } };
            var couriers = new Dictionary<int, string> { { 4, "Сидоров Алексей" }, { 5, "Козлов Дмитрий" } };

            // Товары для демонстрации
            var products = new Dictionary<int, (string name, string image, decimal price)>
            {
                { 1, ("Accu-Chek Active", "/Resources/ProductsImages/Accu-ChekProductImage.jpg", 1450) },
                { 2, ("Тест-полоски OneTouch", "/Resources/ProductsImages/OneTouchStripes.jpg", 950) },
                { 3, ("НовоПен 4", "/Resources/ProductsImages/Novopen.jpg", 850) },
                { 4, ("Крем для ног", "/Resources/ProductsImages/FootCream.jpg", 420) }
            };

            var orders = new List<OrderDto>
            {
                new OrderDto
                {
                    Id = 101,
                    OrderDate = DateTime.Now.AddDays(-5),
                    ClientFullName = clients[3],
                    WorkerFullName = workers[1],
                    CourierFullName = couriers[4],
                    DeliveryStartDate = DateTime.Now.AddDays(-3),
                    DeliveryEndDate = DateTime.Now.AddDays(-2),
                    DeliveryDescription = "Доставка по адресу: ул. Ленина, д.10",
                    Status = 3, // Доставлен
                    TotalCost = 2400,
                    DeliveryType = 1, // Курьер
                    Items = new List<OrderItemDto>
                    {
                        new OrderItemDto { ProductName = products[1].name, Quantity = 1, PricePerUnit = products[1].price, ImagePath = products[1].image },
                        new OrderItemDto { ProductName = products[2].name, Quantity = 2, PricePerUnit = products[2].price, ImagePath = products[2].image }
                    }
                },
                new OrderDto
                {
                    Id = 102,
                    OrderDate = DateTime.Now.AddDays(-2),
                    ClientFullName = clients[6],
                    WorkerFullName = workers[2],
                    CourierFullName = null, // ещё не назначен
                    DeliveryStartDate = null,
                    DeliveryEndDate = null,
                    DeliveryDescription = null,
                    Status = 2, // Ожидает курьера
                    TotalCost = 850,
                    DeliveryType = 1,
                    Items = new List<OrderItemDto>
                    {
                        new OrderItemDto { ProductName = products[3].name, Quantity = 1, PricePerUnit = products[3].price, ImagePath = products[3].image }
                    }
                },
                new OrderDto
                {
                    Id = 103,
                    OrderDate = DateTime.Now.AddDays(-1),
                    ClientFullName = clients[3],
                    WorkerFullName = workers[1],
                    CourierFullName = null,
                    DeliveryStartDate = null,
                    DeliveryEndDate = null,
                    DeliveryDescription = null,
                    Status = 1, // В обработке
                    TotalCost = 420,
                    DeliveryType = 2, // Самовывоз
                    Items = new List<OrderItemDto>
                    {
                        new OrderItemDto { ProductName = products[4].name, Quantity = 1, PricePerUnit = products[4].price, ImagePath = products[4].image }
                    }
                }
            };
            return orders;
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

        private void RefreshOrdersList()
        {
            OrdersListView.ItemsSource = null;
            OrdersListView.ItemsSource = _filteredOrders;
            OrdersListBox.ItemsSource = null;
            OrdersListBox.ItemsSource = _filteredOrders;
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Добавление заказа (будет реализовано)");
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelectedOrder();
            if (selected == null) MessageBox.Show("Выберите заказ.");
            else MessageBox.Show($"Изменить заказ №{selected.Id}");
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelectedOrder();
            if (selected == null) MessageBox.Show("Выберите заказ.");
            else if (MessageBox.Show($"Удалить заказ №{selected.Id}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _allOrders.Remove(selected);
                _filteredOrders.Remove(selected);
            }
        }

        private OrderDto GetSelectedOrder()
        {
            return _filteredOrders.FirstOrDefault(o => o.IsSelected);
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
                string itemsList = string.Join("\n", order.Items.Select(i => $"{i.ProductName} x{i.Quantity} = {i.TotalPrice:N2} ₽"));
                MessageBox.Show($"Заказ №{order.Id}\nДата: {order.OrderDate:dd.MM.yyyy}\nКлиент: {order.ClientFullName}\n" +
                                $"Собирал: {order.WorkerFullName}\nКурьер: {order.CourierFullName ?? "не назначен"}\n" +
                                $"Статус: {order.StatusName}\nСумма: {order.TotalCost:N2} ₽\n\nТовары:\n{itemsList}");
            }
        }
    }

    // Конвертер для видимости (если нужен)
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value is bool && (bool)value) ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}