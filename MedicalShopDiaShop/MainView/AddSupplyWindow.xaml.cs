using MedicalShopDiaShop.Database;
using MedicalShopDiaShop.MainView.Pages;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static MedicalShopDiaShop.AppData.Status;
using static MedicalShopDiaShop.MainView.Pages.ChooseProductsPage;

namespace MedicalShopDiaShop.MainView
{
    public partial class AddSupplyWindow : Window
    {
        public Supply CurrentSupply { get; private set; } // Черновик поставки
        public Store SelectedSupplier { get; set; }
        public ObservableCollection<CartItem> SelectedProducts { get; set; } = new ObservableCollection<CartItem>();
        public DateTime? DeliveryDate { get; set; }

        private int _currentStep = 1;
        private const int TotalSteps = 3;
        private readonly DiaShopEntities2 _context;

        // Конструктор для создания новой поставки
        public AddSupplyWindow()
        {
            InitializeComponent();
            _context = new DiaShopEntities2();
            CreateDraftSupply();
            LoadStep(1);
        }

        // Конструктор для продолжения черновика
        public AddSupplyWindow(int supplyId)
        {
            InitializeComponent();
            _context = new DiaShopEntities2();
            LoadDraftSupply(supplyId);
            //DetermineCurrentStepFromStatus();
            LoadStep(_currentStep);
        }

        private void CreateDraftSupply()
        {
            CurrentSupply = new Supply
            {
                UserId = App.currentUser.Id,
                OrderDate = DateTime.Now,
                TotalCost = 0
                // SupplierId пока null
            };
            _context.Supply.Add(CurrentSupply);
            _context.SaveChanges();
            UpdateSupplyStatus(OrderStatus.ChoosingSupplier);
        }

        private void LoadDraftSupply(int supplyId)
        {
            CurrentSupply = _context.Supply.Find(supplyId);
            if (CurrentSupply == null)
            {
                MessageBox.Show("Черновик не найден.");
                Close();
                return;
            }

            // Загружаем выбранного поставщика
            if (CurrentSupply.SupplierId != 0)
                SelectedSupplier = _context.Store.Find(CurrentSupply.SupplierId);

            // Загружаем товары
            var products = _context.SupplyProduct
                .Where(sp => sp.SupplyId == supplyId)
                .Select(sp => new CartItem
                {
                    Product = sp.Product,
                    Quantity = sp.Quantity
                }).ToList();
            SelectedProducts = new ObservableCollection<CartItem>(products);

            // Дата доставки
            DeliveryDate = CurrentSupply.AroundDate;
        }

        //private void DetermineCurrentStepFromStatus()
        //{
        //    var status = GetOrderStatus(CurrentSupply.Status ?? 6); // по умолчанию ChoosingSupplier
        //    switch (status)
        //    {
        //        case OrderStatus.ChoosingSupplier: _currentStep = 1; break;
        //        case OrderStatus.ChoosingProducts: _currentStep = 2; break;
        //        case OrderStatus.ChoosingAroundTime: _currentStep = 3; break;
        //        default: _currentStep = 1; break;
        //    }
        //}

        private void UpdateSupplyStatus(OrderStatus status)
        {
            if (CurrentSupply != null)
            {
                //CurrentSupply.Status = GetOrderStatusId(status);
                _context.SaveChanges();
            }
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
                    UpdateSupplyStatus(OrderStatus.ChoosingSupplier);
                    break;
                case 2:
                    StepTitleText.Text = "Шаг 2: Выбор товаров";
                    page = new ChooseProductsPage(this);
                    UpdateSupplyStatus(OrderStatus.ChoosingProducts);
                    break;
                case 3:
                    StepTitleText.Text = "Шаг 3: Дата доставки";
                    page = new ChooseDeliveryDatePage(this);
                    UpdateSupplyStatus(OrderStatus.ChoosingAroundTime);
                    break;
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

            CurrentSupply.SupplierId = SelectedSupplier.Id;
            CurrentSupply.AroundDate = DeliveryDate.Value;
            CurrentSupply.ArrivedDate = DeliveryDate.Value; 
            CurrentSupply.TotalCost = SelectedProducts.Sum(p => p.TotalPrice);
            //CurrentSupply.Status = GetOrderStatusId(OrderStatus.InProcess);

            var existingProducts = _context.SupplyProduct.Where(sp => sp.SupplyId == CurrentSupply.Id);
            _context.SupplyProduct.RemoveRange(existingProducts);
            foreach (var item in SelectedProducts)
            {
                _context.SupplyProduct.Add(new SupplyProduct
                {
                    SupplyId = CurrentSupply.Id,
                    ProductId = item.Product.Id,
                    Quantity = item.Quantity,
                    TotalPrice = item.TotalPrice
                });
            }
            _context.SaveChanges();

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