using MedicalShopDiaShop.AppData;
using MedicalShopDiaShop.Database;
using MedicalShopDiaShop.MainView.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MedicalShopDiaShop.MainView
{
    public partial class MainWindow : Window
    {
        private Dictionary<string, (Button Maximized, Button Minimized, Button Visual)> _buttonPairs;
        private string _currentActiveKey;
        private int _currentNotificationPage = 1;
        private const int NotificationPageSize = 5;
        private int _totalNotificationsCount = 0;
        private ObservableCollection<NotificationItem> _notifications;
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _buttonPairs = new Dictionary<string, (Button, Button, Button)>
            {
                ["Profile"] = (MaxProfileBtn, ProfileBtn, ProfileVisualBtn),
                ["Products"] = (MaxProductsBtn, ProductsBtn, ProductsVisualBtn),
                ["Stats"] = (MaxStatsBtn, StatsBtn, StatsVisualBtn),
                ["Clients"] = (MaxClientsBtn, ClientsBtn, ClientsVisualBtn),
                ["Supplies"] = (MaxSuppliesBtn, SuppliesBtn, SuppliesVisualBtn),
                ["Employees"] = (MaxEmployeesBtn, EmployeesBtn, EmployeesVisualBtn),
                ["Orders"] = (MaxOrdersBtn, OrdersBtn, OrdersVisualBtn),
                ["Exit"] = (MaxExitBtn, ExitBtn, ExitVisualBtn),
                //["Stock"] = (MaxStockBtn, StockBtn, StockVisualBtn),
            };

            foreach (var pair in _buttonPairs.Values)
            {
                if (pair.Visual != null)
                    pair.Visual.Visibility = Visibility.Hidden;
            }

            MainFrame.Navigate(new ProfilePage());
            SetActiveButton("Profile");
            LoadNotifications();
            LoadUserInfo();
            //StockHelper.CheckExpiringProducts();
            UpdateNotificationBadge();
        }

        private void SetActiveButton(string key)
        {
            if (!_buttonPairs.ContainsKey(key))
                return;

            var currentPair = _buttonPairs[key];

            if (_currentActiveKey == key)
                return;

            if (!string.IsNullOrEmpty(_currentActiveKey) && _buttonPairs.ContainsKey(_currentActiveKey))
            {
                var prevPair = _buttonPairs[_currentActiveKey];
                if (prevPair.Maximized != null)
                    prevPair.Maximized.Background = Brushes.White;
                if (prevPair.Minimized != null)
                    prevPair.Minimized.Background = Brushes.White;
                if (prevPair.Visual != null)
                    prevPair.Visual.Visibility = Visibility.Hidden;
            }

            if (currentPair.Maximized != null)
                AnimateButtonActivation(currentPair.Maximized);
            if (currentPair.Minimized != null)
                AnimateButtonActivation(currentPair.Minimized);
            if (currentPair.Visual != null)
                currentPair.Visual.Visibility = Visibility.Visible;

            _currentActiveKey = key;
        }

        private void AnimateButtonActivation(Button btn)
        {
            if (btn == null) return;

            var colorAnim = new ColorAnimation
            {
                From = Colors.White,
                To = Colors.White,
                Duration = TimeSpan.FromSeconds(0.15)
            };
            var brush = new SolidColorBrush(Colors.White);
            btn.Background = brush;
            brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);

            if (btn.RenderTransform == null || !(btn.RenderTransform is ScaleTransform))
            {
                btn.RenderTransform = new ScaleTransform(1, 1);
                btn.RenderTransformOrigin = new Point(0.5, 0.5);
            }
            var scaleTransform = (ScaleTransform)btn.RenderTransform;
            var scaleUpAnim = new DoubleAnimation(1.05, TimeSpan.FromSeconds(0.1))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
                AutoReverse = true
            };
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleUpAnim);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleUpAnim);
        }

        private void AnimateMenu(bool expand)
        {
            if (expand)
            {
                MaximizedButtons.Visibility = Visibility.Visible;
                MaximizedButtons.Opacity = 0;
                MaximizedButtons.MaxWidth = 60;
                var widthAnim = new DoubleAnimation(250, TimeSpan.FromSeconds(0.2));
                widthAnim.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };
                var opacityAnim = new DoubleAnimation(1, TimeSpan.FromSeconds(0.15));

                widthAnim.Completed += (s, _) =>
                {
                    MaximizedButtons.MaxWidth = double.PositiveInfinity;
                    ArrowIcon.RenderTransform = new RotateTransform(0);
                };
                MaximizedButtons.BeginAnimation(Grid.MaxWidthProperty, widthAnim);
                MaximizedButtons.BeginAnimation(Grid.OpacityProperty, opacityAnim);

                MinimizedButtons.Visibility = Visibility.Collapsed;
            }
            else
            {
                MaximizedButtons.MaxWidth = 250;
                var widthAnim = new DoubleAnimation(60, TimeSpan.FromSeconds(0.2));
                widthAnim.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };
                var opacityAnim = new DoubleAnimation(0, TimeSpan.FromSeconds(0.15));

                widthAnim.Completed += (s, _) =>
                {
                    MaximizedButtons.Visibility = Visibility.Collapsed;
                    MinimizedButtons.Visibility = Visibility.Visible;
                    MaximizedButtons.MaxWidth = 300;
                    ArrowIcon.RenderTransform = new RotateTransform(180);
                };
                MaximizedButtons.BeginAnimation(Grid.MaxWidthProperty, widthAnim);
                MaximizedButtons.BeginAnimation(Grid.OpacityProperty, opacityAnim);
            }
        }

        private void MinimizeOrMaximizeBtn_Click(object sender, RoutedEventArgs e)
        {
            bool isExpanded = MaximizedButtons.Visibility == Visibility.Visible;
            if (isExpanded)
            {
                ArrowIcon.RenderTransform = new RotateTransform(180);
                ArrowIcon.RenderTransformOrigin = new Point(0.5, 0.5);
                AnimateMenu(false);
            }
            else
            {
                ArrowIcon.RenderTransform = new RotateTransform(0);
                ArrowIcon.RenderTransformOrigin = new Point(0.5, 0.5);
                AnimateMenu(true);
            }
        }

        private void EmployeesBtn_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton("Employees");
            MainFrame.Navigate(new EmployeesPage());
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton("Exit");
            var result = FeedbackService.Question("Вы уверены, что хотите выйти?");
            if (result == MessageBoxResult.Yes)
            {
                AuthorizationWindow authorizationWindow = new AuthorizationWindow();
                authorizationWindow.Show();
                Close();
            }
        }

        private void SuppliesBtn_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton("Supplies");
            MainFrame.Navigate(new SuppliersPage());
        }

        private void ClientsBtn_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton("Clients");
            MainFrame.Navigate(new ClientsPage());
        }

        private void StatsBtn_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton("Stats");
            MainFrame.Navigate(new StatisticsPage());
        }

        private void ProductsBtn_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton("Products");
            MainFrame.Navigate(new ProductsPage());
        }

        private void ProfileBtn_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton("Profile");
            MainFrame.Navigate(new ProfilePage());
        }

        private void OrdersBtn_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton("Orders");
            MainFrame.Navigate(new OrdersPage());
        }

        private void StockBtn_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton("Stock");
            MainFrame.Navigate(new StockPage());
        }

        private void NotificationsBtn_Click(object sender, RoutedEventArgs e)
        {
            BlurGrid.Visibility = Visibility.Visible;
            NotificationGrid.Visibility = Visibility.Visible;
            var story = (Storyboard)FindResource("ShowNotificationAnimation");
            story.Begin(NotificationGrid);
        }

        private void BlurGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var story = (Storyboard)FindResource("HideNotificationAnimation");
            story.Completed += (s, _) =>
            {
                BlurGrid.Visibility = Visibility.Collapsed;
                NotificationGrid.Visibility = Visibility.Collapsed;
            };
            story.Begin(NotificationGrid);
        }

        private void NextNotificationPageBtn_Click(object sender, RoutedEventArgs e)
        {
            int maxPage = (int)Math.Ceiling((double)_totalNotificationsCount / NotificationPageSize);
            if (_currentNotificationPage < maxPage)
            {
                _currentNotificationPage++;
                LoadNotifications(_currentNotificationPage);
            }
        }

        private void PreviousNotificationPageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentNotificationPage > 1)
            {
                _currentNotificationPage--;
                LoadNotifications(_currentNotificationPage);
            }
        }

        private void LoadNotifications(int page = 1)
        {
            using (var context = new DiaShopEntities())
            {
                var query = context.Notification
                    .Where(n => n.UserId == App.currentUser.Id)
                    .OrderByDescending(n => n.CreationDate); // порядок по убыванию Id (новые сверху)

                _totalNotificationsCount = query.Count();
                var items = query
                    .Skip((page - 1) * NotificationPageSize)
                    .Take(NotificationPageSize)
                    .Select(n => new NotificationItem
                    {
                        Id = n.Id,
                        Text = n.Text,
                        IsRead = n.IsRead,
                        CreatedAt = (DateTime)n.CreationDate
                    })
                    .ToList();

                _notifications = new ObservableCollection<NotificationItem>(items);
                NotificationListBox.ItemsSource = _notifications;
                // обновить номер страницы в TextBox
                PageNumberTextBox.Text = page.ToString();
            }
        }

        public void UpdateNotificationBadge()
        {
            using (var context = new DiaShopEntities())
            {
                int unreadCount = context.Notification.Count(n => n.UserId == App.currentUser.Id && !n.IsRead);
                NotificationCheck.Visibility = unreadCount > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void NotificationListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NotificationListBox.SelectedItem is NotificationItem selected)
            {
                // Помечаем как прочитанное
                using (var context = new DiaShopEntities())
                {
                    var dbNotification = context.Notification.Find(selected.Id);
                    if (dbNotification != null && !dbNotification.IsRead)
                    {
                        dbNotification.IsRead = true;
                        context.SaveChanges();
                    }
                }
                selected.IsRead = true;
                // Обновляем отображение (можно перезагрузить страницу или обновить элемент)
                var listBox = sender as ListBox;
                listBox.Items.Refresh();
                UpdateNotificationBadge();
            }
        }

        private void Notification_MouseEnter(object sender, MouseEventArgs e)
        {
            var grid = sender as Grid;
            var notification = grid?.DataContext as NotificationItem;
            if (notification != null && !notification.IsRead)
            {
                using (var context = new DiaShopEntities())
                {
                    var dbNotif = context.Notification.Find(notification.Id);
                    if (dbNotif != null) dbNotif.IsRead = true;
                    context.SaveChanges();
                }
                notification.IsRead = true;
                var listBoxItem = FindVisualParent<ListBoxItem>(grid);
                if (listBoxItem != null)
                {
                    var listBox = ItemsControl.ItemsControlFromItemContainer(listBoxItem) as ListBox;
                    listBox?.Items.Refresh();
                }
                UpdateNotificationBadge();
            }
        }

        private T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null && !(child is T))
                child = VisualTreeHelper.GetParent(child);
            return child as T;
        }

        private void LoadUserInfo()
        {
            var user = App.currentUser;
            if (user != null)
            {
                string fullName = $"{user.LastName} {user.FirstName}";
                if (!string.IsNullOrEmpty(user.MiddleName))
                    fullName += $" {user.MiddleName}";
                FullNameTbl.Text = fullName;

                // Определяем путь к аватару
                string relativePath;
                if (string.IsNullOrEmpty(user.AvatarKey))
                    relativePath = "/Resources/avatarPlaceHolder.png";
                else if (user.AvatarKey.StartsWith("/Resources/"))
                    relativePath = user.AvatarKey;
                else
                    relativePath = $"/Resources/Avatars/{user.AvatarKey}";

                // Преобразуем в абсолютный путь
                string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath.TrimStart('/'));

                try
                {
                    if (System.IO.File.Exists(fullPath))
                        UserAvatarImg.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(fullPath));
                    else
                        UserAvatarImg.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/Resources/avatarPlaceHolder.png", UriKind.Relative));
                }
                catch
                {
                    UserAvatarImg.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/Resources/avatarPlaceHolder.png", UriKind.Relative));
                }
            }
        }

        public void RefreshUserInfo()
        {
            LoadUserInfo();
        }
    }

    public class NotificationItem
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; } // если нет в БД, можно добавить поле CreatedAt, иначе использовать Id как порядок
        public bool IsRead { get; set; }
        public Visibility UnreadVisibility => IsRead ? Visibility.Collapsed : Visibility.Visible;
    }
}