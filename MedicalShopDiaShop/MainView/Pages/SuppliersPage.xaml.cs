using MedicalShopDiaShop.Database;
using MedicalShopDiaShop.AppData;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static MedicalShopDiaShop.AppData.Status;

namespace MedicalShopDiaShop.MainView.Pages
{
    public partial class SuppliersPage : Page
    {
        private enum ViewMode { Suppliers, Supplies }
        private ViewMode _currentMode = ViewMode.Suppliers;

        private ObservableCollection<SupplierItem> _allSuppliers;
        private ObservableCollection<SupplierItem> _filteredSuppliers;

        private ObservableCollection<SupplyItem> _allSupplies;
        private ObservableCollection<SupplyItem> _filteredSupplies;

        public SuppliersPage()
        {
            InitializeComponent();
            Loaded += SuppliersPage_Loaded;
        }

        private void SuppliersPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCityFilter();
            LoadSuppliersFromDatabase();
            LoadSuppliesFromDatabase();
            SetActiveMode(ViewMode.Suppliers);
        }

        #region Загрузка данных

        private void LoadCityFilter()
        {
            var cities = Enum.GetValues(typeof(City))
                .Cast<City>()
                .Select(c => new { Id = (int)c, Name = GetCityName(c) })
                .ToList();
            cities.Insert(0, new { Id = 0, Name = "Все" });
            CityFilterComboBox.ItemsSource = cities;
            CityFilterComboBox.DisplayMemberPath = "Name";
            CityFilterComboBox.SelectedValuePath = "Id";
            CityFilterComboBox.SelectedIndex = 0;
        }

        private void LoadSuppliersFromDatabase()
        {
            var dbSuppliers = App.context.Store
                .Where(s => s.Type == (int)StoreType.SupplierStore)
                .ToList();

            var items = dbSuppliers.Select(s => new SupplierItem
            {
                Id = s.Id,
                Name = s.Name,
                Address = s.Address,
                City = s.City,
                CityName = GetCityName((City)s.City),
                IsSelected = false
            });

            _allSuppliers = new ObservableCollection<SupplierItem>(items);
            ApplyFilterAndSearch();
            SubscribeToSupplierChanges(_filteredSuppliers);
            SuppliersListView.ItemsSource = _filteredSuppliers;
        }

        private void LoadSuppliesFromDatabase()
        {
            var dbSupplies = App.context.Supply
                .OrderByDescending(s => s.OrderDate)
                .ToList();

            var items = dbSupplies.Select(s => new SupplyItem
            {
                Id = s.Id,
                SupplierId = s.SupplierId ?? 0,
                SupplierName = s.SupplierId.HasValue ? (App.context.Store.Find(s.SupplierId)?.Name ?? "Неизвестно") : "Не выбран",
                OrderDate = s.OrderDate,
                AroundDate = s.AroundDate ?? s.OrderDate,
                ArrivedDate = s.ArrivedDate,
                TotalCost = s.TotalCost,
                IsCompleted = s.ArrivedDate.HasValue && s.ArrivedDate <= DateTime.Now,
                IsSelected = false
            }).ToList();

            _allSupplies = new ObservableCollection<SupplyItem>(items);
            _filteredSupplies = new ObservableCollection<SupplyItem>(_allSupplies);
            SubscribeToSupplyChanges(_filteredSupplies);
            SuppliesListBox.ItemsSource = _filteredSupplies;
        }

        private string GetCityName(City city)
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

        #endregion

        #region Фильтрация и поиск

        private void ApplyFilterAndSearch()
        {
            var filtered = _allSuppliers.AsEnumerable();

            if (CityFilterComboBox.SelectedValue is int cityId && cityId != 0)
                filtered = filtered.Where(s => s.City == cityId);

            string searchText = SearchBox.Text?.Trim().ToLower();
            if (!string.IsNullOrEmpty(searchText))
            {
                filtered = filtered.Where(s =>
                    (s.Name ?? string.Empty).ToLower().Contains(searchText) ||
                    (s.Address ?? string.Empty).ToLower().Contains(searchText));
            }

            _filteredSuppliers = new ObservableCollection<SupplierItem>(filtered);
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentMode == ViewMode.Suppliers)
            {
                ApplyFilterAndSearch();
                SubscribeToSupplierChanges(_filteredSuppliers);
                SuppliersListView.ItemsSource = _filteredSuppliers;
            }
            else
            {
                ApplySupplyFilterAndSearch();
                SuppliesListBox.ItemsSource = _filteredSupplies;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) { }

