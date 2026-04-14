using MedicalShopDiaShop.AppData;
using MedicalShopDiaShop.Database;
using MedicalShopDiaShop.MainView.Pages;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static MedicalShopDiaShop.AppData.Status;
using static MedicalShopDiaShop.MainView.Pages.ChooseProductsPage;

namespace MedicalShopDiaShop.MainView
{
    public partial class AddSupplyWindow : Window
    {
        public Supply CurrentSupply { get; private set; }
        public Store SelectedSupplier { get; set; }
        public ObservableCollection<CartItem> SelectedProducts { get; set; } = new ObservableCollection<CartItem>();
        public DateTime? DeliveryDate { get; set; }

        private int _currentStep = 1;
        private const int TotalSteps = 3;
        private readonly DiaShopEntities2 _context;
        private int _navigationDirection = 1; // 1 - вперёд, -1 - назад
        private bool _isFirstLoad = true;

        public AddSupplyWindow()
        {
            InitializeComponent();
            _context = new DiaShopEntities2();
            CreateDraftSupply();
            LoadStep(1);
        }

        public AddSupplyWindow(int supplyId)
        {
            InitializeComponent();
            _context = new DiaShopEntities2();
            LoadDraftSupply(supplyId);
            LoadStep(_currentStep);
        }

        private void CreateDraftSupply()
        {
            CurrentSupply = new Supply
            {
                UserId = App.currentUser.Id,
                OrderDate = DateTime.Now,
                TotalCost = 0
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

            if (CurrentSupply.SupplierId != 0)
                SelectedSupplier = _context.Store.Find(CurrentSupply.SupplierId);

            var products = _context.SupplyProduct
                .Where(sp => sp.SupplyId == supplyId)
                .Select(sp => new CartItem
                {
                    Product = sp.Product,
                    Quantity = sp.Quantity
                }).ToList();
            SelectedProducts = new ObservableCollection<CartItem>(products);

            DeliveryDate = CurrentSupply.AroundDate;
        }

        private void UpdateSupplyStatus(OrderStatus status)
        {
            if (CurrentSupply != null)
            {
                // здесь можно сохранять статус, если нужно
                _context.SaveChanges();
            }
        }

        private void LoadStep(int step)
        {
            _currentStep = step;
            UpdateStepIndicator();
            UpdateBackButtonVisibility();

            // Сбрасываем позицию Frame перед сменой страницы
            var transform = AddSupplyFrame.RenderTransform as TranslateTransform;
            if (transform != null) transform.X = 0;

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

            // Анимация только после первого шага
            if (_isFirstLoad)
            {
                _isFirstLoad = false;
                return;
            }

            // Запускаем анимацию после того, как страница отрисовалась
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                var story = _navigationDirection == 1
                    ? (Storyboard)FindResource("SlideInFromLeft")
                    : (Storyboard)FindResource("SlideInFromRight");
                story.Begin(AddSupplyFrame);
            }), System.Windows.Threading.DispatcherPriority.Loaded);
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
            {
                _navigationDirection = 1;
                LoadStep(_currentStep + 1);
            }
            else
                CreateSupply();
        }

        public void GoToPreviousStep()
        {
            if (_currentStep > 1)
            {
                _navigationDirection = -1;
                LoadStep(_currentStep - 1);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            GoToPreviousStep();
        }

        private void CreateSupply()
        {
            if (SelectedSupplier == null || SelectedProducts.Count == 0 || DeliveryDate == null)
            {
                FeedbackService.Error("Не все данные заполнены.");
                return;
            }

            CurrentSupply.SupplierId = SelectedSupplier.Id;
            CurrentSupply.AroundDate = DeliveryDate.Value;
            CurrentSupply.ArrivedDate = DeliveryDate.Value;
            CurrentSupply.TotalCost = SelectedProducts.Sum(p => p.TotalPrice);

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

            FeedbackService.Information("Поставка успешно создана.");
            DialogResult = true;
            Close();
        }
    }

    public class SupplyProductItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => Product.Price * Quantity;
        public string DisplayName => Product.Name;
        public string ImagePath => string.IsNullOrEmpty(Product.Image) ? "/Resources/placeholder.png" : Product.Image;
    }
}