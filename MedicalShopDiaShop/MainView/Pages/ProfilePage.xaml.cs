using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using MedicalShopDiaShop.AppData;
using MedicalShopDiaShop.Database;
using static MedicalShopDiaShop.AppData.Status;

namespace MedicalShopDiaShop.MainView.Pages
{
    public partial class ProfilePage : Page, INotifyPropertyChanged
    {
        private Database.User _displayedUser;
        private bool _isAdmin => App.currentUser.Role == (int)Role.Admin;

        // Для расписания
        private IEnumerable<DateTime> _scheduledDates;
        public IEnumerable<DateTime> ScheduledDates
        {
            get => _scheduledDates;
            set
            {
                _scheduledDates = value;
                OnPropertyChanged();
                // Принудительно обновляем календарь (через поведение)
                ScheduleCalendar?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    var temp = ScheduleCalendar.DisplayDate;
                    ScheduleCalendar.DisplayDate = temp.AddDays(1);
                    ScheduleCalendar.DisplayDate = temp;
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        public Func<DateTime, object> ScheduleToolTipSelector { get; set; }
        private ObservableCollection<ScheduleItem> _schedulesForSelectedDate;

        // Для задач
        private int _tasksCurrentPage = 1;
        private int _tasksTotalPages = 1;
        private const int TasksPageSize = 4;
        private ObservableCollection<TaskItem> _taskItems;
        private bool _updatingTaskSelection;

        // Для истории покупок
        private List<OrderHistoryItem> _allHistory;
        private ObservableCollection<OrderHistoryItem> _filteredHistory;

        public ProfilePage()
        {
            InitializeComponent();
            DataContext = this;
            LoadUserData(App.currentUser.Id);
        }

        public ProfilePage(int userId)
        {
            InitializeComponent();
            DataContext = this;
            LoadUserData(userId);
        }

        private void LoadUserData(int userId)
        {
            _displayedUser = App.context.User.FirstOrDefault(u => u.Id == userId);
            if (_displayedUser == null)
            {
                FeedbackService.Error("Пользователь не найден");
                return;
            }

            LoadPersonalInfo();

            if (_displayedUser.Role == (int)Role.Client)
            {
                ClientProfileGrid.Visibility = Visibility.Visible;
                EmployeeProfileGrid.Visibility = Visibility.Collapsed;
                EmployeeTextBlock.Visibility = Visibility.Collapsed;
                StoreTextBlock.Visibility = Visibility.Collapsed;
                ClientTextBlock.Visibility = Visibility.Visible;
                AddressTextBlock.Visibility = Visibility.Visible;
                LoadClientHistory();
            }
            else
            {
                ClientProfileGrid.Visibility = Visibility.Collapsed;
                EmployeeProfileGrid.Visibility = Visibility.Visible;
                EmployeeTextBlock.Visibility = Visibility.Visible;
                StoreTextBlock.Visibility = Visibility.Visible;
                ClientTextBlock.Visibility = Visibility.Collapsed;
                AddressTextBlock.Visibility = Visibility.Collapsed;
                LoadEmployeeScheduleAndTasks();
            }

            ChangePasswordButton.Visibility = (App.currentUser.Id == userId || _isAdmin) ? Visibility.Visible : Visibility.Collapsed;
            AddScheduleBtn.Visibility = _isAdmin ? Visibility.Visible : Visibility.Collapsed;
            EditScheduleBtn.Visibility = _isAdmin ? Visibility.Visible : Visibility.Collapsed;
            DeleteScheduleBtn.Visibility = _isAdmin ? Visibility.Visible : Visibility.Collapsed;
            AddTaskBtn.Visibility = _isAdmin ? Visibility.Visible : Visibility.Collapsed;
            EditTaskBtn.Visibility = _isAdmin ? Visibility.Visible : Visibility.Collapsed;
            DeleteTaskBtn.Visibility = _isAdmin ? Visibility.Visible : Visibility.Collapsed;
            UpdateTaskButtonsState();
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

            // Формирование пути к аватару
            string avatarPath;
            if (string.IsNullOrEmpty(_displayedUser.AvatarKey))
                avatarPath = "/Resources/ProfileIcon.png";
            else if (_displayedUser.AvatarKey.StartsWith("/Resources/"))
                avatarPath = _displayedUser.AvatarKey;
            else
                avatarPath = $"/Resources/Avatars/{_displayedUser.AvatarKey}";

            // Используем абсолютный путь для надёжности
            string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, avatarPath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
                AvatarImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(fullPath));
            else
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
                // Сначала загружаем данные из БД без вызова GetImagePath
                var productData = App.context.ProductOrder
                    .Where(po => po.OrderId == order.Id)
                    .Select(po => new
                    {
                        Image = po.Product.Image,
                        po.Product.Name,
                        po.Quantity,
                        po.Price
                    })
                    .ToList(); // Выполняем запрос к БД

                // Затем в памяти применяем GetImagePath
                var orderItems = productData.Select(pd => new OrderItem
                {
                    ImageSource = GetImagePath(pd.Image),
                    Name = pd.Name,
                    Quantity = pd.Quantity,
                    PricePerUnit = pd.Price
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

            if (HistoryDateFilter.SelectedDate.HasValue)
                query = query.Where(h => h.OrderDate.Date == HistoryDateFilter.SelectedDate.Value.Date);

            if (!string.IsNullOrWhiteSpace(HistorySearchBar.Text))
            {
                string search = HistorySearchBar.Text.ToLower();
                query = query.Where(h => h.Items.Any(i => i.Name.ToLower().Contains(search)));
            }

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

        private void HistorySearchBar_FilterTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) =>
            ApplyHistoryFilterAndSort();

        private void HistorySearch_Click(object sender, RoutedEventArgs e) => ApplyHistoryFilterAndSort();
        private void OrderByCmb_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyHistoryFilterAndSort();
        private void HistoryDateFilter_SelectedDateChanged(object sender, SelectionChangedEventArgs e) => ApplyHistoryFilterAndSort();

        #endregion

        #region Сотрудник: расписание

        private void LoadEmployeeScheduleAndTasks()
        {
            ShowScheduleTab();
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
                // Применяем шрифт проекта
                var fontFamily = (FontFamily)FindResource("GothamProFamily");
                var lightFontFamily = (FontFamily)FindResource("GothamProLight");
                panel.Children.Add(new TextBlock
                {
                    Text = date.ToString("dd MMMM yyyy"),
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 5),
                    FontFamily = fontFamily
                });
                foreach (var s in daySchedules)
                {
                    panel.Children.Add(new TextBlock
                    {
                        Text = $"• {s.DateStart:HH:mm} – {s.DateStart.AddHours(s.Hours):HH:mm}",
                        Margin = new Thickness(0, 2, 0, 0),
                        FontFamily = lightFontFamily,
                        FontSize = 12
                    });
                }
                return panel;
            };

            ScheduleCalendar.SelectedDate = DateTime.Today;
            UpdateSchedulesList(DateTime.Today);
        }

        private static Brush GetAppPrimaryBrush() =>
            Application.Current?.TryFindResource("AppPrimaryBrush") as Brush
            ?? new SolidColorBrush(Color.FromRgb(103, 58, 183));

        private void ShowScheduleTab()
        {
            ScheduleTabPanel.Visibility = Visibility.Visible;
            TasksTabPanel.Visibility = Visibility.Collapsed;

            var primaryBrush = GetAppPrimaryBrush();
            ScheduleTabHeader.Background = Brushes.White;
            ScheduleTabHeader.BorderBrush = primaryBrush;
            ScheduleTabHeaderText.Foreground = primaryBrush;

            TasksTabHeader.Background = Brushes.Transparent;
            TasksTabHeader.BorderBrush = Brushes.Transparent;
            TasksTabHeaderText.Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102));
        }

