using LiveCharts;
using LiveCharts.Wpf;
using MedicalShopDiaShop.Database;
using MedicalShopDiaShop.MainView.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using static MedicalShopDiaShop.AppData.Status;

namespace MedicalShopDiaShop.MainView.Pages
{
    public partial class StatisticsPage : Page
    {
        private readonly StatisticsViewModel _viewModel;
        private readonly int _currentStoreId;

        public StatisticsPage()
        {
            InitializeComponent();
            _viewModel = new StatisticsViewModel();
            DataContext = _viewModel;
            _currentStoreId = (int)App.currentUser.StoreId;
            LoadAllData();
        }

        private void LoadAllData()
        {
            using (var context = new DiaShopEntities())
            {
                LoadMonthlyRevenue(context);
                LoadSupplierPie(context);
                LoadTopProducts(context);
                LoadOrderCount(context);
                LoadDeliveryTypePie(context);
                LoadProfitability(context);
                LoadTaskPerformancePie(context);
            }
        }

        private void LoadMonthlyRevenue(DiaShopEntities context)
        {
            var orders = context.Order
                .Where(o => o.User.StoreId == _currentStoreId)
                .ToList();

            var monthlyData = orders
                .GroupBy(o => new { o.DateTime.Year, o.DateTime.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new { Date = new DateTime(g.Key.Year, g.Key.Month, 1), Revenue = g.Sum(o => o.TotalCost) })
                .ToList();

            _viewModel.MonthlyRevenueLabels = monthlyData.Select(d => d.Date.ToString("MMM yyyy")).ToArray();

            var values = new ChartValues<decimal>(monthlyData.Select(d => d.Revenue));
            _viewModel.MonthlyRevenueSeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Выручка",
                    Values = values,
                    DataLabels = true,
                    LabelPoint = point => point.Y.ToString("N0") + " ₽"
                }
            };
            _viewModel.TotalRevenueAllTime = monthlyData.Sum(d => d.Revenue);
        }

        private void LoadSupplierPie(DiaShopEntities context)
        {
            var supplies = context.Supply
                .Where(s => s.User.StoreId == _currentStoreId)
                .GroupBy(s => s.SupplierId)
                .Select(g => new { SupplierId = g.Key, Count = g.Count() })
                .ToList();

            var series = new SeriesCollection();
            foreach (var item in supplies)
            {
                var store = context.Store.Find(item.SupplierId);
                series.Add(new PieSeries
                {
                    Title = store?.Name ?? "Неизвестно",
                    Values = new ChartValues<int> { item.Count },
                    DataLabels = true
                });
            }
            _viewModel.SupplierPieSeries = series;
        }

        private void LoadTopProducts(DiaShopEntities context)
        {
            var topProducts = context.ProductOrder
                .Where(po => po.Order.User.StoreId == _currentStoreId)
                .GroupBy(po => po.ProductId)
                .Select(g => new { ProductId = g.Key, Quantity = g.Sum(po => po.Quantity) })
                .OrderByDescending(g => g.Quantity)
                .Take(10)
                .ToList();

            var productNames = new List<string>();
            var quantities = new ChartValues<int>();

            foreach (var item in topProducts)
            {
                var product = context.Product.Find(item.ProductId);
                productNames.Add(product?.Name ?? "???");
                quantities.Add(item.Quantity);
            }

            _viewModel.TopProductsLabels = productNames.ToArray();
            _viewModel.TopProductsSeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Продано (шт.)",
                    Values = quantities,
                    DataLabels = true,
                    LabelPoint = point => point.Y.ToString()
                }
            };
        }

        private void LoadOrderCount(DiaShopEntities context)
        {
            var orders = context.Order
                .Where(o => o.User.StoreId == _currentStoreId)
                .ToList();

            var monthlyCount = orders
                .GroupBy(o => new { o.DateTime.Year, o.DateTime.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => g.Count())
                .ToArray();

            var values = new ChartValues<int>(monthlyCount);
            _viewModel.OrderCountMonthlySeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Количество заказов",
                    Values = values,
                    DataLabels = true,
                    LabelPoint = point => point.Y.ToString()
                }
            };
            _viewModel.TotalOrdersAllTime = monthlyCount.Sum();
        }

        private void LoadDeliveryTypePie(DiaShopEntities context)
        {
            var deliveryGroups = context.Order
                .Where(o => o.User.StoreId == _currentStoreId)
                .GroupBy(o => o.DeliveryType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToList();

            var series = new SeriesCollection();
            foreach (var g in deliveryGroups)
            {
                string typeName = g.Type == 1 ? "Курьер" : "Самовывоз";
                series.Add(new PieSeries
                {
                    Title = typeName,
                    Values = new ChartValues<int> { g.Count },
                    DataLabels = true
                });
            }
            _viewModel.DeliveryTypePieSeries = series;
        }

        private void LoadProfitability(DiaShopEntities context)
        {
            var products = context.Product.ToList();
            var profitabilityData = new List<decimal>();
            var labels = new List<string>();

            foreach (var product in products)
            {
                // Приводим к nullable decimal, чтобы обработать null от SUM
                decimal? sold = context.ProductOrder
                    .Where(po => po.ProductId == product.Id && po.Order.User.StoreId == _currentStoreId)
                    .Sum(po => (decimal?)po.Quantity * po.Price); // явное приведение к nullable

                decimal? bought = context.SupplyProduct
                    .Where(sp => sp.ProductId == product.Id && sp.Supply.User.StoreId == _currentStoreId)
                    .Sum(sp => (decimal?)sp.TotalPrice); // явное приведение к nullable

                decimal profit = (sold ?? 0) - (bought ?? 0);
                profitabilityData.Add(profit);
                labels.Add(product.Name);
            }

            var top = profitabilityData
                .Select((p, i) => new { Profit = p, Label = labels[i] })
                .OrderByDescending(x => Math.Abs(x.Profit))
                .Take(20)
                .ToList();

            _viewModel.ProfitabilityLabels = top.Select(x => x.Label).ToArray();
            _viewModel.ProfitabilitySeries = new SeriesCollection
    {
        new ColumnSeries
        {
            Title = "Прибыль",
            Values = new ChartValues<decimal>(top.Select(x => x.Profit)),
            Fill = System.Windows.Media.Brushes.Green,
            DataLabels = true,
            LabelPoint = point => point.Y.ToString("N0") + " ₽"
        }
    };
        }

        private void LoadTaskPerformancePie(DiaShopEntities context)
        {
            var tasks = context.Task
                .Where(t => t.User.StoreId == _currentStoreId && t.Deadline.HasValue)
                .ToList();

            int total = tasks.Count;
            int onTime = tasks.Count(t => t.IsCompleted && t.EndAt <= t.Deadline.Value);
            int notOnTime = total - onTime;

            var series = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "В срок",
                    Values = new ChartValues<int> { onTime },
                    DataLabels = true,
                    LabelPoint = point => $"{point.Y} ({(total > 0 ? (double)onTime / total * 100 : 0):F1}%)"
                },
                new PieSeries
                {
                    Title = "С опозданием",
                    Values = new ChartValues<int> { notOnTime },
                    DataLabels = true,
                    LabelPoint = point => $"{point.Y} ({(total > 0 ? (double)notOnTime / total * 100 : 0):F1}%)"
                }
            };

            _viewModel.TaskPerformancePieSeries = series;
        }
    }
}