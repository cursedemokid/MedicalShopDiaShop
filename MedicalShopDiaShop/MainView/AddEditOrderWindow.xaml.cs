using MedicalShopDiaShop.AppData;
using MedicalShopDiaShop.Database;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static MedicalShopDiaShop.AppData.Status;

namespace MedicalShopDiaShop.MainView
{
    public partial class AddEditOrderWindow : Window
    {
        private readonly int? _orderId;
        private Order _editingOrder;
        private ObservableCollection<CartItem> _cartItems;
        private ObservableCollection<ProductItem> _allProducts;
        private ObservableCollection<ProductItem> _filteredProducts;

        public AddEditOrderWindow()
        {
            InitializeComponent();
            TitleText.Text = "Новый заказ";
            LoadClients();
            LoadWorkers();
            LoadProducts();
            DeliveryTypeCmb.SelectedIndex = 0;
            _cartItems = new ObservableCollection<CartItem>();
            CartListBox.ItemsSource = _cartItems;
            UpdateTotalPrice();
        }

        public AddEditOrderWindow(int orderId)
        {
            InitializeComponent();
            _orderId = orderId;
            TitleText.Text = "Редактирование заказа";
            LoadClients();
            LoadWorkers();
            LoadProducts();
            LoadOrderData();
            UpdateTotalPrice();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Дополнительная инициализация, если нужна
        }

        private void LoadClients()
        {
            using (var context = new DiaShopEntities3())
            {
                var clients = context.User
                    .Where(u => u.Role == (int)Role.Client && u.IsDeleted != true)
                    .Select(u => new { u.Id, FullName = $"{u.LastName} {u.FirstName}" })
                    .ToList();
                ClientCmb.ItemsSource = clients;
                ClientCmb.DisplayMemberPath = "FullName";
                ClientCmb.SelectedValuePath = "Id";
                if (ClientCmb.Items.Count > 0) ClientCmb.SelectedIndex = 0;
            }
        }

        private void LoadWorkers()
        {
            using (var context = new DiaShopEntities3())
            {
                var workers = context.User
                    .Where(u => u.Role == (int)Role.Worker && u.IsDeleted != true)
                    .Select(u => new { u.Id, FullName = $"{u.LastName} {u.FirstName}" })
                    .ToList();
                WorkerCmb.ItemsSource = workers;
                WorkerCmb.DisplayMemberPath = "FullName";
                WorkerCmb.SelectedValuePath = "Id";
                // По умолчанию текущий пользователь, если он работник
                if (App.currentUser.Role == (int)Role.Worker)
                    WorkerCmb.SelectedValue = App.currentUser.Id;
                else if (WorkerCmb.Items.Count > 0)
                    WorkerCmb.SelectedIndex = 0;
            }
        }

        private void LoadProducts()
        {
            using (var context = new DiaShopEntities3())
            {
                var products = context.Product.ToList();
                _allProducts = new ObservableCollection<ProductItem>(
                    products.Select(p => new ProductItem
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.Price,
                        ImagePath = string.IsNullOrEmpty(p.Image) ? "/Resources/placeholder.png" : p.Image
                    }));
                _filteredProducts = new ObservableCollection<ProductItem>(_allProducts);
                AvailableProductsListBox.ItemsSource = _filteredProducts;
            }
        }

