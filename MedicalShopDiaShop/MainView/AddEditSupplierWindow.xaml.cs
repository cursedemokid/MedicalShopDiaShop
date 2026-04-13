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
                    MessageBox.Show("Поставщик не найден.");
                    Close();
                    return;
                }

                NameTb.Text = _editingStore.Name;
                AddressTb.Text = _editingStore.Address;
                CityComboBox.SelectedValue = _editingStore.City;
            }
        }

        private void LoadCities()
        {
            var cities = Enum.GetValues(typeof(City))
                .Cast<City>()
                .Select(c => new { Id = (int)c, Name = c.ToString() })
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
                MessageBox.Show("Заполните все поля.");
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