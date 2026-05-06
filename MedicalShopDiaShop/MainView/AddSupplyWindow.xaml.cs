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

namespace MedicalShopDiaShop.MainView
{
    public partial class AddSupplyWindow : Window
    {
        public Supply CurrentSupply { get; private set; }
        public Store SelectedSupplier { get; set; }
        public ObservableCollection<ChooseProductsPage.CartItem> SelectedProducts { get; set; } = new ObservableCollection<ChooseProductsPage.CartItem>();
        public DateTime? DeliveryDate { get; set; }

        private int _currentStep = 1;
        private const int TotalSteps = 3;
        private readonly DiaShopEntities _context;
        private int _navigationDirection = 1;
        private bool _isFirstLoad = true;

        public AddSupplyWindow()
        {
            InitializeComponent();
            _context = new DiaShopEntities();
            CreateDraftSupply();
            LoadStep(1);
        }

        public AddSupplyWindow(int supplyId)
        {
            InitializeComponent();
            _context = new DiaShopEntities();
            LoadDraftSupply(supplyId);
            LoadStep(_currentStep);
        }

        private void CreateDraftSupply()
        {
            CurrentSupply = new Supply
            {
                UserId = App.currentUser.Id,
                SupplierId = 8,
                OrderDate = DateTime.Now,
                TotalCost = 0,
                AroundDate = DateTime.Now,
                ArrivedDate = DateTime.Now
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
                FeedbackService.Information("Черновик не найден.");
                Close();
                return;
            }

            if (CurrentSupply.SupplierId != 0)
                SelectedSupplier = _context.Store.Find(CurrentSupply.SupplierId);

            var products = _context.SupplyProduct
                .Where(sp => sp.SupplyId == supplyId)
                .Select(sp => new ChooseProductsPage.CartItem
                {
                    Product = sp.Product,
                    Quantity = sp.Quantity
                }).ToList();

            SelectedProducts = new ObservableCollection<ChooseProductsPage.CartItem>(products);
            DeliveryDate = CurrentSupply.AroundDate;
        }

        private void UpdateSupplyStatus(OrderStatus status)
        {
            if (CurrentSupply != null)
            {
                _context.SaveChanges();
            }
        }

        private void LoadStep(int step)
        {
            _currentStep = step;
            UpdateStepIndicator();
            UpdateBackButtonVisibility();

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

            if (_isFirstLoad)
            {
                _isFirstLoad = false;
                return;
            }

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
            var activeBrush = this.TryFindResource("MaterialDesign.Brush.Primary.Light") as Brush;
            if (activeBrush == null)
            {
                activeBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6200EE"));
            }

            Step1Ellipse.Fill = _currentStep >= 1 ? activeBrush : Brushes.LightGray;
            Step2Ellipse.Fill = _currentStep >= 2 ? activeBrush : Brushes.LightGray;
            Step3Ellipse.Fill = _currentStep >= 3 ? activeBrush : Brushes.LightGray;
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
            CurrentSupply.AroundDate = ToSqlDateTimeRange(DeliveryDate.Value);
            CurrentSupply.ArrivedDate = ToSqlDateTimeRange(DeliveryDate.Value);
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

            string supplyText = $"Создана новая поставка №{CurrentSupply.Id} от поставщика {SelectedSupplier.Name} на сумму {CurrentSupply.TotalCost:N2} ₽";
            NotificationHelper.NotifyAllStoreEmployees((int)App.currentUser.StoreId, supplyText, supplyId: CurrentSupply.Id);

            FeedbackService.Information("Поставка успешно создана.");
            DialogResult = true;
            Close();
        }

        private DateTime ToSqlDateTimeRange(DateTime date)
        {
            if (date < new DateTime(1753, 1, 1))
                return new DateTime(1753, 1, 1);
            if (date > new DateTime(9999, 12, 31))
                return new DateTime(9999, 12, 31);
            return date;
        }
    }
}