        private void LoadOrderData()
        {
            using (var context = new DiaShopEntities3())
            {
                _editingOrder = context.Order.FirstOrDefault(o => o.Id == _orderId);
                if (_editingOrder == null)
                {
                    FeedbackService.Error("Заказ не найден.");
                    Close();
                    return;
                }

                ClientCmb.SelectedValue = _editingOrder.ClientId;
                WorkerCmb.SelectedValue = _editingOrder.WorkerId;
                DeliveryTypeCmb.SelectedItem = DeliveryTypeCmb.Items
                    .Cast<ComboBoxItem>()
                    .FirstOrDefault(i => i.Tag.ToString() == _editingOrder.DeliveryType.ToString());

                var products = context.ProductOrder
                    .Where(po => po.OrderId == _editingOrder.Id)
                    .Select(po => new CartItem
                    {
                        Product = new ProductItem
                        {
                            Id = po.Product.Id,
                            Name = po.Product.Name,
                            Price = po.Price,
                            ImagePath = string.IsNullOrEmpty(po.Product.Image) ? "/Resources/placeholder.png" : po.Product.Image
                        },
                        Quantity = po.Quantity
                    }).ToList();

                _cartItems = new ObservableCollection<CartItem>(products);
                CartListBox.ItemsSource = _cartItems;
                UpdateTotalPrice();

                // Удаляем из доступных те, что уже в корзине
                foreach (var item in _cartItems)
                {
                    var existing = _allProducts.FirstOrDefault(p => p.Id == item.Product.Id);
                    if (existing != null)
                        _allProducts.Remove(existing);
                }
                _filteredProducts = new ObservableCollection<ProductItem>(_allProducts);
                AvailableProductsListBox.ItemsSource = _filteredProducts;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string search = SearchBox.Text?.Trim().ToLower() ?? "";
            if (string.IsNullOrEmpty(search))
                _filteredProducts = new ObservableCollection<ProductItem>(_allProducts);
            else
                _filteredProducts = new ObservableCollection<ProductItem>(_allProducts.Where(p => p.Name.ToLower().Contains(search)));
            AvailableProductsListBox.ItemsSource = _filteredProducts;
        }

        private void AvailableProduct_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selected = AvailableProductsListBox.SelectedItem as ProductItem;
            if (selected != null)
            {
                var existing = _cartItems.FirstOrDefault(c => c.Product.Id == selected.Id);
                if (existing != null)
                    existing.Quantity++;
                else
                    _cartItems.Add(new CartItem { Product = selected, Quantity = 1 });
                _allProducts.Remove(selected);
                _filteredProducts = new ObservableCollection<ProductItem>(_allProducts);
                AvailableProductsListBox.ItemsSource = _filteredProducts;
                UpdateTotalPrice();
            }
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var cartItem = btn?.Tag as CartItem;
            if (cartItem != null)
            {
                cartItem.Quantity++;
                UpdateTotalPrice();
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var cartItem = btn?.Tag as CartItem;
            if (cartItem != null)
            {
                if (cartItem.Quantity == 1)
                {
                    _cartItems.Remove(cartItem);
                    _allProducts.Add(cartItem.Product);
                    _filteredProducts = new ObservableCollection<ProductItem>(_allProducts);
                    AvailableProductsListBox.ItemsSource = _filteredProducts;
                }
                else
                    cartItem.Quantity--;
                UpdateTotalPrice();
            }
        }

        private void UpdateTotalPrice()
        {
            decimal total = _cartItems.Sum(c => c.TotalPrice);
            TotalPriceText.Text = total.ToString("N2") + " ₽";
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ClientCmb.SelectedValue == null || WorkerCmb.SelectedValue == null || DeliveryTypeCmb.SelectedItem == null)
            {
                FeedbackService.Error("Заполните все поля.");
                return;
            }
            if (_cartItems.Count == 0)
            {
                FeedbackService.Error("Добавьте хотя бы один товар.");
                return;
            }

            int deliveryType = int.Parse(((ComboBoxItem)DeliveryTypeCmb.SelectedItem).Tag.ToString());
            decimal totalCost = _cartItems.Sum(c => c.TotalPrice);
            DateTime now = DateTime.Now;

            using (var context = new DiaShopEntities3())
            {
                if (_editingOrder == null)
                {
                    var order = new Order
                    {
                        DateTime = now,
                        ClientId = (int)ClientCmb.SelectedValue,
                        WorkerId = (int)WorkerCmb.SelectedValue,
                        DeliveryType = deliveryType,
                        Status = (int)OrderStatus.InProcess,
                        TotalCost = totalCost,
                        DeliveryId = null
                    };
                    context.Order.Add(order);
                    context.SaveChanges();

                    foreach (var item in _cartItems)
                    {
                        context.ProductOrder.Add(new ProductOrder
                        {
                            OrderId = order.Id,
                            ProductId = item.Product.Id,
                            Quantity = item.Quantity,
                            Price = item.Product.Price
                        });
                    }

                    // Добавляем запись в историю
                    context.OrderHistory.Add(new OrderHistory
                    {
                        OrderId = order.Id,
                        OldStatus = (int)OrderStatus.InProcess,
                        NewStatus = (int)OrderStatus.InProcess,
                        UpdateAt = now,
                        FromUserId = App.currentUser.Id,
                        ToUserId = null
                    });

                    context.SaveChanges();

                    // Уведомление сотрудникам магазина
                    NotificationHelper.NotifyAllStoreEmployees(App.currentUser.StoreId,
                        $"Новый заказ №{order.Id} на сумму {totalCost:N2} ₽");
                }
                else
                {
                    // Редактирование
                    _editingOrder.ClientId = (int)ClientCmb.SelectedValue;
                    _editingOrder.WorkerId = (int)WorkerCmb.SelectedValue;
                    _editingOrder.DeliveryType = deliveryType;
                    _editingOrder.TotalCost = totalCost;
                    // Статус не меняем при редактировании

                    var existingProducts = context.ProductOrder.Where(po => po.OrderId == _editingOrder.Id);
                    context.ProductOrder.RemoveRange(existingProducts);

                    foreach (var item in _cartItems)
                    {
                        context.ProductOrder.Add(new ProductOrder
                        {
                            OrderId = _editingOrder.Id,
                            ProductId = item.Product.Id,
                            Quantity = item.Quantity,
                            Price = item.Product.Price
                        });
                    }
                    context.SaveChanges();
                }
            }

            FeedbackService.Information("Заказ сохранён.");
            DialogResult = true;
            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    // Вспомогательные классы
    public class ProductItem : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string ImagePath { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class CartItem : INotifyPropertyChanged
    {
        private int _quantity;
        public ProductItem Product { get; set; }
        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged("Quantity"); OnPropertyChanged("TotalPrice"); }
        }
        public decimal TotalPrice => (Product?.Price ?? 0) * Quantity;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}