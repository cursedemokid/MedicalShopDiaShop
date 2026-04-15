using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MedicalShopDiaShop.AppData;
using MedicalShopDiaShop.Database;
using static MedicalShopDiaShop.AppData.Status;

namespace MedicalShopDiaShop.MainView.Pages
{
    public partial class ProfilePage : Page
    {
        private Database.User _displayedUser;
        private bool _isAdmin => App.currentUser.Role == (int)Role.Admin;

        // Для расписания
        public IEnumerable<DateTime> ScheduledDates { get; set; }
        public Func<DateTime, object> ScheduleToolTipSelector { get; set; }
        private ObservableCollection<ScheduleItem> _schedulesForSelectedDate;

        // Для задач
        private int _tasksCurrentPage = 1;
        private int _tasksTotalPages = 1;
        private const int TasksPageSize = 4;
        private ObservableCollection<TaskItem> _taskItems;

        // Для истории покупок
        private List<OrderHistoryItem> _allHistory;
        private ObservableCollection<OrderHistoryItem> _filteredHistory;

        public ProfilePage()
        {
            InitializeComponent();
            LoadUserData(App.currentUser.Id);
        }

        public ProfilePage(int userId)
        {
            InitializeComponent();
            LoadUserData(userId);
        }

        private void LoadUserData(int userId)
        {
            _displayedUser = App.context.User.FirstOrDefault(u => u.Id == userId);
            if (_displayedUser == null)
            {
                MessageBox.Show("Пользователь не найден");
                return;
            }

            LoadPersonalInfo();

            if (_displayedUser.Role == (int)Role.Client)
            {
                ClientProfileGrid.Visibility = Visibility.Visible;
                EmployeeProfileGrid.Visibility = Visibility.Collapsed;

                // Показываем адрес, скрываем магазин
                EmployeeTextBlock.Visibility = Visibility.Collapsed;
                StoreTextBlock.Visibility = Visibility.Collapsed;
                ClientTextBlock.Visibility = Visibility.Visible;
                AddressTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                ClientProfileGrid.Visibility = Visibility.Collapsed;
                EmployeeProfileGrid.Visibility = Visibility.Visible;

                // Показываем магазин, скрываем адрес
                EmployeeTextBlock.Visibility = Visibility.Visible;
                StoreTextBlock.Visibility = Visibility.Visible;
                ClientTextBlock.Visibility = Visibility.Collapsed;
                AddressTextBlock.Visibility = Visibility.Collapsed;
            }

            // Кнопки смены пароля
            ChangePasswordButton.Visibility = (App.currentUser.Id == userId || _isAdmin) ? Visibility.Visible : Visibility.Collapsed;

            // Кнопки управления расписанием и задачами видны только админу
            AddScheduleBtn.Visibility = _isAdmin ? Visibility.Visible : Visibility.Collapsed;
            EditScheduleBtn.Visibility = _isAdmin ? Visibility.Visible : Visibility.Collapsed;
            DeleteScheduleBtn.Visibility = _isAdmin ? Visibility.Visible : Visibility.Collapsed;
            AddTaskBtn.Visibility = _isAdmin ? Visibility.Visible : Visibility.Collapsed;
            EditTaskBtn.Visibility = _isAdmin ? Visibility.Visible : Visibility.Collapsed;
            DeleteTaskBtn.Visibility = _isAdmin ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadPersonalInfo()
        {
            string fullName = $"{_displayedUser.LastName} {_displayedUser.FirstName}";
            if (!string.IsNullOrEmpty(_displayedUser.MiddleName))
                fullName += $" {_displayedUser.MiddleName}";
            FullNameTextBlock.Text = fullName;
            UserNameTextBlock.Text = _displayedUser.UserName;
            RoleTextBlock.Text = GetRoleName(_displayedUser.Role);

            var store = App.context.Store.FirstOrDefault(s => s.Id == _displayedUser.StoreId);
            StoreTextBlock.Text = store != null ? $"{store.Name}, {store.Address}" : "Не указан";
            AddressTextBlock.Text = !string.IsNullOrEmpty(_displayedUser.Address) ? _displayedUser.Address : "Не указан";

            string avatarPath = string.IsNullOrEmpty(_displayedUser.AvatarKey)
                ? "/Resources/avatarka.png"
                : $"/Resources/Avatars/{_displayedUser.AvatarKey}";
            AvatarImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(avatarPath, UriKind.Relative));
        }

