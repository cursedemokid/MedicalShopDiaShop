using MedicalShopDiaShop.Database;
using MedicalShopDiaShop.AppData;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static MedicalShopDiaShop.AppData.Status;
using MedicalShopDiaShop.MainView;

namespace MedicalShopDiaShop.MainView.Pages
{
    public partial class ClientsPage : Page
    {
        private ObservableCollection<ClientItem> _allClients;
        private ObservableCollection<ClientItem> _filteredClients;

        public ClientsPage()
        {
            InitializeComponent();
            Loaded += ClientsPage_Loaded;
        }

        private void ClientsPage_Loaded(object sender, RoutedEventArgs e)
        {
            ClientsListView.Visibility = Visibility.Visible;
            ClientsListBox.Visibility = Visibility.Collapsed;
            SetActiveButton(ListViewOnBtn);

            LoadClientsFromDatabase();
        }

        private void LoadClientsFromDatabase()
        {
            // Загружаем только клиентов (Role = Client), не удалённых
            var dbClients = App.context.User
                .Where(u => u.Role == (int)Role.Client && u.IsDeleted != true)
                .ToList();

            var items = dbClients.Select(u => new ClientItem
            {
                Id = u.Id,
                FullName = $"{u.LastName} {u.FirstName} {u.MiddleName}".Trim(),
                UserName = u.UserName,
                PhoneNumber = u.PhoneNumber,
                Email = u.Email,
                AvatarKey = string.IsNullOrEmpty(u.AvatarKey)
                    ? "/Resources/default_avatar.png"
                    : $"/Resources/Avatars/{u.AvatarKey}",
                Address = u.Address,
                IsSelected = false
            });

            _allClients = new ObservableCollection<ClientItem>(items);
            _filteredClients = new ObservableCollection<ClientItem>(_allClients);

            SubscribeToClientChanges(_filteredClients);
            ClientsListView.ItemsSource = _filteredClients;
            ClientsListBox.ItemsSource = _filteredClients;
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchBox.Text?.Trim().ToLower();
            if (string.IsNullOrEmpty(searchText))
            {
                _filteredClients = new ObservableCollection<ClientItem>(_allClients);
            }
            else
            {
                var filtered = _allClients.Where(c =>
                    c.FullName.ToLower().Contains(searchText) ||
                    c.UserName.ToLower().Contains(searchText) ||
                    c.Email.ToLower().Contains(searchText));
                _filteredClients = new ObservableCollection<ClientItem>(filtered);
            }

            SubscribeToClientChanges(_filteredClients);
            ClientsListView.ItemsSource = _filteredClients;
            ClientsListBox.ItemsSource = _filteredClients;
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditUserWindow(isClientMode: true);
            if (window.ShowDialog() == true)
                LoadClientsFromDatabase();
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelectedClient();
            if (selected == null)
            {
                FeedbackService.Warning("Не выбран ни один клиент для изменения.", "Изменение");
                return;
            }
            var window = new AddEditUserWindow(selected.Id, isClientMode: true);
            if (window.ShowDialog() == true)
                LoadClientsFromDatabase();
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelectedClient();
            if (selected == null)
            {
                FeedbackService.Warning("Не выбран ни один клиент для удаления.", "Удаление");
                return;
            }

            var result = FeedbackService.Question(
                $"Вы действительно хотите удалить клиента: {selected.FullName}?",
                "Подтверждение удаления");

            if (result == MessageBoxResult.Yes)
            {
                var dbUser = App.context.User.FirstOrDefault(u => u.Id == selected.Id);
                if (dbUser != null)
                {
                    // Мягкое удаление: IsDeleted = true
                    dbUser.IsDeleted = true;
                    App.context.SaveChanges();
                }
                LoadClientsFromDatabase();
                FeedbackService.Information("Клиент удалён.", "Удаление");
            }
        }

        private ClientItem GetSelectedClient()
        {
            return _filteredClients?.FirstOrDefault(c => c.IsSelected);
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
            var button = sender as Button;
            var client = button?.Tag as ClientItem;
            if (client == null)
            {
                FeedbackService.Error("Не удалось получить данные клиента.");
                return;
            }

            // Получаем главное окно через визуальный родитель текущей страницы
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow == null)
            {
                FeedbackService.Error("Не удалось получить главное окно.");
                return;
            }

            mainWindow.MainFrame.Navigate(new ProfilePage(client.Id));
        }

        // --- Логика единственного выделения ---
        private void SubscribeToClientChanges(ObservableCollection<ClientItem> clients)
        {
            foreach (var c in clients)
                c.PropertyChanged += OnClientPropertyChanged;
            clients.CollectionChanged += OnClientsCollectionChanged;
        }

        private void OnClientsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (ClientItem c in e.NewItems)
                    c.PropertyChanged += OnClientPropertyChanged;
            if (e.OldItems != null)
                foreach (ClientItem c in e.OldItems)
                    c.PropertyChanged -= OnClientPropertyChanged;
        }

        private void OnClientPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ClientItem.IsSelected))
            {
                var changed = sender as ClientItem;
                if (changed != null && changed.IsSelected)
                {
                    foreach (var c in _filteredClients)
                        if (c != changed && c.IsSelected)
                            c.IsSelected = false;
                }
            }
        }
    }

    // Модель клиента с поддержкой INotifyPropertyChanged
    public class ClientItem : INotifyPropertyChanged
    {
        public int Id { get; set; }
        private bool _isSelected;
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string AvatarKey { get; set; }
        public string Address { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}