using MedicalShopDiaShop.AppData;
using MedicalShopDiaShop.Model;
using MedicalShopDiaShop.View.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MedicalShopDiaShop.View.Windows
{
    /// <summary>
    /// Логика взаимодействия для StoreWindow.xaml
    /// </summary>
    public partial class StoreWindow : Window
    {
        //List<Product> _product = App.context.Product.ToList();
        public StoreWindow()
        {
            InitializeComponent();

            //App.currentButton = CatalogBtn;
            //SearchByProductNameCmb.ItemsSource = App.context.Product.ToList();
            //MainFrame.Navigate(new PopularProductsPage());

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
            MainFrame.Navigate(new OrderHistoryPage());
            ChangeButton(StoreBasketBtn);
        }

        private void FavoritePageBtn_Click(object sender, RoutedEventArgs e)
        {
            //MainFrame.Navigate(new PopularProductsPage(PageType.Favorite));
            //ChangeButton(FavoritePageBtn);
        }

        private void StoreBasketBtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new OrderBasketPage());
            ChangeButton(StoreBasketBtn);
        }

        private void PopularProductsBtn_Click(object sender, RoutedEventArgs e)
        {
            //MainFrame.Navigate(new PopularProductsPage(PageType.Popular));
            //ChangePage(PopularProductsBtn);
        }

        private void SearchByProductNameCmb_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //if (SearchByProductNameCmb.SelectedItem != null)
            //{
            //    int id = Convert.ToInt32(SearchByProductNameCmb.SelectedValue);
            //    MainFrame.Navigate(new PopularProductsPage(id));
            //}
            //else
            //{
            //    MainFrame.Navigate(new PopularProductsPage());
            //}
        }

        private void CatalogBtn_Click(object sender, RoutedEventArgs e)
        {
            //MainFrame.Navigate(new PopularProductsPage());
            //ChangePage(CatalogBtn);
        }

        private void ChangePage(Button button)
        {
            //if (App.currentButton != null)
            //{
            //    App.currentButton.Background = new SolidColorBrush(Colors.White);
            //    App.currentButton.Foreground = new SolidColorBrush(Colors.DarkGreen);
            //}

            //App.currentButton = button;
            //App.currentButton.Background = new SolidColorBrush(Colors.Green);
            //App.currentButton.Foreground = new SolidColorBrush(Colors.White);
        }
        private void ChangeButton(Button button)
        {
            //App.currentButton.Background = new SolidColorBrush(Colors.White);
            //App.currentButton.Foreground = new SolidColorBrush(Colors.DarkGreen);

            //App.currentButton = new Button();
        }

        private void LowCarbohydratesBtn_Click(object sender, RoutedEventArgs e)
        {
            //MainFrame.Navigate(new PopularProductsPage(PageType.LowCarbohydrates));
            //ChangePage(LowCarbohydratesBtn);
        }

        private void WithoutGlutenBtn_Click(object sender, RoutedEventArgs e)
        {
            //MainFrame.Navigate(new PopularProductsPage(PageType.WithoutGluten));
            //ChangePage(WithoutGlutenBtn);
        }

        private void NewProductsBtn_Click(object sender, RoutedEventArgs e)
        {
            //MainFrame.Navigate(new PopularProductsPage(PageType.New));
            //ChangePage(NewProductsBtn);
        }
    }
}
