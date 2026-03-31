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
        public MainWindow()
        {
            InitializeComponent();
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

        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SuppliesBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ClientsBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StatsBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ProductsBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ProfileBtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProfilePage());
        }
    }
}
