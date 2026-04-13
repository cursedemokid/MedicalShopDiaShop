using MedicalShopDiaShop.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MedicalShopDiaShop.MainView.Pages;

namespace MedicalShopDiaShop.MainView
{
    public partial class AddSupplyWindow : Window
    {
        // Данные, собираемые на шагах
        public Store SelectedSupplier { get; set; }
        public ObservableCollection<SupplyProductItem> SelectedProducts { get; set; } = new ObservableCollection<SupplyProductItem>();
        public DateTime? DeliveryDate { get; set; }

        private int _currentStep = 1;
        private const int TotalSteps = 3;

        public AddSupplyWindow()
        {
            InitializeComponent();
            LoadStep(1);
        }

        private void LoadStep(int step)
        {
            _currentStep = step;
            UpdateStepIndicator();
            UpdateBackButtonVisibility();

            Page page = null;
            switch (step)
            {
                case 1:
                    StepTitleText.Text = "Шаг 1: Выбор поставщика";
                    page = new ChooseSupplierPage(this);
                    break;
                case 2:
                    StepTitleText.Text = "Шаг 2: Выбор товаров";
                    page = new ChooseProductsPage(this);
                    break;
                case 3:
                    StepTitleText.Text = "Шаг 3: Дата доставки";
                    page = new ChooseDeliveryDatePage(this);
                    break;
                default:
                    return;
            }

            AddSupplyFrame.Navigate(page);
        }

        private void UpdateStepIndicator()
        {
            Step1Ellipse.Fill = _currentStep >= 1 ? FindResource("PrimaryHueLightBrush") as System.Windows.Media.Brush : System.Windows.Media.Brushes.LightGray;
            Step2Ellipse.Fill = _currentStep >= 2 ? FindResource("PrimaryHueLightBrush") as System.Windows.Media.Brush : System.Windows.Media.Brushes.LightGray;
            Step3Ellipse.Fill = _currentStep >= 3 ? FindResource("PrimaryHueLightBrush") as System.Windows.Media.Brush : System.Windows.Media.Brushes.LightGray;
        }

        private void UpdateBackButtonVisibility()
        {
            BackButton.Visibility = _currentStep > 1 ? Visibility.Visible : Visibility.Hidden;
        }

        public void GoToNextStep()
        {
            if (_currentStep < TotalSteps)
                LoadStep(_currentStep + 1);
            else
                CreateSupply(); // На последнем шаге кнопка "Продолжить" заменяется на "Создать поставку"
        }

        public void GoToPreviousStep()
        {
            if (_currentStep > 1)
                LoadStep(_currentStep - 1);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            GoToPreviousStep();
        }

        private void CreateSupply()
        {
            if (SelectedSupplier == null || SelectedProducts.Count == 0 || DeliveryDate == null)
            {
                MessageBox.Show("Не все данные заполнены.");
                return;
            }

            using (var context = new DiaShopEntities1())
            {
                var supply = new Supply
                {
                    SupplierId = SelectedSupplier.Id,
                    UserId = App.currentUser.Id,
                    OrderDate = DateTime.Now,
                    ArrivedDate = DeliveryDate.Value, // или другая логика
                    AroundDate = DeliveryDate.Value,
                    TotalCost = SelectedProducts.Sum(p => p.TotalPrice)
                };
                context.Supply.Add(supply);
                context.SaveChanges();

                foreach (var item in SelectedProducts)
                {
                    context.SupplyProduct.Add(new SupplyProduct
                    {
                        SupplyId = supply.Id,
                        ProductId = item.Product.Id,
                        Quantity = item.Quantity,
                        TotalPrice = item.TotalPrice
                    });
                }
                context.SaveChanges();
            }

            MessageBox.Show("Поставка успешно создана.");
            DialogResult = true;
            Close();
        }
    }

    // Вспомогательный класс для товара в корзине
    public class SupplyProductItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => Product.Price * Quantity;
        public string DisplayName => Product.Name;
        public string ImagePath => string.IsNullOrEmpty(Product.Image) ? "/Resources/placeholder.png" : Product.Image;
    }
}