using MedicalShopDiaShop.Database;
using MedicalShopDiaShop.MainView;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MedicalShopDiaShop.MainView.Pages
{
    public partial class ChooseProductsPage : Page
    {
        private readonly AddSupplyWindow _parentWindow;
        private ObservableCollection<ProductItem> _availableProducts;
        private ObservableCollection<CartItem> _cartItems;

        public ChooseProductsPage(AddSupplyWindow parent)
        {
            InitializeComponent();
            _parentWindow = parent;
            _cartItems = _parentWindow.SelectedProducts ?? new ObservableCollection<CartItem>();
            LoadProducts();
            UpdateCartVisibility();
            UpdateTotalPrice();
        }

        private void LoadProducts()
        {
            // Загружаем все товары из БД
            var allProducts = App.context.Product.ToList();
            _availableProducts = new ObservableCollection<ProductItem>(
                allProducts.Select(p => new ProductItem
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    PricePerTen = p.Price,
                    Image = string.IsNullOrEmpty(p.Image) ? "/Resources/placeholder.png" : p.Image
                })
            );
            // Удаляем из доступных те, которые уже в корзине
            foreach (var cartItem in _cartItems)
            {
                var existing = _availableProducts.FirstOrDefault(p => p.Id == cartItem.Product.Id);
                if (existing != null)
                    _availableProducts.Remove(existing);
            }
            AvailableProductsListBox.ItemsSource = _availableProducts;
            CartListBox.ItemsSource = _cartItems;
        }

        private void AvailableProduct_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            var product = border?.DataContext as ProductItem;
            if (product != null)
            {
                // Перемещаем товар в корзину
                var cartItem = new CartItem
                {
                    Product = new Product
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Description = product.Description,
                        Price = product.PricePerTen,
                        Image = product.Image
                    },
                    Quantity = 10 // начальное количество
                };
                _cartItems.Add(cartItem);
                _availableProducts.Remove(product);

                UpdateCartVisibility();
                UpdateTotalPrice();
            }
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var cartItem = button?.Tag as CartItem;
            if (cartItem != null)
            {
                cartItem.Quantity += 10;
                UpdateTotalPrice();
                // Обновляем иконку минуса (вдруг было 10 стало 20)
                RefreshCartItemButtons();
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var cartItem = button?.Tag as CartItem;
            if (cartItem != null)
            {
                if (cartItem.Quantity == 10)
                {
                    // Удаляем из корзины
                    _cartItems.Remove(cartItem);
                    // Возвращаем в доступные
                    _availableProducts.Add(new ProductItem
                    {
                        Id = cartItem.Product.Id,
                        Name = cartItem.Product.Name,
                        Description = cartItem.Product.Description,
                        PricePerTen = cartItem.Product.Price,
                        Image = cartItem.Product.Image
                    });
                }
                else
                {
                    cartItem.Quantity -= 10;
                }
                UpdateCartVisibility();
                UpdateTotalPrice();
                RefreshCartItemButtons();
            }
        }

        private void RefreshCartItemButtons()
        {
            // Принудительно обновляем контейнеры, чтобы изменилась иконка минуса
            // Простой способ — перезадать ItemsSource
            var items = _cartItems.ToList();
            _cartItems.Clear();
            foreach (var item in items)
                _cartItems.Add(item);
        }

        private void UpdateCartVisibility()
        {
            if (_cartItems.Count == 0)
            {
                CartListBox.Visibility = Visibility.Collapsed;
                EmptyCartImage.Visibility = Visibility.Visible;
                ContinueButton.IsEnabled = false;
            }
            else
            {
                CartListBox.Visibility = Visibility.Visible;
                EmptyCartImage.Visibility = Visibility.Collapsed;
                ContinueButton.IsEnabled = true;
            }
        }

        private void UpdateTotalPrice()
        {
            decimal total = _cartItems.Sum(c => c.TotalPrice);
            TotalPriceText.Text = total.ToString("N2") + " ₽";
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            _parentWindow.GoToNextStep();
        }

        // Вспомогательные классы
        public class ProductItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public decimal PricePerTen { get; set; }
            public string Image { get; set; }
            public decimal PricePerUnit => PricePerTen / 10m;
        }

        public class CartItem : INotifyPropertyChanged
        {
            public Product Product { get; set; }
            private int _quantity;
            public int Quantity
            {
                get => _quantity;
                set { _quantity = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalPrice)); OnPropertyChanged(nameof(TotalPriceText)); }
            }
            public decimal TotalPrice => Product.Price * Quantity / 10m; // Если Price за 10 шт.
            public string UnitPriceText => $"Цена/1: {Product.Price / 10m:N2} ₽";
            public string TotalPriceText => $"Итог: {TotalPrice:N2} ₽";

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string prop = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}