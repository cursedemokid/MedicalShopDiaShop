using MedicalShopDiaShop.AppData;
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
                    RetailPricePerUnit = p.Price,
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
                        Price = product.RetailPricePerUnit,
                        Image = product.Image
                    },
                    Quantity = 1
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
                cartItem.Quantity += 1;
                UpdateTotalPrice();
                // Обновляем отображение строки корзины
                RefreshCartItemButtons();
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var cartItem = button?.Tag as CartItem;
            if (cartItem != null)
            {
                if (cartItem.Quantity <= 1)
                {
                    _cartItems.Remove(cartItem);
                    _availableProducts.Add(new ProductItem
                    {
                        Id = cartItem.Product.Id,
                        Name = cartItem.Product.Name,
                        Description = cartItem.Product.Description,
                        RetailPricePerUnit = cartItem.Product.Price,
                        Image = cartItem.Product.Image
                    });
                }
                else
                {
                    cartItem.Quantity -= 1;
                }
                UpdateCartVisibility();
                UpdateTotalPrice();
                RefreshCartItemButtons();
            }
        }

        private void RefreshCartItemButtons()
        {
            // Пересоздаём коллекцию, чтобы ListBox обновил привязки строки.
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
            decimal purchase = _cartItems.Sum(c => c.TotalPrice);
            decimal retailRef = _cartItems.Sum(c => c.RetailReferenceTotal);
            TotalPriceText.Text = purchase.ToString("N2") + " ₽";
            if (SavingsSummaryText != null)
            {
                SavingsSummaryText.Text = _cartItems.Count == 0
                    ? string.Empty
                    : $"Сумма по справочной рознице: {retailRef:N2} ₽ · экономия к опту: {Math.Max(0, retailRef - purchase):N2} ₽ (закупка = {(SupplyPricing.WholesaleFromRetailFactor * 100m):F0}% от розничной цены за 1 шт.)";
            }
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
            /// <summary>Розничная цена за 1 шт. из БД.</summary>
            public decimal RetailPricePerUnit { get; set; }
            public string Image { get; set; }
            public string RetailUnitLabel => $"Справ. розница за 1 шт.: {RetailPricePerUnit:N2} ₽";
            public string WholesaleUnitLabel => $"Опт закуп. за 1 шт.: {SupplyPricing.WholesaleUnitPrice(RetailPricePerUnit):N2} ₽";
        }

        public class CartItem : INotifyPropertyChanged
        {
            public Product Product { get; set; }

            private decimal? _purchaseTotalOverride;

            /// <summary>Если задано (черновик из БД), сумма строки как сохранено; иначе считается по опту.</summary>
            public decimal? PurchaseTotalOverride
            {
                get => _purchaseTotalOverride;
                set
                {
                    _purchaseTotalOverride = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalPrice));
                    OnPropertyChanged(nameof(TotalPriceText));
                    OnPropertyChanged(nameof(UnitPriceText));
                    OnPropertyChanged(nameof(RetailReferenceTotal));
                    OnPropertyChanged(nameof(SavingsVsRetail));
                    OnPropertyChanged(nameof(WholesaleUnitEffective));
                }
            }

            private int _quantity;
            public int Quantity
            {
                get => _quantity;
                set
                {
                    _quantity = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalPrice));
                    OnPropertyChanged(nameof(TotalPriceText));
                    OnPropertyChanged(nameof(UnitPriceText));
                    OnPropertyChanged(nameof(RetailReferenceTotal));
                    OnPropertyChanged(nameof(SavingsVsRetail));
                    OnPropertyChanged(nameof(WholesaleUnitEffective));
                }
            }

            public decimal RetailReferenceTotal =>
                Product != null ? SupplyPricing.RetailReferenceLineTotal(Product.Price, Quantity) : 0m;

            public decimal TotalPrice =>
                PurchaseTotalOverride ?? (Product != null
                    ? SupplyPricing.WholesaleLineTotal(Product.Price, Quantity)
                    : 0m);

            public decimal SavingsVsRetail => RetailReferenceTotal - TotalPrice;

            public decimal WholesaleUnitEffective =>
                Quantity > 0 ? TotalPrice / Quantity : 0m;

            public string UnitPriceText =>
                Product == null
                    ? string.Empty
                    : $"Розн. за 1 шт.: {SupplyPricing.RetailUnitPrice(Product.Price):N2} ₽ · Опт за 1 шт.: {WholesaleUnitEffective:N2} ₽";

            public string TotalPriceText =>
                $"Опт: {TotalPrice:N2} ₽ (розн.: {RetailReferenceTotal:N2} ₽, −{SavingsVsRetail:N2} ₽)";

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string prop = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}