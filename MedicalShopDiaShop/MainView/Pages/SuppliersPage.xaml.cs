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
        private ObservableCollection<SupplierItem> _allSuppliers;
        private ObservableCollection<SupplierItem> _filteredSuppliers;

        public SuppliersPage()
        {
            InitializeComponent();
            Loaded += SuppliersPage_Loaded;
        }

        private void SuppliersPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCityFilter();
            LoadSuppliersFromDatabase();
        }

        private void LoadCityFilter()
        {
            // Заполняем комбобокс городами (если enum City определён)
            var cities = Enum.GetValues(typeof(City))
                .Cast<City>()
                .Select(c => new { Id = (int)c, Name = c.ToString() })
                .ToList();
            cities.Insert(0, new { Id = 0, Name = "Все" });
            CityFilterComboBox.ItemsSource = cities;
            CityFilterComboBox.DisplayMemberPath = "Name";
            CityFilterComboBox.SelectedValuePath = "Id";
            CityFilterComboBox.SelectedIndex = 0;
        }

        private void LoadSuppliersFromDatabase()
        {
            // Загружаем магазины с типом SupplierStore (Type == 2)
            var dbSuppliers = App.context.Store
                .Where(s => s.Type == (int)StoreType.SupplierStore)
                .ToList();

            var items = dbSuppliers.Select(s => new SupplierItem
            {
                Id = s.Id,
                Name = s.Name,
                Address = s.Address,
                City = s.City,
                CityName = GetCityName(s.City),
                IsSelected = false
            });

            _allSuppliers = new ObservableCollection<SupplierItem>(items);
            ApplyFilterAndSearch();

            SubscribeToSupplierChanges(_filteredSuppliers);
            SuppliersListView.ItemsSource = _filteredSuppliers;
        }

        private string GetCityName(int cityId)
        {
            // Предполагаем, что City - enum
            if (Enum.IsDefined(typeof(City), cityId))
                return ((City)cityId).ToString();
            return "Неизвестно";
        }

        private void ApplyFilterAndSearch()
        {
            var filtered = _allSuppliers.AsEnumerable();

            // Фильтр по городу
            if (CityFilterComboBox.SelectedValue is int cityId && cityId != 0)
            {
                filtered = filtered.Where(s => s.City == cityId);
            }

            // Поиск по названию или адресу
            string searchText = SearchBox.Text?.Trim().ToLower();
            if (!string.IsNullOrEmpty(searchText))
            {
                filtered = filtered.Where(s =>
                    s.Name.ToLower().Contains(searchText) ||
                    s.Address.ToLower().Contains(searchText));
            }

            _filteredSuppliers = new ObservableCollection<SupplierItem>(filtered);
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilterAndSearch();
            SubscribeToSupplierChanges(_filteredSuppliers);
            SuppliersListView.ItemsSource = _filteredSuppliers;
        }

        private void CityFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_allSuppliers == null) return;
            ApplyFilterAndSearch();
            SubscribeToSupplierChanges(_filteredSuppliers);
            SuppliersListView.ItemsSource = _filteredSuppliers;
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditSupplierWindow();
            if (window.ShowDialog() == true)
            {
                LoadSuppliersFromDatabase();
            }
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelectedSupplier();
            if (selected == null)
            {
                FeedbackService.Warning("Не выбран ни один поставщик для изменения.", "Изменение");
                return;
            }

            var window = new AddEditSupplierWindow(selected.Id);
            if (window.ShowDialog() == true)
            {
                LoadSuppliersFromDatabase();
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelectedSupplier();
            if (selected == null)
            {
                FeedbackService.Warning("Не выбран ни один поставщик для удаления.", "Удаление");
                return;
            }

            var result = MessageBox.Show(
                $"Вы действительно хотите удалить поставщика: {selected.Name}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var dbStore = App.context.Store.FirstOrDefault(s => s.Id == selected.Id);
                if (dbStore != null)
                {
                    // Физическое удаление (можно заменить на мягкое, добавив IsDeleted)
                    App.context.Store.Remove(dbStore);
                    App.context.SaveChanges();
                }
                LoadSuppliersFromDatabase();
                FeedbackService.Information("Поставщик удалён.", "Удаление");
            }
        }

        private SupplierItem GetSelectedSupplier()
        {
            return _filteredSuppliers?.FirstOrDefault(s => s.IsSelected);
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var supplier = btn?.Tag as SupplierItem;
            if (supplier != null)
            {
                // Можно открыть детальную информацию о поставщике
                FeedbackService.Information(
                    $"Название: {supplier.Name}\nАдрес: {supplier.Address}\nГород: {supplier.CityName}",
                    "Информация о поставщике");
            }
        }

        // --- Логика единственного выделения ---
        private void SubscribeToSupplierChanges(ObservableCollection<SupplierItem> suppliers)
        {
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

        private void AddSupply_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddSupplyWindow();
            window.ShowDialog();
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
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}