        private string GetRoleName(int roleId)
        {
            switch (roleId)
            {
                case (int)Role.Admin: return "Администратор";
                case (int)Role.Client: return "Клиент";
                case (int)Role.Worker: return "Сотрудник";
                case (int)Role.Courier: return "Курьер";
                default: return "Неизвестно";
            }
        }

        #region Клиент: история покупок

        private void LoadClientHistory()
        {
            var orders = App.context.Order
                .Where(o => o.ClientId == _displayedUser.Id)
                .OrderByDescending(o => o.DateTime)
                .ToList();

            _allHistory = new List<OrderHistoryItem>();
            foreach (var order in orders)
            {
                var orderItems = App.context.ProductOrder
                    .Where(po => po.OrderId == order.Id)
                    .Select(po => new OrderItem
                    {
                        ImageSource = GetImagePath(po.Product.Image),
                        Name = po.Product.Name,
                        Quantity = po.Quantity,
                        PricePerUnit = po.Price
                    }).ToList();

                _allHistory.Add(new OrderHistoryItem
                {
                    OrderDate = order.DateTime,
                    Items = new ObservableCollection<OrderItem>(orderItems),
                    TotalOrderPrice = orderItems.Sum(i => i.TotalPrice),
                    ViewDetailsCommand = new RelayCommand(() => ShowDetails($"Заказ №{order.Id}"))
                });
            }
            ApplyHistoryFilterAndSort();
        }

        private void ApplyHistoryFilterAndSort()
        {
            var query = _allHistory.AsEnumerable();

            // Фильтр по дате
            if (HistoryDateFilter.SelectedDate.HasValue)
                query = query.Where(h => h.OrderDate.Date == HistoryDateFilter.SelectedDate.Value.Date);

            // Поиск по тексту (если добавить поле поиска)
            if (!string.IsNullOrWhiteSpace(HistorySearchBox.Text))
            {
                string search = HistorySearchBox.Text.ToLower();
                query = query.Where(h => h.Items.Any(i => i.Name.ToLower().Contains(search)));
            }

            // Сортировка
            if (OrderByCmb.SelectedItem is ComboBoxItem selected && selected.Tag is string sort)
            {
                if (sort == "Date")
                    query = query.OrderByDescending(h => h.OrderDate);
                else if (sort == "Cost")
                    query = query.OrderByDescending(h => h.TotalOrderPrice);
                else
                    query = query.OrderByDescending(h => h.OrderDate);
            }
            else
                query = query.OrderByDescending(h => h.OrderDate);

            _filteredHistory = new ObservableCollection<OrderHistoryItem>(query);
            HistoryListBox.ItemsSource = _filteredHistory;
        }

        private void HistorySearch_Click(object sender, RoutedEventArgs e) => ApplyHistoryFilterAndSort();
        private void OrderByCmb_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyHistoryFilterAndSort();
        private void HistoryDateFilter_SelectedDateChanged(object sender, SelectionChangedEventArgs e) => ApplyHistoryFilterAndSort();

        #endregion

        #region Сотрудник: расписание

        private void LoadEmployeeScheduleAndTasks()
        {
            LoadSchedules();
            LoadTasks(1);
        }

        private void LoadSchedules()
        {
            var schedules = App.context.Schedule
                .Where(s => s.UserId == _displayedUser.Id)
                .ToList();

            ScheduledDates = schedules.Select(s => s.DateStart.Date).Distinct().ToList();

            ScheduleToolTipSelector = date =>
            {
                var daySchedules = schedules.Where(s => s.DateStart.Date == date).OrderBy(s => s.DateStart).ToList();
                if (!daySchedules.Any()) return null;
                var panel = new StackPanel();
                panel.Children.Add(new TextBlock { Text = date.ToString("dd MMMM yyyy"), FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 5) });
                foreach (var s in daySchedules)
                {
                    panel.Children.Add(new TextBlock { Text = $"• {s.DateStart:HH:mm} – {s.DateStart.AddHours(s.Hours):HH:mm}", Margin = new Thickness(0, 2, 0, 0) });
                }
                return panel;
            };
            DataContext = this;
            ScheduleCalendar.SelectedDate = DateTime.Today;
            UpdateSchedulesList(DateTime.Today);
        }

