using MedicalShopDiaShop.AppData;
using MedicalShopDiaShop.Database;
using MedicalShopDiaShop.MainView.Dto;
using System;
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
        }

        private void ProductsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProductsFromDatabase();

            ProductsListView.ItemsSource = _filteredProducts;
            ProductsListBox.ItemsSource = _filteredProducts;

            ProductsListView.Visibility = Visibility.Visible;
            ProductsListBox.Visibility = Visibility.Collapsed;
            SetActiveButton(ListViewOnBtn);
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
            _filteredProducts = new ObservableCollection<ProductDto>(_allProducts);
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
            if (CategoryFilterComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string tag && int.TryParse(tag, out int selectedCat))
            {
                _filteredProducts = new ObservableCollection<ProductDto>(_allProducts.Where(p => p.Category == selectedCat));
            }
            else
            {
                _filteredProducts = new ObservableCollection<ProductDto>(_allProducts);
            }
            RefreshProductsList();
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchBox.Text?.Trim().ToLower();
            if (string.IsNullOrEmpty(searchText))
                _filteredProducts = new ObservableCollection<ProductDto>(_allProducts);
            else
                _filteredProducts = new ObservableCollection<ProductDto>(_allProducts.Where(p =>
                    p.Name.ToLower().Contains(searchText) ||
                    (p.Description?.ToLower().Contains(searchText) ?? false)));

            RefreshProductsList();
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
            var window = new AddEditProductWindow();
            if (window.ShowDialog() == true)
            {
                LoadProductsFromDatabase();
                RefreshProductsList();
            }
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelectedProduct();
            if (selected == null)
            {
                FeedbackService.Warning("Выберите товар для редактирования.");
                return;
            }
            var window = new AddEditProductWindow(selected.Id);
            if (window.ShowDialog() == true)
            {
                LoadProductsFromDatabase();
                RefreshProductsList();
            }
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
                    LoadProductsFromDatabase();
                    RefreshProductsList();
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