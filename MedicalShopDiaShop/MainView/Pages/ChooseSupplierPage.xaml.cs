using MedicalShopDiaShop.Database;
using MedicalShopDiaShop.MainView;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using static MedicalShopDiaShop.AppData.Status;

namespace MedicalShopDiaShop.MainView.Pages
{
    public partial class ChooseSupplierPage : Page
    {
        private readonly AddSupplyWindow _parentWindow;
        private ObservableCollection<SupplierItem> _suppliers;

        public ChooseSupplierPage(AddSupplyWindow parent)
        {
            InitializeComponent();
            _parentWindow = parent;
            LoadSuppliers();
        }

        private void LoadSuppliers()
        {
            var dbSuppliers = App.context.Store
                .Where(s => s.Type == (int)StoreType.SupplierStore)
                .ToList();

            var items = dbSuppliers.Select(s => new SupplierItem
            {
                Id = s.Id,
                Name = s.Name,
                Address = s.Address,
                City = s.City,
                CityName = GetCityName(s.City),
                IsSelected = false
            });

            _suppliers = new ObservableCollection<SupplierItem>(items);
            foreach (var s in _suppliers)
                s.PropertyChanged += OnSupplierPropertyChanged;

            SuppliersListBox.ItemsSource = _suppliers;
        }

        private string GetCityName(int cityId)
        {
            switch ((City)cityId)
            {
                case City.Moscow: return "Москва";
                case City.SaintPetersburg: return "Санкт-Петербург";
                case City.Novosibirsk: return "Новосибирск";
                case City.Yekaterinburg: return "Екатеринбург";
                case City.Kazan: return "Казань";
                case City.NizhnyNovgorod: return "Нижний Новгород";
                case City.Chelyabinsk: return "Челябинск";
                case City.Omsk: return "Омск";
                case City.RostovOnDon: return "Ростов-на-Дону";
                case City.Ufa: return "Уфа";
                case City.Krasnoyarsk: return "Красноярск";
                case City.Perm: return "Пермь";
                case City.Voronezh: return "Воронеж";
                case City.Volgograd: return "Волгоград";
                case City.Krasnodar: return "Краснодар";
                default: return "Неизвестно";
            }
        }

        private void OnSupplierPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SupplierItem.IsSelected))
            {
                var changed = sender as SupplierItem;
                if (changed.IsSelected)
                {
                    // Снимаем выделение с остальных
                    foreach (var s in _suppliers)
                        if (s != changed && s.IsSelected)
                            s.IsSelected = false;

                    _parentWindow.SelectedSupplier = App.context.Store.Find(changed.Id);
                    ContinueButton.IsEnabled = true;
                }
                else
                {
                    // Если сняли выделение и больше никто не выбран
                    if (!_suppliers.Any(s => s.IsSelected))
                    {
                        _parentWindow.SelectedSupplier = null;
                        ContinueButton.IsEnabled = false;
                    }
                }
            }
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            _parentWindow.GoToNextStep();
        }

        public class SupplierItem : INotifyPropertyChanged
        {
            public int Id { get; set; }
            private bool _isSelected;
            public string Name { get; set; }
            public string Address { get; set; }
            public int City { get; set; }
            public string CityName { get; set; }

            public bool IsSelected
            {
                get => _isSelected;
                set { _isSelected = value; OnPropertyChanged(); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string prop = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}