        private void ScheduleCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ScheduleCalendar.SelectedDate.HasValue)
                UpdateSchedulesList(ScheduleCalendar.SelectedDate.Value);
        }

        private void UpdateSchedulesList(DateTime date)
        {
            var allSchedules = App.context.Schedule
                .Where(s => s.UserId == _displayedUser.Id)
                .ToList();

            var schedules = allSchedules
                .Where(s => s.DateStart.Date == date.Date)
                .ToList();

            _schedulesForSelectedDate = new ObservableCollection<ScheduleItem>(
                schedules.Select(s => new ScheduleItem
                {
                    Id = s.Id,
                    DateStart = s.DateStart,
                    Hours = s.Hours,
                    DisplayText = $"{s.DateStart:HH:mm} - {s.DateStart.AddHours(s.Hours):HH:mm} ({s.Hours} ч.)"
                }));
            SchedulesListBox.ItemsSource = _schedulesForSelectedDate;
        }

        private void AddScheduleBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditScheduleWindow(_displayedUser.Id);
            if (window.ShowDialog() == true)
                LoadSchedules();
        }

        private void EditScheduleBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SchedulesListBox.SelectedItem is ScheduleItem selected)
            {
                var window = new AddEditScheduleWindow(_displayedUser.Id, selected.Id);
                if (window.ShowDialog() == true)
                    LoadSchedules();
            }
            else
                FeedbackService.Warning("Выберите смену для редактирования.");
        }

        private void DeleteScheduleBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SchedulesListBox.SelectedItem is ScheduleItem selected)
            {
                if (FeedbackService.Question($"Удалить смену {selected.DisplayText}?") == MessageBoxResult.Yes)
                {
                    var dbSchedule = App.context.Schedule.Find(selected.Id);
                    if (dbSchedule != null)
                    {
                        App.context.Schedule.Remove(dbSchedule);
                        App.context.SaveChanges();
                        LoadSchedules();
                    }
                }
            }
            else
                FeedbackService.Warning("Выберите смену для удаления.");
        }

        #endregion

        #region Сотрудник: задачи с пагинацией

        private void LoadTasks(int page)
        {
            var query = App.context.Task
                .Where(t => t.UserId == _displayedUser.Id)
                .OrderByDescending(t => t.StartAt);

            int totalCount = query.Count();
            _tasksTotalPages = (int)Math.Ceiling((double)totalCount / TasksPageSize);
            if (page < 1) page = 1;
            if (page > _tasksTotalPages && _tasksTotalPages > 0) page = _tasksTotalPages;
            _tasksCurrentPage = page;

            var tasks = query
                .Skip((page - 1) * TasksPageSize)
                .Take(TasksPageSize)
                .ToList();

            _taskItems = new ObservableCollection<TaskItem>();
            foreach (var task in tasks)
            {
                var author = App.context.User.FirstOrDefault(u => u.Id == task.AuthorId);
                _taskItems.Add(new TaskItem
                {
                    Id = task.Id,
                    Title = task.Description,
                    StartDate = task.StartAt,
                    Deadline = (DateTime)(task.Deadline ?? task.EndAt),
                    IsCompleted = task.IsCompleted,
                    AuthorName = author?.UserName ?? "Неизвестно"
                });
            }
            TasksListBox.ItemsSource = _taskItems;

            TasksPageText.Text = $"{_tasksCurrentPage} / {(_tasksTotalPages == 0 ? 1 : _tasksTotalPages)}";
            PrevTasksBtn.IsEnabled = _tasksCurrentPage > 1;
            NextTasksBtn.IsEnabled = _tasksCurrentPage < _tasksTotalPages;
        }

        private void PrevTasksBtn_Click(object sender, RoutedEventArgs e) => LoadTasks(_tasksCurrentPage - 1);
        private void NextTasksBtn_Click(object sender, RoutedEventArgs e) => LoadTasks(_tasksCurrentPage + 1);

        private void AddTaskBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditTaskWindow(_displayedUser.Id);
            if (window.ShowDialog() == true)
                LoadTasks(_tasksCurrentPage);
        }

        private void EditTaskBtn_Click(object sender, RoutedEventArgs e)
        {
            if (TasksListBox.SelectedItem is TaskItem selected)
            {
                var window = new AddEditTaskWindow(_displayedUser.Id, selected.Id);
                if (window.ShowDialog() == true)
                    LoadTasks(_tasksCurrentPage);
            }
            else
                FeedbackService.Warning("Выберите задачу для редактирования.");
        }

        private void DeleteTaskBtn_Click(object sender, RoutedEventArgs e)
        {
            if (TasksListBox.SelectedItem is TaskItem selected)
            {
                if (FeedbackService.Question($"Удалить задачу \"{selected.Title}\"?") == MessageBoxResult.Yes)
                {
                    var dbTask = App.context.Task.Find(selected.Id);
                    if (dbTask != null)
                    {
                        App.context.Task.Remove(dbTask);
                        App.context.SaveChanges();
                        LoadTasks(_tasksCurrentPage);
                    }
                }
            }
            else
                FeedbackService.Warning("Выберите задачу для удаления.");
        }

        private void TaskCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var cb = sender as CheckBox;
            var task = cb?.DataContext as TaskItem;
            if (task != null)
                UpdateTaskCompletion(task.Id, true);
        }

        private void TaskCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var cb = sender as CheckBox;
            var task = cb?.DataContext as TaskItem;
            if (task != null)
                UpdateTaskCompletion(task.Id, false);
        }

        private void UpdateTaskCompletion(int taskId, bool isCompleted)
        {
            var dbTask = App.context.Task.Find(taskId);
            if (dbTask != null)
            {
                dbTask.IsCompleted = isCompleted;
                App.context.SaveChanges();
                // Обновляем кэш
                var item = _taskItems.FirstOrDefault(t => t.Id == taskId);
                if (item != null) item.IsCompleted = isCompleted;
            }
        }

        #endregion

        private string GetImagePath(string imageName) => string.IsNullOrEmpty(imageName) ? "/Resources/placeholder.png" : imageName;
        private void ShowDetails(string orderId)
        {
            // orderId приходит в формате "Заказ №123"
            string numberStr = orderId.Replace("Заказ №", "");
            if (int.TryParse(numberStr, out int orderIdInt))
            {
                var window = new OrderDetailsWindow(orderIdInt);
                window.ShowDialog();
            }
        }

        public class RelayCommand : ICommand
        {
            private readonly Action _execute;
            public RelayCommand(Action execute) => _execute = execute;
            public event EventHandler CanExecuteChanged;
            public bool CanExecute(object parameter) => true;
            public void Execute(object parameter) => _execute();
        }

        private void ChangeAvatar_Click(object sender, RoutedEventArgs e)
        {
            // Разрешено только если это свой профиль
            if (App.currentUser.Id != _displayedUser.Id)
            {
                FeedbackService.Warning("Вы можете изменить аватар только своего профиля.");
                return;
            }

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Изображения|*.jpg;*.png;*.jpeg;*.bmp",
                Title = "Выберите изображение для аватара"
            };
            if (dialog.ShowDialog() == true)
            {
                string fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(dialog.FileName);
                string destFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Avatars");
                if (!System.IO.Directory.Exists(destFolder))
                    System.IO.Directory.CreateDirectory(destFolder);
                string destPath = System.IO.Path.Combine(destFolder, fileName);
                try
                {
                    System.IO.File.Copy(dialog.FileName, destPath, true);
                    _displayedUser.AvatarKey = fileName;
                    App.context.SaveChanges();
                    // Обновить отображение
                    AvatarImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(destPath, UriKind.Absolute));
                    FeedbackService.Information("Аватар успешно обновлён.");
                }
                catch (Exception ex)
                {
                    FeedbackService.Error($"Ошибка при сохранении аватара: {ex.Message}");
                }
            }
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            // Разрешено если свой профиль или админ
            if (App.currentUser.Id != _displayedUser.Id && App.currentUser.Role != (int)Role.Admin)
            {
                FeedbackService.Warning("Вы можете сменить пароль только своего профиля.");
                return;
            }

            var window = new ChangePasswordWindow(_displayedUser.Id);
            window.ShowDialog();
        }
    }

    // Модели для отображения
    public class OrderHistoryItem
    {
        public ObservableCollection<OrderItem> Items { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalOrderPrice { get; set; }
        public ICommand ViewDetailsCommand { get; set; }
    }

    public class OrderItem
    {
        public string ImageSource { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal PricePerUnit { get; set; }
        public decimal TotalPrice => PricePerUnit * Quantity;
    }

    public class TaskItem : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime Deadline { get; set; }
        private bool _isCompleted;
        public bool IsCompleted
        {
            get => _isCompleted;
            set { _isCompleted = value; OnPropertyChanged(); }
        }
        public string AuthorName { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class ScheduleItem
    {
        public int Id { get; set; }
        public DateTime DateStart { get; set; }
        public int Hours { get; set; }
        public string DisplayText { get; set; }
    }
}