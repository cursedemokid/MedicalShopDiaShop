using MedicalShopDiaShop.AppData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MedicalShopDiaShop.MainView.Pages
{
    public partial class EmployeesPage : Page
    {
        // Текущий список сотрудников (используем ObservableCollection для автоматического обновления UI)
        private ObservableCollection<EmployeeItem> _employees;

        public EmployeesPage()
        {
            InitializeComponent();
            Loaded += EmployeesPage_Loaded;
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            // TODO: открыть окно добавления нового сотрудника
            FeedbackService.Information("Функция добавления сотрудника будет реализована позже.", "Добавление");
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedEmployee = GetSelectedEmployee();
            if (selectedEmployee == null)
            {
                FeedbackService.Warning("Не выбран ни один сотрудник для изменения.", "Изменение");
                return;
            }

            // TODO: открыть окно редактирования сотрудника
            FeedbackService.Information($"Выбран сотрудник: {selectedEmployee.FullName}\nРедактирование будет реализовано позже.", "Изменение");
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedEmployee = GetSelectedEmployee();
            if (selectedEmployee == null)
            {
                FeedbackService.Warning("Не выбран ни один сотрудник для удаления.", "Удаление");
                return;
            }

            var result = MessageBox.Show($"Вы действительно хотите удалить сотрудника: {selectedEmployee.FullName}?",
                                         "Подтверждение удаления",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _employees.Remove(selectedEmployee);
                RefreshLists();
                FeedbackService.Information("Сотрудник удален.", "Удаление");
            }
        }

        private void EmployeesPage_Loaded(object sender, RoutedEventArgs e)
        {
            EmployeesListView.Visibility = Visibility.Visible;
            EmployeesListBox.Visibility = Visibility.Collapsed;
            SetActiveButton(ListViewOnBtn);

            // Инициализация списка с подпиской на изменения
            _employees = new ObservableCollection<EmployeeItem>(GetTestEmployees());
            SubscribeToEmployeeChanges(_employees);
            EmployeesListView.ItemsSource = _employees;
            EmployeesListBox.ItemsSource = _employees;
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

        private List<EmployeeItem> GetTestEmployees()
        {
            return new List<EmployeeItem>
            {
                new EmployeeItem
                {
                    FullName = "Иванов Иван Иванович",
                    UserName = "ivan_admin",
                    RoleName = "Администратор",
                    PhoneNumber = "+7 (999) 123-45-67",
                    Email = "ivan@diashop.ru",
                    AvatarKey = "/Resources/default_avatar.png"
                },
                new EmployeeItem
                {
                    FullName = "Петрова Анна Сергеевна",
                    UserName = "anna_worker",
                    RoleName = "Сотрудник",
                    PhoneNumber = "+7 (999) 234-56-78",
                    Email = "anna@diashop.ru",
                    AvatarKey = "/Resources/default_avatar.png"
                },
                new EmployeeItem
                {
                    FullName = "Сидоров Алексей Дмитриевич",
                    UserName = "alex_courier",
                    RoleName = "Курьер",
                    PhoneNumber = "+7 (999) 345-67-89",
                    Email = "alex@diashop.ru",
                    AvatarKey = "/Resources/default_avatar.png"
                },
                new EmployeeItem
                {
                    FullName = "Козлова Елена Викторовна",
                    UserName = "elena_worker",
                    RoleName = "Сотрудник",
                    PhoneNumber = "+7 (999) 456-78-90",
                    Email = "elena@diashop.ru",
                    AvatarKey = "/Resources/default_avatar.png"
                },
                new EmployeeItem
                {
                    FullName = "Николаев Дмитрий Андреевич",
                    UserName = "dmitry_admin",
                    RoleName = "Администратор",
                    PhoneNumber = "+7 (999) 567-89-01",
                    Email = "dmitry@diashop.ru",
                    AvatarKey = "/Resources/default_avatar.png"
                }
            };
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var employee = button?.Tag as EmployeeItem;
            if (employee != null)
            {
                FeedbackService.Information(
                    $"ФИО: {employee.FullName}\nРоль: {employee.RoleName}\nТелефон: {employee.PhoneNumber}\nEmail: {employee.Email}",
                    "Подробнее о сотруднике");
            }
        }

        /// <summary>
        /// Возвращает выбранного сотрудника (у которого IsSelected == true) или null.
        /// </summary>
        private EmployeeItem GetSelectedEmployee()
        {
            return _employees?.FirstOrDefault(e => e.IsSelected);
        }

        /// <summary>
        /// Обновляет отображение в обоих контролах (сохраняя текущий видимый).
        /// </summary>
        private void RefreshLists()
        {
            if (_employees == null) return;

            // Переподписываемся на изменения (на случай, если были замены объектов)
            UnsubscribeFromEmployeeChanges(_employees);
            SubscribeToEmployeeChanges(_employees);

            // Обновляем привязки
            EmployeesListView.ItemsSource = null;
            EmployeesListView.ItemsSource = _employees;
            EmployeesListBox.ItemsSource = null;
            EmployeesListBox.ItemsSource = _employees;
        }

        #region Логика выделения только одного сотрудника

        /// <summary>
        /// Подписывается на событие PropertyChanged каждого сотрудника в коллекции.
        /// </summary>
        private void SubscribeToEmployeeChanges(ObservableCollection<EmployeeItem> employees)
        {
            foreach (var emp in employees)
            {
                emp.PropertyChanged += OnEmployeePropertyChanged;
            }
            // Подписываемся на изменение коллекции (добавление/удаление)
            employees.CollectionChanged += OnEmployeesCollectionChanged;
        }

        /// <summary>
        /// Отписывается от событий сотрудников.
        /// </summary>
        private void UnsubscribeFromEmployeeChanges(ObservableCollection<EmployeeItem> employees)
        {
            employees.CollectionChanged -= OnEmployeesCollectionChanged;
            foreach (var emp in employees)
            {
                emp.PropertyChanged -= OnEmployeePropertyChanged;
            }
        }

        /// <summary>
        /// Обработчик изменения коллекции: подписывается на новых сотрудников.
        /// </summary>
        private void OnEmployeesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (EmployeeItem emp in e.NewItems)
                {
                    emp.PropertyChanged += OnEmployeePropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (EmployeeItem emp in e.OldItems)
                {
                    emp.PropertyChanged -= OnEmployeePropertyChanged;
                }
            }
        }

        /// <summary>
        /// Обработчик изменения свойства сотрудника.
        /// Реализует правило: может быть выбран только один сотрудник.
        /// </summary>
        private void OnEmployeePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EmployeeItem.IsSelected))
            {
                var changedEmployee = sender as EmployeeItem;
                if (changedEmployee != null && changedEmployee.IsSelected)
                {
                    // Сбрасываем выделение у всех остальных сотрудников
                    foreach (var emp in _employees)
                    {
                        if (emp != changedEmployee && emp.IsSelected)
                        {
                            emp.IsSelected = false;
                        }
                    }
                }
            }
        }

        private void EmployeesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Мгновенно снимаем выделение
            if (EmployeesListView.SelectedItem != null)
                EmployeesListView.SelectedItem = null;
        }

        #endregion
    }

    /// <summary>
    /// Модель сотрудника с поддержкой уведомлений об изменениях.
    /// </summary>
    public class EmployeeItem : INotifyPropertyChanged
    {
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