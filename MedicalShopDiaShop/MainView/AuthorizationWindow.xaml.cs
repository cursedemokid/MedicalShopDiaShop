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
    /// Логика взаимодействия для AuthorizationWindow.xaml
    /// </summary>
    public partial class AuthorizationWindow : Window
    {
        public AuthorizationWindow()
        {
            InitializeComponent();
        }

        private void RegistrationBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void EnterBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateFields())
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                Close();
            }
        }

        private void LoginTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(LoginTextBox.Text))
                ClearLoginError();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(PasswordBox.Password))
                ClearPasswordError();
        }

        private void ClearPasswordError()
        {
            PasswordGroupBox.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF673AB7"));
            PasswordHeader.Foreground = Brushes.Black;
            PasswordErrorText.Visibility = Visibility.Collapsed;
        }

        private bool ValidateFields()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(LoginTextBox.Text))
            {
                LoginGroupBox.BorderBrush = Brushes.Red;
                LoginHeader.Foreground = Brushes.Red;
                LoginErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                ClearLoginError();
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                PasswordGroupBox.BorderBrush = Brushes.Red;
                PasswordHeader.Foreground = Brushes.Red;
                PasswordErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                ClearPasswordError();
            }

            return isValid;
        }

        private void ClearLoginError()
        {
            LoginGroupBox.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF673AB7"));
            LoginHeader.Foreground = Brushes.Black;
            LoginErrorText.Visibility = Visibility.Collapsed;
        }
    }
}
