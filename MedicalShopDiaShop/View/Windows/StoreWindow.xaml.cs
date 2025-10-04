using MedicalShopDiaShop.AppData;
using MedicalShopDiaShop.Model;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace MedicalShopDiaShop.View.Windows
{
    /// <summary>
    /// Логика взаимодействия для StoreWindow.xaml
    /// </summary>
    public partial class StoreWindow : Window
    {
        List<Product> _product = App.context.Product.ToList();
        public StoreWindow()
        {
            InitializeComponent();

            SearchTbx.Text = "Поиск";
            
        }

        private void SearchTbx_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SearchTbx.Text = "";

        }

        private void ExitMI_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = FeedbackService.Question("Вы уверены, что хотите выйти?");
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                App.currentUser = null;
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                Close();
            }
        }

        private void PersonalDataMI_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CurrentOrdersMI_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OrderHistoryMI_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FavoritePageBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StoreBasketBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
