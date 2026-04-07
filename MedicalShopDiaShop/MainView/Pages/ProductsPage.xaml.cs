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
        }

        private void ProductsPage_Loaded(object sender, RoutedEventArgs e)
        {
            _allProducts = new ObservableCollection<ProductDto>(GetStaticProducts());
            _filteredProducts = new ObservableCollection<ProductDto>(_allProducts);

            ProductsListView.ItemsSource = _filteredProducts;
            ProductsListBox.ItemsSource = _filteredProducts;

            ProductsListView.Visibility = Visibility.Visible;
            ProductsListBox.Visibility = Visibility.Collapsed;
            SetActiveButton(ListViewOnBtn);
        }

        private List<ProductDto> GetStaticProducts()
        {
            return new List<ProductDto>
            {
                new ProductDto
                {
                    Id = 1,
                    Name = "Глюкометр Accu-Chek Active",
                    Description = "Точный глюкометр с тест-полосками в комплекте",
                    Category = 1,
                    Price = 1450,
                    Image = "/Resources/ProductsImages/Accu-ChekProductImage.jpg"
                },
                new ProductDto
                {
                    Id = 2,
                    Name = "Тест-полоски OneTouch Select",
                    Description = "50 шт., для глюкометров OneTouch",
                    Category = 2,
                    Price = 950,
                    Image = "/Resources/ProductsImages/OneTouchStripes.jpg"
                },
                new ProductDto
                {
                    Id = 3,
                    Name = "Шприц-ручка НовоПен 4",
                    Description = "Для инсулина, шаг дозы 1 ед.",
                    Category = 3,
                    Price = 850,
                    Image = "/Resources/ProductsImages/Novopen.jpg"
                },
                new ProductDto
                {
                    Id = 4,
                    Name = "Крем для ног Diaderm",
                    Description = "Защитный крем для ног при диабете",
                    Category = 4,
                    Price = 420,
                    Image = "/Resources/ProductsImages/FootCream.jpg"
                },
                new ProductDto
                {
                    Id = 5,
                    Name = "Глюкометр Contour TS",
                    Description = "Простой и надежный глюкометр",
                    Category = 1,
                    Price = 1250,
                    Image = "/Resources/ProductsImages/ContourTS.jpg"
                },
                new ProductDto
                {
                    Id = 6,
                    Name = "Тест-полоски Accu-Chek Active",
                    Description = "50 шт., для Accu-Chek Active",
                    Category = 2,
                    Price = 890,
                    Image = "/Resources/ProductsImages/TestStripesRoche.jpg"
                },
                new ProductDto
                {
                    Id = 7,
                    Name = "Витамины для диабетиков",
                    Description = "Комплекс витаминов и минералов",
                    Category = 5,
                    Price = 1200,
                    Image = "/Resources/ProductsImages/DoppelherzVitamins.jpg"
                }
            };
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
            ProductsListView.ItemsSource = _filteredProducts;
            ProductsListBox.ItemsSource = _filteredProducts;
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchBox.Text?.Trim().ToLower();
            if (string.IsNullOrEmpty(searchText))
                _filteredProducts = new ObservableCollection<ProductDto>(_allProducts);
            else
                _filteredProducts = new ObservableCollection<ProductDto>(_allProducts.Where(p => p.Name.ToLower().Contains(searchText) || p.Description.ToLower().Contains(searchText)));

            ProductsListView.ItemsSource = _filteredProducts;
            ProductsListBox.ItemsSource = _filteredProducts;
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Добавление товара (будет реализовано)");
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelectedProduct();
            if (selected == null) MessageBox.Show("Выберите товар.");
            else MessageBox.Show($"Изменить: {selected.Name}");
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelectedProduct();
            if (selected == null) MessageBox.Show("Выберите товар.");
            else if (MessageBox.Show($"Удалить {selected.Name}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _allProducts.Remove(selected);
                _filteredProducts.Remove(selected);
            }
        }

        private ProductDto GetSelectedProduct()
        {
            return _filteredProducts.FirstOrDefault(p => p.IsSelected);
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
                MessageBox.Show($"Товар: {product.Name}\nКатегория: {product.CategoryName}\nЦена: {product.Price:N2} ₽\nОписание: {product.Description}");
        }
    }
}