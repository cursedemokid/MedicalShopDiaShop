using MedicalShopDiaShop.AppData;
using MedicalShopDiaShop.Database;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace MedicalShopDiaShop.MainView
{
    public partial class ProductDetailsWindow : Window
    {
        private readonly int _productId;
        private Product _product;
        private ProductDetailsViewModel _viewModel;

        public ProductDetailsWindow(int productId)
        {
            InitializeComponent();
            _productId = productId;
            _viewModel = new ProductDetailsViewModel();
            DataContext = _viewModel;
            LoadProductData();
        }

        private async void LoadProductData()
        {
            using (var context = new DiaShopEntities())
            {
                _product = await context.Product.FindAsync(_productId);
                if (_product == null)
                {
                    FeedbackService.Error("Товар не найден.");
                    Close();
                    return;
                }

                // Загрузка изображения
                string imagePath = string.IsNullOrEmpty(_product.Image)
                    ? "/Resources/productPlaceholder.png"
                    : _product.Image;
                ProductImage.Source = new System.Windows.Media.Imaging.BitmapImage(
                    new Uri(imagePath, UriKind.Relative));

                // Заполнение ViewModel
                _viewModel.Name = _product.Name;
                _viewModel.CategoryName = GetCategoryName(_product.Category);
                _viewModel.Description = _product.Description;
                _viewModel.Price = _product.Price;
                int stock = GetProductStock(_product.Id);
                _viewModel.Stock = stock;
                _viewModel.StockColor = GetStockColor(stock);
            }
        }

        private int GetProductStock(int productId)
        {
            var stock = StockHelper.GetCurrentStock(App.currentUser.StoreId ?? 1);
            return stock.Where(s => s.ProductId == productId).Sum(s => s.Available);
        }

        private SolidColorBrush GetStockColor(int stock)
        {
            if (stock <= 0) return Brushes.Red;
            if (stock < 10) return Brushes.Orange;
            return Brushes.Green;
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

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new AddEditProductWindow(_productId);
            if (editWindow.ShowDialog() == true)
            {
                // Обновляем данные после редактирования
                LoadProductData();
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    // ViewModel для окна деталей
    public class ProductDetailsViewModel : INotifyPropertyChanged
    {
        private string _name;
        private string _categoryName;
        private string _description;
        private decimal _price;
        private int _stock;
        private SolidColorBrush _stockColor;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string CategoryName
        {
            get => _categoryName;
            set { _categoryName = value; OnPropertyChanged(); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        public decimal Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(); }
        }

        public int Stock
        {
            get => _stock;
            set { _stock = value; OnPropertyChanged(); }
        }

        public SolidColorBrush StockColor
        {
            get => _stockColor;
            set { _stockColor = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}