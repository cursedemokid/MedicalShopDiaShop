using MedicalShopDiaShop.Database;
using MedicalShopDiaShop.Model;
using MedicalShopDiaShop.Properties;
using MedicalShopDiaShop.View.Windows;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MedicalShopDiaShop.MainView
{
    public partial class AuthorizationWindow : Window
    {
        public AuthorizationWindow()
        {
            InitializeComponent();
            LoadSavedCredentials();
        }

        private void LoadSavedCredentials()
        {
            if (!string.IsNullOrEmpty(Settings.Default.SavedLogin))
            {
                LoginTextBox.Text = Settings.Default.SavedLogin;
                if (!string.IsNullOrEmpty(Settings.Default.SavedPassword))
                {
                    PasswordBox.Password = Settings.Default.SavedPassword;
                }
                RememberMeCheckBox.IsChecked = true;
            }
        }

        private void RegistrationBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RegistrationWindow registrationWindow = new RegistrationWindow();
            registrationWindow.Show();
            Close();
        }

        private void EnterBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateFields())
            {
                string login = LoginTextBox.Text.Trim();
                string password = PasswordBox.Password;

                // Поиск пользователя по Email или UserName
                var user = App.context.User.FirstOrDefault(u => u.Email == login || u.UserName == login);
                if (user != null && user.Password == password)
                {
                    App.currentUser = user;

                    // Сохраняем или очищаем настройки
                    if (RememberMeCheckBox.IsChecked == true)
                    {
                        Settings.Default.SavedLogin = login;
                        Settings.Default.SavedPassword = password;
                    }
                    else
                    {
                        Settings.Default.SavedLogin = string.Empty;
                        Settings.Default.SavedPassword = string.Empty;
                    }
                    Settings.Default.Save();

                    // Открываем главное окно (например, StoreWindow)
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    Close();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль. Попробуйте снова.",
                                    "Ошибка авторизации",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    PasswordBox.Password = "";
                }
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
            PasswordGroupBox.BorderBrush = new SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF673AB7"));
            PasswordHeader.Foreground = System.Windows.Media.Brushes.Black;
            PasswordErrorText.Visibility = Visibility.Collapsed;
        }

        private bool ValidateFields()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(LoginTextBox.Text))
            {
                LoginGroupBox.BorderBrush = System.Windows.Media.Brushes.Red;
                LoginHeader.Foreground = System.Windows.Media.Brushes.Red;
                LoginErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                ClearLoginError();
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                PasswordGroupBox.BorderBrush = System.Windows.Media.Brushes.Red;
                PasswordHeader.Foreground = System.Windows.Media.Brushes.Red;
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
            LoginGroupBox.BorderBrush = new SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF673AB7"));
            LoginHeader.Foreground = System.Windows.Media.Brushes.Black;
            LoginErrorText.Visibility = Visibility.Collapsed;
        }
    }
}