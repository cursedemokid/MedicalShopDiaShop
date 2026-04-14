using LiveCharts;
using LiveCharts.Wpf;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MedicalShopDiaShop.MainView.ViewModels
{
    public class StatisticsViewModel : INotifyPropertyChanged
    {
        private decimal _totalRevenueAllTime;
        public decimal TotalRevenueAllTime
        {
            get => _totalRevenueAllTime;
            set { _totalRevenueAllTime = value; OnPropertyChanged(); }
        }

        private int _totalOrdersAllTime;
        public int TotalOrdersAllTime
        {
            get => _totalOrdersAllTime;
            set { _totalOrdersAllTime = value; OnPropertyChanged(); }
        }

        private SeriesCollection _monthlyRevenueSeries;
        public SeriesCollection MonthlyRevenueSeries
        {
            get => _monthlyRevenueSeries;
            set { _monthlyRevenueSeries = value; OnPropertyChanged(); }
        }

        private string[] _monthlyRevenueLabels;
        public string[] MonthlyRevenueLabels
        {
            get => _monthlyRevenueLabels;
            set { _monthlyRevenueLabels = value; OnPropertyChanged(); }
        }

        private SeriesCollection _supplierPieSeries;
        public SeriesCollection SupplierPieSeries
        {
            get => _supplierPieSeries;
            set { _supplierPieSeries = value; OnPropertyChanged(); }
        }

        private SeriesCollection _topProductsSeries;
        public SeriesCollection TopProductsSeries
        {
            get => _topProductsSeries;
            set { _topProductsSeries = value; OnPropertyChanged(); }
        }

        private string[] _topProductsLabels;
        public string[] TopProductsLabels
        {
            get => _topProductsLabels;
            set { _topProductsLabels = value; OnPropertyChanged(); }
        }

        private SeriesCollection _orderCountMonthlySeries;
        public SeriesCollection OrderCountMonthlySeries
        {
            get => _orderCountMonthlySeries;
            set { _orderCountMonthlySeries = value; OnPropertyChanged(); }
        }

        private SeriesCollection _deliveryTypePieSeries;
        public SeriesCollection DeliveryTypePieSeries
        {
            get => _deliveryTypePieSeries;
            set { _deliveryTypePieSeries = value; OnPropertyChanged(); }
        }

        private SeriesCollection _profitabilitySeries;
        public SeriesCollection ProfitabilitySeries
        {
            get => _profitabilitySeries;
            set { _profitabilitySeries = value; OnPropertyChanged(); }
        }

        private string[] _profitabilityLabels;
        public string[] ProfitabilityLabels
        {
            get => _profitabilityLabels;
            set { _profitabilityLabels = value; OnPropertyChanged(); }
        }

        private SeriesCollection _taskPerformancePieSeries;
        public SeriesCollection TaskPerformancePieSeries
        {
            get => _taskPerformancePieSeries;
            set { _taskPerformancePieSeries = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}