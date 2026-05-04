using MedicalShopDiaShop.AppData;
using MedicalShopDiaShop.Database;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace MedicalShopDiaShop.MainView
{
    public partial class ProductDetailsWindow : Window
    {
        private readonly int _productId;
        private Product _product;

        public ProductDetailsWindow(int productId)
        {
            InitializeComponent();
            _productId = productId;
            LoadProductData();
        }

        private void LoadProductData()
        {
            using (var context = new DiaShopEntities())
            {
                _product = context.Product.FirstOrDefault(p => p.Id == _productId);
                if (_product == null)
                {
                    FeedbackService.Error("Товар не найден.");
                    Close();
                    return;
                }

                // Загрузка аватара
                string imagePath = string.IsNullOrEmpty(_product.Image)
                    ? "/Resources/productPlaceholder.png"
                    : _product.Image;
                ProductImage.Source = new System.Windows.Media.Imaging.BitmapImage(
                    new Uri(imagePath, UriKind.Relative));

                // Установка DataContext для привязок
                DataContext = new
                {
                    Name = _product.Name,
                    CategoryName = GetCategoryName(_product.Category),
                    Description = _product.Description,
                    Price = _product.Price,
                    Stock = GetProductStock(_product.Id),
                    StockColor = GetStockColor(GetProductStock(_product.Id))
                };
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
                // Обновляем данные в окне после редактирования
                LoadProductData();
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}