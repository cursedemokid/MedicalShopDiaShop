using MedicalShopDiaShop.AppData;
using MedicalShopDiaShop.MainView.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MedicalShopDiaShop.MainView
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
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
                    pair.Visual.Visibility = Visibility.Collapsed;
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
                //currentPair.Maximized.Background = Brushes.Green;
            if (currentPair.Minimized != null)
                //currentPair.Minimized.Background = Brushes.Green;
            if (currentPair.Visual != null)
                currentPair.Visual.Visibility = Visibility.Visible;

            _currentActiveKey = key;
        }

        private void MinimizeOrMaximizeBtn_Click(object sender, RoutedEventArgs e)
        {
            bool isExpanded = MaximizedButtons.Visibility == Visibility.Visible;
            MaximizedButtons.Visibility = isExpanded ? Visibility.Collapsed : Visibility.Visible;
            MinimizedButtons.Visibility = isExpanded ? Visibility.Visible : Visibility.Collapsed;

            var transform = ArrowIcon.RenderTransform as RotateTransform;
            if (transform == null)
            {
                ArrowIcon.RenderTransform = new RotateTransform(isExpanded ? 180 : 0);
                ArrowIcon.RenderTransformOrigin = new Point(0.5, 0.5);
            }
            else
            {
                transform.Angle = isExpanded ? 180 : 0;
            }
        }

        private void EmployeesBtn_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton("Employees");
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton("Exit");
            FeedbackService.Question("Вы уверены, что хотите выйти?");
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
        }

        private void BlurGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BlurGrid.Visibility = Visibility.Collapsed;
            NotificationGrid.Visibility = Visibility.Collapsed;
        }

        private void NextNotificationPageBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PreviousNotificationPageBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