        private void CityFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_allSuppliers == null || _currentMode != ViewMode.Suppliers) return;
            ApplyFilterAndSearch();
            SubscribeToSupplierChanges(_filteredSuppliers);
            SuppliersListView.ItemsSource = _filteredSuppliers;
        }

        private void ApplySupplyFilterAndSearch()
        {
            var filtered = _allSupplies.AsEnumerable();
            string searchText = SearchBox.Text?.Trim().ToLower();
            if (!string.IsNullOrEmpty(searchText))
            {
                filtered = filtered.Where(s =>
                    (s.SupplierName ?? string.Empty).ToLower().Contains(searchText) ||
                    s.Id.ToString().Contains(searchText));
            }
            _filteredSupplies = new ObservableCollection<SupplyItem>(filtered);
        }

        #endregion

        #region Переключение режимов

        private void SetActiveMode(ViewMode mode)
        {
            _currentMode = mode;

            var primaryBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF673AB7"));
            var whiteBrush = Brushes.White;
            var transparentBrush = Brushes.Transparent;

            if (mode == ViewMode.Suppliers)
            {
                SuppliersListView.Visibility = Visibility.Visible;
                SuppliesListBox.Visibility = Visibility.Collapsed;
                CityFilterGroup.Visibility = Visibility.Visible;

                EditBtn.Visibility = Visibility.Visible;

                SuppliersModeBtn.Background = primaryBrush;
                SuppliersModeBtn.Foreground = whiteBrush;
                SuppliesModeBtn.Background = transparentBrush;
                SuppliesModeBtn.Foreground = primaryBrush;

                ApplyFilterAndSearch();
                SuppliersListView.ItemsSource = _filteredSuppliers;
            }
            else
            {
                SuppliersListView.Visibility = Visibility.Collapsed;
                SuppliesListBox.Visibility = Visibility.Visible;
                CityFilterGroup.Visibility = Visibility.Collapsed;

                EditBtn.Visibility = Visibility.Collapsed;

                SuppliesModeBtn.Background = primaryBrush;
                SuppliesModeBtn.Foreground = whiteBrush;
                SuppliersModeBtn.Background = transparentBrush;
                SuppliersModeBtn.Foreground = primaryBrush;

                ApplySupplyFilterAndSearch();
                SuppliesListBox.ItemsSource = _filteredSupplies;
            }
        }

        private void SuppliersModeBtn_Click(object sender, RoutedEventArgs e) => SetActiveMode(ViewMode.Suppliers);
        private void SuppliesModeBtn_Click(object sender, RoutedEventArgs e) => SetActiveMode(ViewMode.Supplies);

        #endregion

        #region Действия с поставщиками и поставками

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentMode == ViewMode.Suppliers)
            {
                var window = new AddEditSupplierWindow();
                if (window.ShowDialog() == true)
                    LoadSuppliersFromDatabase();
            }
            else
            {
                var window = new AddSupplyWindow();
                if (window.ShowDialog() == true)
                    LoadSuppliesFromDatabase();
            }
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentMode == ViewMode.Suppliers)
            {
                var selected = GetSelectedSupplier();
                if (selected == null)
                {
                    FeedbackService.Warning("Выберите поставщика для изменения.");
                    return;
                }
                var window = new AddEditSupplierWindow(selected.Id);
                if (window.ShowDialog() == true)
                    LoadSuppliersFromDatabase();
            }
            // В режиме поставок кнопка Edit скрыта, поэтому else не требуется
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentMode == ViewMode.Suppliers)
            {
                var selected = GetSelectedSupplier();
                if (selected == null)
                {
                    FeedbackService.Warning("Выберите поставщика для удаления.");
                    return;
                }
                if (FeedbackService.Question($"Удалить поставщика {selected.Name}?", "Подтверждение") == MessageBoxResult.Yes)
                {
                    var dbStore = App.context.Store.FirstOrDefault(s => s.Id == selected.Id);
                    if (dbStore != null)
                    {
                        App.context.Store.Remove(dbStore);
                        App.context.SaveChanges();
                    }
                    LoadSuppliersFromDatabase();
                    FeedbackService.Information("Поставщик удалён.");
                }
            }
            else
            {
                var selected = GetSelectedSupply();
                if (selected == null)
                {
                    FeedbackService.Warning("Выберите поставку для удаления.");
                    return;
                }
                if (FeedbackService.Question($"Удалить поставку №{selected.Id}?", "Подтверждение") == MessageBoxResult.Yes)
                {
                    var dbSupply = App.context.Supply.FirstOrDefault(s => s.Id == selected.Id);
                    if (dbSupply != null)
                    {
                        var products = App.context.SupplyProduct.Where(sp => sp.SupplyId == dbSupply.Id);
                        App.context.SupplyProduct.RemoveRange(products);
                        App.context.Supply.Remove(dbSupply);
                        App.context.SaveChanges();
                    }
                    LoadSuppliesFromDatabase();
                    FeedbackService.Information("Поставка удалена.");
                }
            }
        }

        private SupplierItem GetSelectedSupplier() => _filteredSuppliers?.FirstOrDefault(s => s.IsSelected);
        private SupplyItem GetSelectedSupply() => _filteredSupplies?.FirstOrDefault(s => s.IsSelected);

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var supplier = btn?.Tag as SupplierItem;
            if (supplier != null)
            {
                NavigationService?.Navigate(new SupplierProfilePage(supplier.Id));
            }
        }

        private void EditSupply_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var supply = btn?.Tag as SupplyItem;
            if (supply != null)
            {
                var window = new AddSupplyWindow(supply.Id);
                if (window.ShowDialog() == true)
                    LoadSuppliesFromDatabase();
            }
        }

        #endregion

        #region Подписки на изменения выделения

        private void SubscribeToSupplierChanges(ObservableCollection<SupplierItem> suppliers)
        {
            if (suppliers == null) return;
            foreach (var s in suppliers)
                s.PropertyChanged += OnSupplierPropertyChanged;
            suppliers.CollectionChanged += OnSuppliersCollectionChanged;
        }

        private void OnSuppliersCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (SupplierItem s in e.NewItems)
                    s.PropertyChanged += OnSupplierPropertyChanged;
            if (e.OldItems != null)
                foreach (SupplierItem s in e.OldItems)
                    s.PropertyChanged -= OnSupplierPropertyChanged;
        }

        private void OnSupplierPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SupplierItem.IsSelected))
            {
                var changed = sender as SupplierItem;
                if (changed != null && changed.IsSelected)
                {
                    foreach (var s in _filteredSuppliers)
                        if (s != changed && s.IsSelected)
                            s.IsSelected = false;
                }
            }
        }

        private void SubscribeToSupplyChanges(ObservableCollection<SupplyItem> supplies)
        {
            if (supplies == null) return;
            foreach (var s in supplies)
                s.PropertyChanged += OnSupplyPropertyChanged;
            supplies.CollectionChanged += OnSuppliesCollectionChanged;
        }

        private void OnSuppliesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (SupplyItem s in e.NewItems)
                    s.PropertyChanged += OnSupplyPropertyChanged;
            if (e.OldItems != null)
                foreach (SupplyItem s in e.OldItems)
                    s.PropertyChanged -= OnSupplyPropertyChanged;
        }

        private void OnSupplyPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SupplyItem.IsSelected))
            {
                var changed = sender as SupplyItem;
                if (changed != null && changed.IsSelected)
                {
                    foreach (var s in _filteredSupplies)
                        if (s != changed && s.IsSelected)
                            s.IsSelected = false;
                }
            }
        }

        #endregion

        private void ViewSupplyDetails_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var supply = btn?.Tag as SupplyItem;
            if (supply != null)
            {
                var window = new SupplyDetailsWindow(supply.Id);
                window.ShowDialog();
                LoadSuppliesFromDatabase();
                ApplySupplyFilterAndSearch();
                SuppliesListBox.ItemsSource = _filteredSupplies;
            }
        }
    }

    // Модель поставщика
    public class SupplierItem : INotifyPropertyChanged
    {
        public int Id { get; set; }
        private bool _isSelected;
        public string Name { get; set; }
        public string Address { get; set; }
        public int City { get; set; }
        public string CityName { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Модель поставки для отображения
    public class SupplyItem : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime AroundDate { get; set; }
        public DateTime? ArrivedDate { get; set; }
        public decimal TotalCost { get; set; }
        public bool IsCompleted { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public string DisplayName => $"Поставка №{Id}";
        public string DeliveryStatusText
        {
            get
            {
                if (!ArrivedDate.HasValue)
                    return "Статус: в пути";

                int diffDays = (ArrivedDate.Value.Date - AroundDate.Date).Days;
                if (diffDays == 0)
                    return "Статус: вовремя";
                if (diffDays < 0)
                    return $"Статус: раньше на {Math.Abs(diffDays)} д.";
                return $"Статус: опоздание на {diffDays} д.";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}