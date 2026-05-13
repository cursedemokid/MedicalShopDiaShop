using MedicalShopDiaShop.AppData;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace MedicalShopDiaShop.MainView.Pages
{
    public partial class StockPage : Page
    {
        private ObservableCollection<StockItem> _allStock;
        private ObservableCollection<StockItem> _filteredStock;

        public StockPage()
        {
            InitializeComponent();
            Loaded += StockPage_Loaded;
        }

        private void StockPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            LoadStock();
        }

        private void LoadStock()
        {
            if (!App.currentUser.StoreId.HasValue)
            {
                _allStock = new ObservableCollection<StockItem>();
                _filteredStock = new ObservableCollection<StockItem>();
                StockListView.ItemsSource = _filteredStock;
                return;
            }

            var list = StockHelper.GetCurrentStock(App.currentUser.StoreId.Value);
            _allStock = new ObservableCollection<StockItem>(list);
            ApplyStockFilter();
        }

        private void ApplyStockFilter()
        {
            if (_allStock == null) return;

            string search = PageSearchBar.Text?.Trim().ToLowerInvariant() ?? string.Empty;
            if (string.IsNullOrEmpty(search))
                _filteredStock = new ObservableCollection<StockItem>(_allStock);
            else
                _filteredStock = new ObservableCollection<StockItem>(
                    _allStock.Where(s =>
                        (s.ProductName ?? string.Empty).ToLowerInvariant().Contains(search)));

            StockListView.ItemsSource = _filteredStock;
        }

        private void PageSearchBar_FilterTextChanged(object sender, TextChangedEventArgs e) => ApplyStockFilter();

        private void PageSearchBar_SearchClicked(object sender, System.Windows.RoutedEventArgs e) => ApplyStockFilter();
    }
}
