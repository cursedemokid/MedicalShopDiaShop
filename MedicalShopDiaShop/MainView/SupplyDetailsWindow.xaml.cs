using MedicalShopDiaShop.AppData;
using MedicalShopDiaShop.Database;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace MedicalShopDiaShop.MainView
{
    public partial class SupplyDetailsWindow : Window
    {
        private readonly int _supplyId;

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
                var supply = context.Supply.FirstOrDefault(s => s.Id == _supplyId);
                if (supply == null)
                {
                    FeedbackService.Error("Поставка не найдена.");
                    Close();
                    return;
                }

                TitleText.Text = $"Поставка №{supply.Id}";

                var supplier = context.Store.FirstOrDefault(s => s.Id == supply.SupplierId);
                SupplierText.Text = supplier != null ? supplier.Name : "Неизвестно";

                OrderDateText.Text = supply.OrderDate.ToString("dd.MM.yyyy HH:mm");
                AroundDateText.Text = supply.AroundDate?.ToString("dd.MM.yyyy") ?? "Не указана";
                TotalText.Text = $"{supply.TotalCost:N2} ₽";

                var products = context.SupplyProduct
                    .Where(sp => sp.SupplyId == supply.Id)
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