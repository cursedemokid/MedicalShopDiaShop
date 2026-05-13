using MedicalShopDiaShop.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using static MedicalShopDiaShop.AppData.Status;

namespace MedicalShopDiaShop.MainView
{
    public partial class SupplierProfileWindow : Window, INotifyPropertyChanged
    {
        private readonly int _supplierId;
        private readonly List<SupplierProductViewItem> _allProducts = new List<SupplierProductViewItem>();
        private readonly ObservableCollection<SupplierProductViewItem> _filteredProducts = new ObservableCollection<SupplierProductViewItem>();
        private readonly ObservableCollection<SupplierSupplyViewItem> _supplies = new ObservableCollection<SupplierSupplyViewItem>();

        private string _supplierName;
        private string _supplierAddress;
        private string _supplierCityName;

        public string SupplierName
        {
            get => _supplierName;
            private set
            {
                _supplierName = value;
                OnPropertyChanged();
            }
        }

        public string SupplierAddress
        {
            get => _supplierAddress;
            private set
            {
                _supplierAddress = value;
                OnPropertyChanged();
            }
        }

        public string SupplierCityName
        {
            get => _supplierCityName;
            private set
            {
                _supplierCityName = value;
                OnPropertyChanged();
            }
        }

        public SupplierProfileWindow(int supplierId)
        {
            InitializeComponent();
            _supplierId = supplierId;
            DataContext = this;
            Loaded += SupplierProfileWindow_Loaded;
        }

        private void SupplierProfileWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSupplierInfo();
            LoadSupplierProducts();
            LoadSupplierSuppliesForCurrentStore();
        }

        private void LoadSupplierInfo()
        {
            var supplier = App.context.Store.FirstOrDefault(s => s.Id == _supplierId && s.Type == (int)StoreType.SupplierStore);
            if (supplier == null)
            {
                SupplierName = "Поставщик не найден";
                SupplierAddress = string.Empty;
                SupplierCityName = string.Empty;
                return;
            }

            SupplierName = supplier.Name;
            SupplierAddress = supplier.Address;
            SupplierCityName = GetCityName((City)supplier.City);
        }

        private void LoadSupplierProducts()
        {
            _allProducts.Clear();

            var products = App.context.SupplyProduct
                .Where(sp => sp.Supply.SupplierId == _supplierId)
                .Select(sp => sp.Product)
                .Distinct()
                .ToList();

            foreach (var product in products)
            {
                _allProducts.Add(new SupplierProductViewItem
                {
                    Name = product.Name,
                    PriceText = string.Format(CultureInfo.CurrentCulture, "{0:N2} ₽", product.Price)
                });
            }

            ApplyProductFilter();
        }

        private void LoadSupplierSuppliesForCurrentStore()
        {
            _supplies.Clear();

            int? currentStoreId = App.currentUser?.StoreId;
            if (!currentStoreId.HasValue)
            {
                SuppliesListBox.ItemsSource = _supplies;
                return;
            }

            var storeEmployeeIds = App.context.User
                .Where(u => u.StoreId == currentStoreId.Value)
                .Select(u => u.Id)
                .ToList();

            var supplies = App.context.Supply
                .Where(s => s.SupplierId == _supplierId && s.UserId.HasValue && storeEmployeeIds.Contains(s.UserId.Value))
                .OrderByDescending(s => s.OrderDate)
                .ToList();

            foreach (var supply in supplies)
            {
                _supplies.Add(new SupplierSupplyViewItem
                {
                    Header = $"Поставка №{supply.Id} от {supply.OrderDate:dd.MM.yyyy}",
                    Details = $"Ожидаемая дата: {(supply.AroundDate.HasValue ? supply.AroundDate.Value.ToString("dd.MM.yyyy") : "не указана")} | Сумма: {supply.TotalCost:N2} ₽"
                });
            }

            SuppliesListBox.ItemsSource = _supplies;
        }

        private static string GetCityName(City city)
        {
            switch (city)
            {
                case City.Moscow: return "Москва";
                case City.SaintPetersburg: return "Санкт-Петербург";
                case City.Novosibirsk: return "Новосибирск";
                case City.Yekaterinburg: return "Екатеринбург";
                case City.Kazan: return "Казань";
                case City.NizhnyNovgorod: return "Нижний Новгород";
                case City.Chelyabinsk: return "Челябинск";
                case City.Omsk: return "Омск";
                case City.RostovOnDon: return "Ростов-на-Дону";
                case City.Ufa: return "Уфа";
                case City.Krasnoyarsk: return "Красноярск";
                case City.Perm: return "Пермь";
                case City.Voronezh: return "Воронеж";
                case City.Volgograd: return "Волгоград";
                case City.Krasnodar: return "Краснодар";
                default: return "Неизвестно";
            }
        }

        private void ProductSearchButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyProductFilter();
        }

        private void ProductSearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ApplyProductFilter();
        }

        private void ApplyProductFilter()
        {
            string search = ProductSearchBox.Text?.Trim().ToLowerInvariant() ?? string.Empty;

            var filtered = string.IsNullOrWhiteSpace(search)
                ? _allProducts
                : _allProducts.Where(p => p.Name.ToLowerInvariant().Contains(search)).ToList();

            _filteredProducts.Clear();
            foreach (var item in filtered)
                _filteredProducts.Add(item);

            ProductsListBox.ItemsSource = _filteredProducts;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SupplierProductViewItem
    {
        public string Name { get; set; }
        public string PriceText { get; set; }
    }

    public class SupplierSupplyViewItem
    {
        public string Header { get; set; }
        public string Details { get; set; }
    }
}
