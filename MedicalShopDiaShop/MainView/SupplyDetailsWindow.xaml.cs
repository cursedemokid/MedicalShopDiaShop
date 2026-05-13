using MedicalShopDiaShop.AppData;
using MedicalShopDiaShop.Database;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using static MedicalShopDiaShop.AppData.Status;

namespace MedicalShopDiaShop.MainView
{
    public partial class SupplyDetailsWindow : Window
    {
        private readonly int _supplyId;
        private Supply _supply;

        public SupplyDetailsWindow(int supplyId)
        {
            InitializeComponent();
            _supplyId = supplyId;
            LoadSupplyDetails();
        }

        private void LoadSupplyDetails()
        {
            using (var context = new DiaShopEntities())
            {
                _supply = context.Supply.FirstOrDefault(s => s.Id == _supplyId);
                if (_supply == null)
                {
                    FeedbackService.Error("Поставка не найдена.");
                    Close();
                    return;
                }

                TitleText.Text = $"Поставка №{_supply.Id}";

                var supplier = context.Store.FirstOrDefault(s => s.Id == _supply.SupplierId);
                SupplierText.Text = supplier != null ? supplier.Name : "Неизвестно";

                OrderDateText.Text = _supply.OrderDate.ToString("dd.MM.yyyy HH:mm");
                AroundDateText.Text = _supply.AroundDate?.ToString("dd.MM.yyyy") ?? "Не указана";
                TotalText.Text = $"{_supply.TotalCost:N2} ₽";
                ArrivedDateText.Text = _supply.ArrivedDate?.ToString("dd.MM.yyyy HH:mm") ?? "Не принята";
                DeviationText.Text = GetDeviationText(_supply.AroundDate, _supply.ArrivedDate);
                MarkArrivedBtn.Visibility = _supply.ArrivedDate.HasValue ? Visibility.Collapsed : Visibility.Visible;

                var products = context.SupplyProduct
                    .Where(sp => sp.SupplyId == _supply.Id)
                    .Select(sp => new SupplyProductItem
                    {
                        Name = sp.Product.Name,
                        Quantity = sp.Quantity,
                        PricePerUnit = sp.TotalPrice / sp.Quantity,
                        TotalPrice = sp.TotalPrice,
                        Image = string.IsNullOrEmpty(sp.Product.Image) ? "/Resources/placeholder.png" : sp.Product.Image
                    }).ToList();

                ProductsItemsControl.ItemsSource = new ObservableCollection<SupplyProductItem>(products);
            }
        }

        private void MarkArrivedBtn_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new DiaShopEntities())
            {
                var supply = context.Supply.FirstOrDefault(s => s.Id == _supplyId);
                if (supply == null)
                {
                    FeedbackService.Error("Поставка не найдена.");
                    return;
                }

                if (supply.ArrivedDate.HasValue)
                {
                    FeedbackService.Information("Поставка уже принята.");
                    return;
                }

                supply.ArrivedDate = DateTime.Now;
                supply.Status = (int)OrderStatus.Delivered;
                context.SaveChanges();
            }

            FeedbackService.Information("Фактическая дата поставки сохранена.");
            LoadSupplyDetails();
        }

        private static string GetDeviationText(DateTime? aroundDate, DateTime? arrivedDate)
        {
            if (!aroundDate.HasValue)
                return "Невозможно рассчитать";
            if (!arrivedDate.HasValue)
                return "Поставка еще в пути";

            int diffDays = (arrivedDate.Value.Date - aroundDate.Value.Date).Days;
            if (diffDays == 0) return "Вовремя";
            if (diffDays < 0) return $"Раньше на {Math.Abs(diffDays)} д.";
            return $"Опоздание на {diffDays} д.";
        }
    }

    public class SupplyProductItem
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal PricePerUnit { get; set; }
        public decimal TotalPrice { get; set; }
        public string Image { get; set; }
    }
}