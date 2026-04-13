using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MedicalShopDiaShop.Database;
using static MedicalShopDiaShop.AppData.Status;

namespace MedicalShopDiaShop.MainView.Pages
{
    public partial class ProfilePage : Page
    {
        private Database.User _displayedUser;

        // Свойства для привязки расписания
        public IEnumerable<DateTime> ScheduledDates { get; set; }
        public Func<DateTime, object> ScheduleToolTipSelector { get; set; }

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
                ClientTextBlock.Visibility = Visibility.Visible;
                AddressTextBlock.Visibility = Visibility.Visible;
                LoadClientHistory();
            }
            else
            {
                ClientProfileGrid.Visibility = Visibility.Collapsed;
                EmployeeProfileGrid.Visibility = Visibility.Visible;
                ClientTextBlock.Visibility = Visibility.Collapsed;
                AddressTextBlock.Visibility = Visibility.Collapsed;
                LoadEmployeeScheduleAndTasks();
            }

            ChangePasswordButton.Visibility = (App.currentUser.Id == userId || App.currentUser.Role == (int)Role.Admin)
                ? Visibility.Visible : Visibility.Collapsed;
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

        private void LoadClientHistory()
        {
            var orders = App.context.Order
                .Where(o => o.ClientId == _displayedUser.Id)
                .OrderByDescending(o => o.DateTime)
                .ToList();

            var history = new ObservableCollection<OrderHistoryItem>();

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
                    })
                    .ToList();

                var total = orderItems.Sum(i => i.TotalPrice);

                history.Add(new OrderHistoryItem
                {
                    OrderDate = order.DateTime,
                    Items = new ObservableCollection<OrderItem>(orderItems),
                    TotalOrderPrice = total,
                    ViewDetailsCommand = new RelayCommand(() => ShowDetails($"Заказ №{order.Id}"))
                });
            }

            HistoryListBox.ItemsSource = history;
        }

        private void LoadEmployeeScheduleAndTasks()
        {
            // --- Задачи (без изменений) ---
            var tasks = App.context.Task
                .Where(t => t.UserId == _displayedUser.Id)
                .OrderBy(t => t.StartAt)
                .ToList();

            var taskItems = new ObservableCollection<TaskItem>();
            foreach (var task in tasks)
            {
                var author = App.context.User.FirstOrDefault(u => u.Id == task.AuthorId);
                string authorName = author != null ? author.UserName : "Неизвестно";

                taskItems.Add(new TaskItem
                {
                    Title = task.Description,
                    StartDate = task.StartAt,
                    Deadline = task.Deadline ?? task.EndAt,
                    IsCompleted = task.IsCompleted,
                    AuthorName = authorName
                });
            }
            TasksListBox.ItemsSource = taskItems;

            // --- Загрузка расписания ---
            var schedules = App.context.Schedule
                .Where(s => s.UserId == _displayedUser.Id)
                .ToList();

            // Уникальные даты
            ScheduledDates = schedules.Select(s => s.DateStart.Date).Distinct().ToList();

            // Функция создания ToolTip для конкретной даты
            ScheduleToolTipSelector = date =>
            {
                var daySchedules = schedules
                    .Where(s => s.DateStart.Date == date)
                    .OrderBy(s => s.DateStart)
                    .ToList();

                if (!daySchedules.Any())
                    return null;

                var panel = new StackPanel();
                panel.Children.Add(new TextBlock
                {
                    Text = date.ToString("dd MMMM yyyy"),
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 5)
                });

                foreach (var s in daySchedules)
                {
                    string startTime = s.DateStart.ToString("HH:mm");
                    string endTime = s.DateStart.AddHours(s.Hours).ToString("HH:mm");
                    panel.Children.Add(new TextBlock
                    {
                        Text = $"• {startTime} – {endTime}",
                        Margin = new Thickness(0, 2, 0, 0)
                    });
                }

                return panel;
            };

            // Устанавливаем DataContext для привязок (сама страница)
            DataContext = this;
        }

        private string GetImagePath(string imageName)
        {
            if (string.IsNullOrEmpty(imageName))
                return "/Resources/placeholder.png";
            return imageName;
        }

        private void ShowDetails(string orderId)
        {
            MessageBox.Show($"Открыть подробности: {orderId}");
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция смены пароля будет здесь");
        }

        public class RelayCommand : ICommand
        {
            private readonly Action _execute;
            public RelayCommand(Action execute) => _execute = execute;
            public event EventHandler CanExecuteChanged;
            public bool CanExecute(object parameter) => true;
            public void Execute(object parameter) => _execute();
        }
    }

    // Классы моделей (оставьте как есть)
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

    public class TaskItem
    {
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime Deadline { get; set; }
        public bool IsCompleted { get; set; }
        public string AuthorName { get; set; }
    }
}