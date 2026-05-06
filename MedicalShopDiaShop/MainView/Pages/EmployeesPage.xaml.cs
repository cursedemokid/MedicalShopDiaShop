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

namespace MedicalShopDiaShop.MainView.Pages
{
    public partial class EmployeesPage : Page
    {
        private ObservableCollection<EmployeeItem> _employees;

        public EmployeesPage()
        {
            InitializeComponent();
            Loaded += EmployeesPage_Loaded;
        }

        private void EmployeesPage_Loaded(object sender, RoutedEventArgs e)
        {
            EmployeesListView.Visibility = Visibility.Visible;
            EmployeesListBox.Visibility = Visibility.Collapsed;
            SetActiveButton(ListViewOnBtn);

            LoadEmployeesFromDatabase();
        }

        // Загрузка сотрудников из БД и преобразование в EmployeeItem
        private void LoadEmployeesFromDatabase()
        {
            // Получаем сотрудников (роли Admin, Worker, Courier)
            var dbEmployees = App.context.User
                .Where(u => u.Role == (int)Role.Admin ||
                            u.Role == (int)Role.Worker ||
                            u.Role == (int)Role.Courier &&
                            u.IsDeleted != true)
                .ToList();

            var items = dbEmployees.Select(u => new EmployeeItem
            {
                Id = u.Id,
                FullName = $"{u.LastName} {u.FirstName} {u.MiddleName}".Trim(),
                UserName = u.UserName,
                RoleName = GetRoleName(u.Role),
                PhoneNumber = u.PhoneNumber,
                Email = u.Email,
                AvatarKey = string.IsNullOrEmpty(u.AvatarKey)
                    ? "/Resources/default_avatar.png"
                    : $"/Resources/Avatars/{u.AvatarKey}",
                IsSelected = false
            });

            _employees = new ObservableCollection<EmployeeItem>(items);
            SubscribeToEmployeeChanges(_employees);
            EmployeesListView.ItemsSource = _employees;
            EmployeesListBox.ItemsSource = _employees;
        }

        private string GetRoleName(int roleId)
        {
            switch (roleId)
            {
                case (int)Role.Admin: return "Администратор";
                case (int)Role.Worker: return "Сотрудник";
                case (int)Role.Courier: return "Курьер";
                default: return "Неизвестно";
            }
        }

        // Добавление нового сотрудника
        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditUserWindow();
            if (window.ShowDialog() == true)
            {
                LoadEmployeesFromDatabase(); // Обновляем список
            }
        }

        // Редактирование выбранного сотрудника
        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelectedEmployee();
            if (selected == null)
            {
                FeedbackService.Warning("Не выбран ни один сотрудник для изменения.", "Изменение");
                return;
            }

            var window = new AddEditUserWindow(selected.Id);
            if (window.ShowDialog() == true)
            {
                LoadEmployeesFromDatabase();
            }
        }

        // Удаление сотрудника
        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelectedEmployee();
            if (selected == null)
            {
                FeedbackService.Warning("Не выбран ни один сотрудник для удаления.", "Удаление");
                return;
            }

            var result = FeedbackService.Question(
                $"Вы действительно хотите удалить сотрудника: {selected.FullName}?",
                "Подтверждение удаления");

            if (result == MessageBoxResult.Yes)
            {
                var dbUser = App.context.User.FirstOrDefault(u => u.Id == selected.Id);
                if (dbUser != null)
                {
                    App.context.User.Remove(dbUser);
                    App.context.SaveChanges();
                }
                LoadEmployeesFromDatabase();
                FeedbackService.Information("Сотрудник удалён.", "Удаление");
            }
        }

        private void ListViewOnBtn_Click(object sender, RoutedEventArgs e)
        {
            EmployeesListView.Visibility = Visibility.Visible;
            EmployeesListBox.Visibility = Visibility.Collapsed;
            SetActiveButton(ListViewOnBtn);
        }

        private void ListBoxOnBtn_Click(object sender, RoutedEventArgs e)
        {
            EmployeesListView.Visibility = Visibility.Collapsed;
            EmployeesListBox.Visibility = Visibility.Visible;
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
            var employee = button?.Tag as EmployeeItem;
            if (employee != null)
            {
                // Получаем окно, содержащее текущую страницу
                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.MainFrame.Navigate(new ProfilePage(employee.Id));
                }
                else
                {
                    FeedbackService.Error("Не удалось получить главное окно.");
                }
            }
        }

        private EmployeeItem GetSelectedEmployee()
        {
            return _employees?.FirstOrDefault(e => e.IsSelected);
        }

        // --- Логика единственного выделения ---
        private void SubscribeToEmployeeChanges(ObservableCollection<EmployeeItem> employees)
        {
            foreach (var emp in employees)
                emp.PropertyChanged += OnEmployeePropertyChanged;
            employees.CollectionChanged += OnEmployeesCollectionChanged;
        }

        private void UnsubscribeFromEmployeeChanges(ObservableCollection<EmployeeItem> employees)
        {
            employees.CollectionChanged -= OnEmployeesCollectionChanged;
            foreach (var emp in employees)
                emp.PropertyChanged -= OnEmployeePropertyChanged;
        }

        private void OnEmployeesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (EmployeeItem emp in e.NewItems)
                    emp.PropertyChanged += OnEmployeePropertyChanged;
            if (e.OldItems != null)
                foreach (EmployeeItem emp in e.OldItems)
                    emp.PropertyChanged -= OnEmployeePropertyChanged;
        }

        private void OnEmployeePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EmployeeItem.IsSelected))
            {
                var changed = sender as EmployeeItem;
                if (changed != null && changed.IsSelected)
                {
                    foreach (var emp in _employees)
                        if (emp != changed && emp.IsSelected)
                            emp.IsSelected = false;
                }
            }
        }

        // Предотвращаем выделение в ListView (мы используем свой чекбокс)
        private void EmployeesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EmployeesListView.SelectedItem != null)
                EmployeesListView.SelectedItem = null;
        }
    }

    // Модель EmployeeItem (добавлен Id)
    public class EmployeeItem : INotifyPropertyChanged
    {
        public int Id { get; set; }
        private bool _isSelected;
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string RoleName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string AvatarKey { get; set; }

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