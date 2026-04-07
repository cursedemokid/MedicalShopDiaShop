using MedicalShopDiaShop.MainView.Dto;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MedicalShopDiaShop.MainView.Pages
{
    public partial class ClientsPage : Page
    {
        private ObservableCollection<UserDto> _allClients;
        private ObservableCollection<UserDto> _filteredClients;

        public ClientsPage()
        {
            InitializeComponent();
            Loaded += ClientsPage_Loaded;
        }

        private void ClientsPage_Loaded(object sender, RoutedEventArgs e)
        {
            _allClients = new ObservableCollection<UserDto>(GetStaticClients());
            _filteredClients = new ObservableCollection<UserDto>(_allClients);

            ClientsListView.ItemsSource = _filteredClients;
            ClientsListBox.ItemsSource = _filteredClients;

            ClientsListView.Visibility = Visibility.Visible;
            ClientsListBox.Visibility = Visibility.Collapsed;
            SetActiveButton(ListViewOnBtn);
        }

        private List<UserDto> GetStaticClients()
        {
            return new List<UserDto>
            {
                new UserDto
                {
                    Id = 3,
                    FirstName = "Алексей",
                    LastName = "Кузнецов",
                    MiddleName = null,
                    UserName = "alex_k",
                    Email = "alexey@mail.ru",
                    PhoneNumber = "79163456789",
                    Role = 2, // Client
                    AvatarKey = "/Resources/default_avatar.png"
                },
                new UserDto
                {
                    Id = 6,
                    FirstName = "Екатерина",
                    LastName = "Морозова",
                    MiddleName = null,
                    UserName = "katya_m",
                    Email = "katya@mail.ru",
                    PhoneNumber = "79166789012",
                    Role = 2,
                    AvatarKey = "/Resources/default_avatar.png"
                }
            };
        }


        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchBox.Text?.Trim().ToLower();
            if (string.IsNullOrEmpty(searchText))
                _filteredClients = new ObservableCollection<UserDto>(_allClients);
            else
                _filteredClients = new ObservableCollection<UserDto>(_allClients.Where(c => c.FullName.ToLower().Contains(searchText) || c.UserName.ToLower().Contains(searchText) || c.Email.ToLower().Contains(searchText)));

            ClientsListView.ItemsSource = _filteredClients;
            ClientsListBox.ItemsSource = _filteredClients;
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Добавление клиента (будет реализовано)");
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelectedClient();
            if (selected == null) MessageBox.Show("Выберите клиента.");
            else MessageBox.Show($"Изменить: {selected.FullName}");
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelectedClient();
            if (selected == null) MessageBox.Show("Выберите клиента.");
            else if (MessageBox.Show($"Удалить {selected.FullName}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _allClients.Remove(selected);
                _filteredClients.Remove(selected);
            }
        }

        private UserDto GetSelectedClient()
        {
            return _filteredClients.FirstOrDefault(c => c.IsSelected);
        }

        private void ListViewOnBtn_Click(object sender, RoutedEventArgs e)
        {
            ClientsListView.Visibility = Visibility.Visible;
            ClientsListBox.Visibility = Visibility.Collapsed;
            SetActiveButton(ListViewOnBtn);
        }

        private void ListBoxOnBtn_Click(object sender, RoutedEventArgs e)
        {
            ClientsListView.Visibility = Visibility.Collapsed;
            ClientsListBox.Visibility = Visibility.Visible;
            SetActiveButton(ListBoxOnBtn);
        }

        private void SetActiveButton(Button activeButton)
        {
            ListViewOnBtn.BorderBrush = Brushes.Transparent;
            ListViewOnBtn.BorderThickness = new Thickness(1);
            ListBoxOnBtn.BorderBrush = Brushes.Transparent;
            ListBoxOnBtn.BorderThickness = new Thickness(1);
            activeButton.BorderBrush = Brushes.Black;
            activeButton.BorderThickness = new Thickness(2);
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var client = btn?.Tag as UserDto;
            if (client != null)
                MessageBox.Show($"Клиент: {client.FullName}\nТелефон: {client.PhoneNumber}\nEmail: {client.Email}");
        }
    }
}