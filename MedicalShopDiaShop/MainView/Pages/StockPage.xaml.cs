using MedicalShopDiaShop.AppData;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            SearchBox.TextChanged += SearchBox_TextChanged;
        }

        private void StockPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadStock();
        }

        private void LoadStock()
        {
            _allStock = new ObservableCollection<StockItem>(StockHelper.GetCurrentStock(App.currentUser.StoreId));
            _filteredStock = new ObservableCollection<StockItem>(_allStock);
            StockListView.ItemsSource = _filteredStock;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string search = SearchBox.Text?.Trim().ToLower() ?? "";
            if (string.IsNullOrEmpty(search))
                _filteredStock = new ObservableCollection<StockItem>(_allStock);
            else
                _filteredStock = new ObservableCollection<StockItem>(_allStock.Where(s => s.ProductName.ToLower().Contains(search)));
            StockListView.ItemsSource = _filteredStock;
        }
    }

    // Расширение для отображения статуса в UI
    public partial class StockItem
    {
        public string StatusText
        {
            get
            {
                if (IsExpired) return "Просрочен";
                if (IsExpiringSoon) return "Скоро истекает";
                return "Годен";
            }
        }

        public Brush StatusColor
        {
            get
            {
                if (IsExpired) return Brushes.Red;
                if (IsExpiringSoon) return Brushes.Orange;
                return Brushes.Green;
            }
        }
    }
}