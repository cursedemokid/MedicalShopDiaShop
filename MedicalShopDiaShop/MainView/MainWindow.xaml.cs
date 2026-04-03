using MedicalShopDiaShop.AppData;
using MedicalShopDiaShop.MainView.Pages;
using System;
using System.Collections.Generic;
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
                ["Exit"] = (MaxExitBtn, ExitBtn, ExitVisualBtn)
            };

            foreach (var pair in _buttonPairs.Values)
            {
                if (pair.Visual != null)
                    pair.Visual.Visibility = Visibility.Hidden;
            }

            SetActiveButton("Profile");
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
        }

        private void ClientsBtn_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton("Clients");
        }

        private void StatsBtn_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton("Stats");
        }

        private void ProductsBtn_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton("Products");
        }

        private void ProfileBtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProfilePage());
            SetActiveButton("Profile");
        }

        private void OrdersBtn_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton("Orders");
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
            // TODO: реализовать пагинацию уведомлений
        }

        private void PreviousNotificationPageBtn_Click(object sender, RoutedEventArgs e)
        {
            // TODO: реализовать пагинацию уведомлений
        }
    }
}