        private void ShowTasksTab()
        {
            ScheduleTabPanel.Visibility = Visibility.Collapsed;
            TasksTabPanel.Visibility = Visibility.Visible;

            var primaryBrush = GetAppPrimaryBrush();
            TasksTabHeader.Background = Brushes.White;
            TasksTabHeader.BorderBrush = primaryBrush;
            TasksTabHeaderText.Foreground = primaryBrush;

            ScheduleTabHeader.Background = Brushes.Transparent;
            ScheduleTabHeader.BorderBrush = Brushes.Transparent;
            ScheduleTabHeaderText.Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102));
        }

        private void ScheduleTabHeader_Click(object sender, MouseButtonEventArgs e) => ShowScheduleTab();

        private void TasksTabHeader_Click(object sender, MouseButtonEventArgs e) => ShowTasksTab();

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
                    StartDate = (DateTime)task.StartAt,
                    Deadline = (DateTime)(task.Deadline ?? task.EndAt),
                    IsCompleted = task.IsCompleted,
                    AuthorName = author?.UserName ?? "Неизвестно"
                });
            }
            TasksListBox.ItemsSource = _taskItems;
            UpdateTaskButtonsState();

            TasksPageText.Text = $"{_tasksCurrentPage} / {(_tasksTotalPages == 0 ? 1 : _tasksTotalPages)}";
            PrevTasksBtn.IsEnabled = _tasksCurrentPage > 1;
            NextTasksBtn.IsEnabled = _tasksCurrentPage < _tasksTotalPages;
        }

        private void PrevTasksBtn_Click(object sender, RoutedEventArgs e) => LoadTasks(_tasksCurrentPage - 1);
        private void NextTasksBtn_Click(object sender, RoutedEventArgs e) => LoadTasks(_tasksCurrentPage + 1);

        private TaskItem GetSelectedTask() =>
            _taskItems?.FirstOrDefault(t => t.IsChosen);

        private void UpdateTaskButtonsState()
        {
            bool hasSelection = GetSelectedTask() != null;
            if (EditTaskBtn.Visibility == Visibility.Visible)
                EditTaskBtn.IsEnabled = hasSelection;
            if (DeleteTaskBtn.Visibility == Visibility.Visible)
                DeleteTaskBtn.IsEnabled = hasSelection;
        }

        private void TaskSelectCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_updatingTaskSelection) return;
            var task = (sender as CheckBox)?.DataContext as TaskItem;
            if (task == null || _taskItems == null) return;

            _updatingTaskSelection = true;
            foreach (var item in _taskItems)
                item.IsChosen = item.Id == task.Id;
            _updatingTaskSelection = false;
            UpdateTaskButtonsState();
        }

        private void TaskSelectCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_updatingTaskSelection) return;
            var task = (sender as CheckBox)?.DataContext as TaskItem;
            if (task == null) return;

            task.IsChosen = false;
            UpdateTaskButtonsState();
        }

        private void AddTaskBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditTaskWindow(_displayedUser.Id);
            if (window.ShowDialog() == true)
                LoadTasks(_tasksCurrentPage);
        }

        private void EditTaskBtn_Click(object sender, RoutedEventArgs e)
        {
            if (GetSelectedTask() is TaskItem selected)
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
            if (GetSelectedTask() is TaskItem selected)
            {
                if (FeedbackService.Question($"Удалить задачу \"{selected.Title}\"?") == MessageBoxResult.Yes)
                {
                    var dbTask = App.context.Task.Find(selected.Id);
                    if (dbTask != null)
                    {
                        // У задачи могут быть связанные уведомления (FK_Notification_Task с ограничением DELETE),
                        // поэтому перед удалением задачи удаляем зависимые Notification записи.
                        var linkedNotifications = App.context.Notification
                            .Where(n => n.TaskId == dbTask.Id)
                            .ToList();

                        if (linkedNotifications.Count > 0)
                            App.context.Notification.RemoveRange(linkedNotifications);

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
                var item = _taskItems.FirstOrDefault(t => t.Id == taskId);
                if (item != null) item.IsCompleted = isCompleted;
            }
        }

        #endregion

        private string GetImagePath(string imageName) => string.IsNullOrEmpty(imageName) ? "/Resources/placeholder.png" : imageName;

        private void ShowDetails(string orderId)
        {
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
                    // Сохраняем полный относительный путь, начинающийся с /Resources/Avatars/
                    string relativePath = $"/Resources/Avatars/{fileName}";
                    _displayedUser.AvatarKey = relativePath;
                    App.context.SaveChanges();

                    // Обновляем отображение на странице профиля
                    AvatarImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(destPath, UriKind.Absolute));

                    // Обновляем аватар в главном окне
                    if (Application.Current.MainWindow is MainWindow mainWindow)
                        mainWindow.RefreshUserInfo();

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
            if (App.currentUser.Id != _displayedUser.Id && App.currentUser.Role != (int)Role.Admin)
            {
                FeedbackService.Warning("Вы можете сменить пароль только своего профиля.");
                return;
            }

            var window = new ChangePasswordWindow(_displayedUser.Id);
            window.ShowDialog();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private void SupplierProductSearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            // Оставляем пустым, чтобы не падать при вводе в поле поиска на вкладке поставщика.
            // Фильтрация реализована в отдельном окне SupplierProfileWindow.
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
        private bool _isChosen;
        public bool IsChosen
        {
            get => _isChosen;
            set { _isChosen = value; OnPropertyChanged(); }
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