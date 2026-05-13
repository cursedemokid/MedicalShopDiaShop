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
        private int _orderStoreId;

        public AddEditOrderWindow()
        {
            InitializeComponent();
            TitleText.Text = "Новый заказ";
            LoadClients();
            LoadWorkers();
            LoadCouriers();
            LoadProducts();
            DeliveryTypeCmb.SelectedIndex = 0;
            _cartItems = new ObservableCollection<CartItem>();
            CartListBox.ItemsSource = _cartItems;
            UpdateTotalPrice();
            DeliveryTypeCmb.SelectionChanged += DeliveryTypeCmb_SelectionChanged;
            UpdateCourierVisibility();
        }

        public AddEditOrderWindow(int orderId)
        {
            InitializeComponent();
            _orderId = orderId;
            TitleText.Text = "Редактирование заказа";
            LoadClients();
            LoadWorkers();
            LoadCouriers();
            LoadProducts();
            LoadOrderData();
            UpdateTotalPrice();
            DeliveryTypeCmb.SelectionChanged += DeliveryTypeCmb_SelectionChanged;
            UpdateCourierVisibility();
        }

        private void DeliveryTypeCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCourierVisibility();
        }

        private void UpdateCourierVisibility()
        {
            bool isCourierDelivery = DeliveryTypeCmb.SelectedItem is ComboBoxItem item &&
                                     item.Tag.ToString() == "1";
            CourierLabel.Visibility = isCourierDelivery ? Visibility.Visible : Visibility.Collapsed;
            CourierBorder.Visibility = isCourierDelivery ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadOrderData()
        {
            using (var context = new DiaShopEntities())
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
                // Выбираем тип доставки
                var deliveryTypeItem = DeliveryTypeCmb.Items
                    .Cast<ComboBoxItem>()
                    .FirstOrDefault(i => i.Tag.ToString() == _editingOrder.DeliveryType.ToString());
                if (deliveryTypeItem != null) DeliveryTypeCmb.SelectedItem = deliveryTypeItem;

                // Загружаем текущего курьера, если есть
                if (_editingOrder.DeliveryId.HasValue)
                {
                    var delivery = context.Delivery.FirstOrDefault(d => d.Id == _editingOrder.DeliveryId);
                    if (delivery != null)
                        CourierCmb.SelectedValue = delivery.CourierId;
                }

                // Сначала материализуем строки заказа — GetAvailableQuantity нельзя перевести в SQL.
                var productOrders = context.ProductOrder
                    .Where(po => po.OrderId == _editingOrder.Id)
                    .ToList();

                var products = productOrders.Select(po => new CartItem
                {
                    Product = new ProductItem
                    {
                        Id = po.Product.Id,
                        Name = po.Product.Name,
                        Price = po.Price,
                        ImagePath = string.IsNullOrEmpty(po.Product.Image) ? "/Resources/placeholder.png" : po.Product.Image,
                        AvailableQuantity = GetAvailableQuantity(po.Product.Id)
                    },
                    Quantity = po.Quantity
                }).ToList();

                foreach (var item in products)
                    item.MaxQuantity = item.Product.AvailableQuantity + item.Quantity;

                _cartItems = new ObservableCollection<CartItem>(products);
                CartListBox.ItemsSource = _cartItems;

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

            foreach (var item in _cartItems)
            {
                if (item.Quantity > item.MaxQuantity)
                {
                    FeedbackService.Error($"Для товара '{item.Product.Name}' доступно только {item.MaxQuantity} шт.");
                    return;
                }

                if (_orderStoreId > 0 && !StockHelper.CheckAvailability(item.Product.Id, item.Quantity, _orderStoreId))
                {
                    FeedbackService.Error($"Недостаточно остатков по товару '{item.Product.Name}'.");
                    return;
                }
            }

            int deliveryType = int.Parse(((ComboBoxItem)DeliveryTypeCmb.SelectedItem).Tag.ToString());
            decimal totalCost = _cartItems.Sum(c => c.TotalPrice);
            DateTime now = DateTime.Now;

            using (var context = new DiaShopEntities())
            {
                if (_editingOrder == null)
                {
                    // Создание заказа
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

                    // Если тип доставки курьер и выбран курьер – создаём Delivery
                    if (deliveryType == 1 && CourierCmb.SelectedValue != null)
                    {
                        var delivery = new Delivery
                        {
                            StartDate = now,
                            EndDate = now.AddDays(3),
                            CourierId = (int)CourierCmb.SelectedValue,
                            Description = $"Доставка заказа №{order.Id}"
                        };
                        context.Delivery.Add(delivery);
                        context.SaveChanges();
                        order.DeliveryId = delivery.Id;
                        // Обновляем статус заказа на "Ожидает курьера"
                        order.Status = (int)OrderStatus.WaitCourier;
                        context.SaveChanges();

                        // Запись в историю
                        context.OrderHistory.Add(new OrderHistory
                        {
                            OrderId = order.Id,
                            OldStatus = (int)OrderStatus.InProcess,
                            NewStatus = (int)OrderStatus.WaitCourier,
                            UpdateAt = now,
                            FromUserId = App.currentUser.Id,
                            ToUserId = delivery.CourierId
                        });
                    }
                    else
                    {
                        // Для самовывоза сразу доставлен?
                        if (deliveryType == 2)
                        {
                            order.Status = (int)OrderStatus.Delivered;
                            context.SaveChanges();
                            context.OrderHistory.Add(new OrderHistory
                            {
                                OrderId = order.Id,
                                OldStatus = (int)OrderStatus.InProcess,
                                NewStatus = (int)OrderStatus.Delivered,
                                UpdateAt = now,
                                FromUserId = App.currentUser.Id,
                                ToUserId = null
                            });
                        }
                        else
                        {
                            context.OrderHistory.Add(new OrderHistory
                            {
                                OrderId = order.Id,
                                OldStatus = (int)OrderStatus.InProcess,
                                NewStatus = (int)OrderStatus.InProcess,
                                UpdateAt = now,
                                FromUserId = App.currentUser.Id,
                                ToUserId = null
                            });
                        }
                    }

                    // Добавляем товары
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
                    context.SaveChanges();

                    // Уведомление
                    if (App.currentUser.StoreId.HasValue)
                    {
                        NotificationHelper.NotifyAllStoreEmployees(App.currentUser.StoreId.Value,
                            $"Новый заказ №{order.Id} на сумму {totalCost:N2} ₽");
                    }
                }
                else
                {
                    // Редактирование заказа
                    _editingOrder.ClientId = (int)ClientCmb.SelectedValue;
                    _editingOrder.WorkerId = (int)WorkerCmb.SelectedValue;
                    _editingOrder.DeliveryType = deliveryType;
                    _editingOrder.TotalCost = totalCost;

                    // Обновляем доставку, если курьерская
                    if (deliveryType == 1)
                    {
                        if (_editingOrder.DeliveryId.HasValue)
                        {
                            var delivery = context.Delivery.Find(_editingOrder.DeliveryId);
                            if (delivery != null && CourierCmb.SelectedValue != null)
                            {
                                if (delivery.CourierId != (int)CourierCmb.SelectedValue)
                                {
                                    delivery.CourierId = (int)CourierCmb.SelectedValue;
                                    context.OrderHistory.Add(new OrderHistory
                                    {
                                        OrderId = _editingOrder.Id,
                                        OldStatus = _editingOrder.Status,
                                        NewStatus = _editingOrder.Status,
                                        UpdateAt = now,
                                        FromUserId = App.currentUser.Id,
                                        ToUserId = delivery.CourierId
                                    });
                                }
                                context.SaveChanges();
                            }
                        }
                        else if (CourierCmb.SelectedValue != null)
                        {
                            // Создаём новую доставку
                            var delivery = new Delivery
                            {
                                StartDate = now,
                                EndDate = now.AddDays(3),
                                CourierId = (int)CourierCmb.SelectedValue,
                                Description = $"Доставка заказа №{_editingOrder.Id}"
                            };
                            context.Delivery.Add(delivery);
                            context.SaveChanges();
                            _editingOrder.DeliveryId = delivery.Id;
                            context.SaveChanges();

                            context.OrderHistory.Add(new OrderHistory
                            {
                                OrderId = _editingOrder.Id,
                                OldStatus = _editingOrder.Status,
                                NewStatus = (int)OrderStatus.WaitCourier,
                                UpdateAt = now,
                                FromUserId = App.currentUser.Id,
                                ToUserId = delivery.CourierId
                            });
                            _editingOrder.Status = (int)OrderStatus.WaitCourier;
                        }
                    }
                    else
                    {
                        // Если сменили с курьера на самовывоз, удаляем связь с доставкой
                        if (_editingOrder.DeliveryId.HasValue)
                        {
                            var delivery = context.Delivery.Find(_editingOrder.DeliveryId);
                            if (delivery != null) context.Delivery.Remove(delivery);
                            _editingOrder.DeliveryId = null;
                            _editingOrder.Status = (int)OrderStatus.Delivered; // или InProcess?
                            context.SaveChanges();
                        }
                    }

                    // Обновляем товары
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Дополнительная инициализация, если нужна
        }

        private void LoadClients()
        {
            using (var context = new DiaShopEntities())
            {
                var clients = context.User
                    .Where(u => u.Role == (int)Role.Client && u.IsDeleted != true)
                    .Select(u => new { u.Id, u.LastName, u.FirstName })
                    .ToList() // Выполняем запрос к БД
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
            using (var context = new DiaShopEntities())
            {
                var workers = context.User
                    .Where(u => u.Role == (int)Role.Worker && u.IsDeleted != true)
                    .Select(u => new { u.Id, u.LastName, u.FirstName })
                    .ToList()
                    .Select(u => new { u.Id, FullName = $"{u.LastName} {u.FirstName}" })
                    .ToList();
                WorkerCmb.ItemsSource = workers;
                WorkerCmb.DisplayMemberPath = "FullName";
                WorkerCmb.SelectedValuePath = "Id";
                if (App.currentUser.Role == (int)Role.Worker)
                    WorkerCmb.SelectedValue = App.currentUser.Id;
                else if (WorkerCmb.Items.Count > 0)
                    WorkerCmb.SelectedIndex = 0;

                WorkerCmb.SelectionChanged += WorkerCmb_SelectionChanged;
                RefreshStoreContext();
            }
        }

        private void LoadCouriers()
        {
            using (var context = new DiaShopEntities())
            {
                var couriers = context.User
                    .Where(u => u.Role == (int)Role.Courier && u.IsDeleted != true)
                    .Select(u => new { u.Id, u.LastName, u.FirstName })
                    .ToList()
                    .Select(u => new { u.Id, FullName = $"{u.LastName} {u.FirstName}" })
                    .ToList();
                CourierCmb.ItemsSource = couriers;
                CourierCmb.DisplayMemberPath = "FullName";
                CourierCmb.SelectedValuePath = "Id";
            }
        }

        private void LoadProducts()
        {
            using (var context = new DiaShopEntities())
            {
                RefreshStoreContext();
                var products = context.Product.ToList();
                _allProducts = new ObservableCollection<ProductItem>(
                    products.Select(p => new ProductItem
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.Price,
                        ImagePath = string.IsNullOrEmpty(p.Image) ? "/Resources/placeholder.png" : p.Image,
                        AvailableQuantity = GetAvailableQuantity(p.Id)
                    }));
                _filteredProducts = new ObservableCollection<ProductItem>(_allProducts);
                AvailableProductsListBox.ItemsSource = _filteredProducts;
            }
        }

        private void OrderProductSearchBar_FilterTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ApplyOrderProductSearchFilter();
        }

        private void OrderProductSearchBar_SearchClicked(object sender, RoutedEventArgs e) => ApplyOrderProductSearchFilter();

        private void ApplyOrderProductSearchFilter()
        {
            string search = OrderProductSearchBar.Text?.Trim().ToLower() ?? "";
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
                {
                    if (existing.Quantity >= existing.MaxQuantity)
                    {
                        FeedbackService.Warning($"Нельзя добавить больше. Доступно: {existing.MaxQuantity} шт.");
                        return;
                    }
                    existing.Quantity++;
                }
                else
                    _cartItems.Add(new CartItem { Product = selected, Quantity = 1, MaxQuantity = selected.AvailableQuantity });
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
                if (cartItem.Quantity >= cartItem.MaxQuantity)
                {
                    FeedbackService.Warning($"Превышен лимит по товару '{cartItem.Product.Name}'. Доступно: {cartItem.MaxQuantity} шт.");
                    return;
                }
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

        private void WorkerCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshStoreContext();
            RefreshAvailableQuantities();
        }

        private void RefreshStoreContext()
        {
            using (var context = new DiaShopEntities())
            {
                int workerId = 0;
                if (WorkerCmb.SelectedValue is int selectedWorkerId)
                    workerId = selectedWorkerId;
                else
                    workerId = App.currentUser.Id;

                _orderStoreId = context.User
                    .Where(u => u.Id == workerId)
                    .Select(u => u.StoreId)
                    .FirstOrDefault() ?? (App.currentUser.StoreId ?? 0);
            }
        }

        private int GetAvailableQuantity(int productId)
        {
            if (_orderStoreId <= 0) return 0;

            var stock = StockHelper.GetCurrentStock(_orderStoreId);
            return stock.Where(s => s.ProductId == productId).Sum(s => s.Available);
        }

        private void RefreshAvailableQuantities()
        {
            if (_allProducts != null)
            {
                foreach (var product in _allProducts)
                    product.AvailableQuantity = GetAvailableQuantity(product.Id);
            }

            if (_cartItems != null)
            {
                foreach (var item in _cartItems)
                {
                    item.Product.AvailableQuantity = GetAvailableQuantity(item.Product.Id);
                    if (item.MaxQuantity < item.Quantity)
                        item.MaxQuantity = item.Quantity;
                }
            }

            AvailableProductsListBox.Items.Refresh();
            CartListBox.Items.Refresh();
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
        public int AvailableQuantity { get; set; }
        public string AvailableText => $"Остаток: {AvailableQuantity} шт.";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class CartItem : INotifyPropertyChanged
    {
        private int _quantity;
        public ProductItem Product { get; set; }
        public int MaxQuantity { get; set; }
        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged("Quantity"); OnPropertyChanged("TotalPrice"); OnPropertyChanged("TotalPriceText"); OnPropertyChanged("QuantityHint"); }
        }
        public decimal TotalPrice => (Product?.Price ?? 0) * Quantity;
        public string QuantityHint => $"Доступно: {MaxQuantity} шт.";
        public string UnitPriceText => $"Цена/1: {(Product?.Price ?? 0):N2} ₽";
        public string TotalPriceText => $"Итог: {TotalPrice:N2} ₽";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}