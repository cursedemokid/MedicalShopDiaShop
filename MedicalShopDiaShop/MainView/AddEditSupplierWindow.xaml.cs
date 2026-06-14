using MedicalShopDiaShop.AppData;
using MedicalShopDiaShop.Database;
using System;
using System.Linq;
using System.Windows;
using static MedicalShopDiaShop.AppData.Status;

namespace MedicalShopDiaShop.MainView
{
    public partial class AddEditSupplierWindow : Window
    {
        private readonly int? _supplierId;
        private Store _editingStore;

        public AddEditSupplierWindow()
        {
            InitializeComponent();
            Title = "Добавление поставщика";
        }

        public AddEditSupplierWindow(int supplierId)
        {
            InitializeComponent();
            _supplierId = supplierId;
            Title = "Редактирование поставщика";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCities();

            if (_supplierId.HasValue)
            {
                _editingStore = App.context.Store.FirstOrDefault(s => s.Id == _supplierId.Value);
                if (_editingStore == null)
                {
                    FeedbackService.Error("Поставщик не найден.");
                    Close();
                    return;
                }

                NameTb.Text = _editingStore.Name;
                AddressTb.Text = _editingStore.Address;
                CityComboBox.SelectedValue = _editingStore.City;
            }
        }
        private string GetCityName(City city)
        {
            switch (city)
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
                default: return city.ToString();
            }
        }

        private void LoadCities()
        {
            var cities = Enum.GetValues(typeof(City))
                .Cast<City>()
                .Select(c => new { Id = (int)c, Name = GetCityName(c) })
                .ToList();

            CityComboBox.ItemsSource = cities;
            CityComboBox.SelectedIndex = 0;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTb.Text) ||
                string.IsNullOrWhiteSpace(AddressTb.Text) ||
                CityComboBox.SelectedValue == null)
            {
                FeedbackService.Error("Заполните все поля.");
                return;
            }

            if (_supplierId.HasValue && _editingStore != null)
            {
                // Редактирование
                _editingStore.Name = NameTb.Text.Trim();
                _editingStore.Address = AddressTb.Text.Trim();
                _editingStore.City = (int)CityComboBox.SelectedValue;
            }
            else
            {
                // Добавление
                var newStore = new Store
                {
                    Name = NameTb.Text.Trim(),
                    Address = AddressTb.Text.Trim(),
                    City = (int)CityComboBox.SelectedValue,
                    Type = (int)StoreType.SupplierStore
                };
                App.context.Store.Add(newStore);
            }

            App.context.SaveChanges();
            DialogResult = true;
            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}