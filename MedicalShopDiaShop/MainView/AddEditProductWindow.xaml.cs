using MedicalShopDiaShop.AppData;
using MedicalShopDiaShop.Database;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using static MedicalShopDiaShop.AppData.Status;

namespace MedicalShopDiaShop.MainView
{
    public partial class AddEditProductWindow : Window
    {
        private readonly int? _productId;
        private Product _editingProduct;
        private string _selectedImagePath = null; // относительный путь к сохранённому файлу

        public AddEditProductWindow()
        {
            InitializeComponent();
            TitleText.Text = "Добавление товара";
            LoadCategories();
        }

        public AddEditProductWindow(int productId)
        {
            InitializeComponent();
            _productId = productId;
            TitleText.Text = "Редактирование товара";
            LoadCategories();
            LoadProductData();
        }

        private void LoadCategories()
        {
            var categories = Enum.GetValues(typeof(Category))
                .Cast<Category>()
                .Select(c => new { Id = (int)c, Name = GetCategoryName(c) })
                .ToList();
            CategoryCmb.ItemsSource = categories;
            CategoryCmb.DisplayMemberPath = "Name";
            CategoryCmb.SelectedValuePath = "Id";
            if (CategoryCmb.Items.Count > 0)
                CategoryCmb.SelectedIndex = 0;
        }

        private string GetCategoryName(Category category)
        {
            switch (category)
            {
                case Category.Glucometers: return "Глюкометры";
                case Category.TestStrips: return "Тест-полоски";
                case Category.Syringes: return "Шприцы";
                case Category.Creams: return "Кремы";
                case Category.Vitamins: return "Витамины";
                default: return category.ToString();
            }
        }

        private void LoadProductData()
        {
            if (!_productId.HasValue) return;
            _editingProduct = App.context.Product.FirstOrDefault(p => p.Id == _productId.Value);
            if (_editingProduct == null)
            {
                FeedbackService.Error("Товар не найден.");
                Close();
                return;
            }

            NameTb.Text = _editingProduct.Name;
            DescriptionTb.Text = _editingProduct.Description;
            PriceTb.Text = _editingProduct.Price.ToString("F2");
            CategoryCmb.SelectedValue = _editingProduct.Category;

            if (!string.IsNullOrEmpty(_editingProduct.Image))
            {
                string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _editingProduct.Image);
                if (File.Exists(fullPath))
                    PreviewImage.Source = new BitmapImage(new Uri(fullPath, UriKind.Absolute));
                _selectedImagePath = _editingProduct.Image;
            }
        }

        private void SelectImageBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Изображения|*.jpg;*.png;*.jpeg;*.bmp",
                Title = "Выберите изображение товара"
            };
            if (dialog.ShowDialog() == true)
            {
                // Генерируем уникальное имя файла
                string fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(dialog.FileName);
                string destFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "ProductsImages");
                if (!Directory.Exists(destFolder))
                    Directory.CreateDirectory(destFolder);
                string destPath = System.IO.Path.Combine(destFolder, fileName);

                try
                {
                    File.Copy(dialog.FileName, destPath, true);
                    _selectedImagePath = $"/Resources/ProductsImages/{fileName}";
                    PreviewImage.Source = new BitmapImage(new Uri(destPath, UriKind.Absolute));
                }
                catch (Exception ex)
                {
                    FeedbackService.Error($"Ошибка копирования файла: {ex.Message}");
                }
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(NameTb.Text))
            {
                FeedbackService.Error("Введите название товара.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(DescriptionTb.Text))
            {
                FeedbackService.Error("Введите описание товара.");
                return false;
            }
            if (CategoryCmb.SelectedValue == null)
            {
                FeedbackService.Error("Выберите категорию.");
                return false;
            }
            if (!decimal.TryParse(PriceTb.Text, out decimal price) || price <= 0)
            {
                FeedbackService.Error("Введите корректную цену (положительное число).");
                return false;
            }
            return true;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;

            decimal price = decimal.Parse(PriceTb.Text);
            int categoryId = (int)CategoryCmb.SelectedValue;

            if (_editingProduct == null)
            {
                // Добавление
                var newProduct = new Product
                {
                    Name = NameTb.Text.Trim(),
                    Description = DescriptionTb.Text.Trim(),
                    Category = categoryId,
                    Price = price,
                    Image = _selectedImagePath
                };
                App.context.Product.Add(newProduct);
            }
            else
            {
                // Редактирование
                _editingProduct.Name = NameTb.Text.Trim();
                _editingProduct.Description = DescriptionTb.Text.Trim();
                _editingProduct.Category = categoryId;
                _editingProduct.Price = price;
                if (!string.IsNullOrEmpty(_selectedImagePath))
                    _editingProduct.Image = _selectedImagePath;
            }

            try
            {
                App.context.SaveChanges();
                FeedbackService.Information("Товар успешно сохранён.");
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                FeedbackService.Error($"Ошибка при сохранении: {ex.Message}");
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}