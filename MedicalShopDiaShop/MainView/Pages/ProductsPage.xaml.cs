using MedicalShopDiaShop.AppData;
using MedicalShopDiaShop.Database;
using MedicalShopDiaShop.MainView.Dto;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MedicalShopDiaShop.MainView.Pages
{
    public partial class ProductsPage : Page
    {
        private ObservableCollection<ProductDto> _allProducts;
        private ObservableCollection<ProductDto> _filteredProducts;

        public ProductsPage()
        {
            InitializeComponent();
            Loaded += ProductsPage_Loaded;
            Unloaded += ProductsPage_Unloaded;
        }

        private void ProductsPage_Loaded(object sender, RoutedEventArgs e)
        {
            DataRefreshHub.DataChanged -= ProductsPage_OnDataRefresh;
            DataRefreshHub.DataChanged += ProductsPage_OnDataRefresh;

            LoadProductsFromDatabase();

            ProductsListView.ItemsSource = _filteredProducts;
            ProductsListBox.ItemsSource = _filteredProducts;

            ProductsListView.Visibility = Visibility.Visible;
            ProductsListBox.Visibility = Visibility.Collapsed;
            SetActiveButton(ListViewOnBtn);
        }

        private void ProductsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            DataRefreshHub.DataChanged -= ProductsPage_OnDataRefresh;
        }

        private void ProductsPage_OnDataRefresh(object sender, EventArgs e)
        {
            if (!IsLoaded) return;
            LoadProductsFromDatabase();
        }

        private void LoadProductsFromDatabase()
        {
            var stockMap = App.currentUser.StoreId.HasValue
                ? StockHelper.GetCurrentStock(App.currentUser.StoreId.Value)
                    .GroupBy(s => s.ProductId)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Available))
                : null;

            var products = App.context.Product.ToList();
            _allProducts = new ObservableCollection<ProductDto>(
                products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Category = p.Category,
                    Price = p.Price,
                    Image = p.Image,
                    AvailableQuantity = stockMap != null && stockMap.ContainsKey(p.Id) ? stockMap[p.Id] : 0,
                    IsSelected = false
                }));

            ApplyProductFiltersFromUi();
        }

        private void ApplyProductFiltersFromUi()
        {
            if (_allProducts == null) return;

            IEnumerable<ProductDto> query = _allProducts;

            if (CategoryFilterComboBox.SelectedItem is ComboBoxItem selectedItem &&
                selectedItem.Tag is string tag &&
                int.TryParse(tag, out int selectedCat))
            {
                query = query.Where(p => p.Category == selectedCat);
            }

            string searchText = SearchBox.Text?.Trim().ToLower();
            if (!string.IsNullOrEmpty(searchText))
            {
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchText) ||
                    (p.Description?.ToLower().Contains(searchText) ?? false));
            }

            _filteredProducts = new ObservableCollection<ProductDto>(query);
            RefreshProductsList();
        }

        private string GetCategoryName(int categoryId)
        {
            switch (categoryId)
            {
                case 1: return "Глюкометры";
                case 2: return "Тест-полоски";
                case 3: return "Шприцы";
                case 4: return "Кремы";
                case 5: return "Витамины";
                default: return "Другое";
            }
        }

        private void CategoryFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyProductFiltersFromUi();
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            ApplyProductFiltersFromUi();
        }

        private void RefreshProductsList()
        {
            ProductsListView.ItemsSource = null;
            ProductsListView.ItemsSource = _filteredProducts;
            ProductsListBox.ItemsSource = null;
            ProductsListBox.ItemsSource = _filteredProducts;
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            new AddEditProductWindow().ShowDialog();
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelectedProduct();
            if (selected == null)
            {
                FeedbackService.Warning("Выберите товар для редактирования.");
                return;
            }
            new AddEditProductWindow(selected.Id).ShowDialog();
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelectedProduct();
            if (selected == null)
            {
                FeedbackService.Warning("Выберите товар для удаления.");
                return;
            }

            if (FeedbackService.Question($"Удалить {selected.Name}?", "Подтверждение") == MessageBoxResult.Yes)
            {
                var productFromDb = App.context.Product.FirstOrDefault(p => p.Id == selected.Id);
                if (productFromDb != null)
                {
                    App.context.Product.Remove(productFromDb);
                    App.context.SaveChanges();
                    DataRefreshHub.Notify();
                    FeedbackService.Information("Товар удалён.");
                }
            }
        }

        private ProductDto GetSelectedProduct()
        {
            return _filteredProducts?.FirstOrDefault(p => p.IsSelected);
        }

        private void ListViewOnBtn_Click(object sender, RoutedEventArgs e)
        {
            ProductsListView.Visibility = Visibility.Visible;
            ProductsListBox.Visibility = Visibility.Collapsed;
            SetActiveButton(ListViewOnBtn);
        }

        private void ListBoxOnBtn_Click(object sender, RoutedEventArgs e)
        {
            ProductsListView.Visibility = Visibility.Collapsed;
            ProductsListBox.Visibility = Visibility.Visible;
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
            var product = btn?.Tag as ProductDto;
            if (product != null)
            {
                var detailsWindow = new ProductDetailsWindow(product.Id);
                detailsWindow.ShowDialog();
                LoadProductsFromDatabase();
                RefreshProductsList();
            }
        }
    }
}