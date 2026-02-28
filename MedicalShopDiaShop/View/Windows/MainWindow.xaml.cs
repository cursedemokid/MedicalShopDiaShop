using MedicalShopDiaShop.AppData;
using MedicalShopDiaShop.Model;
using MedicalShopDiaShop.Properties;
using MedicalShopDiaShop.View.Windows;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MedicalShopDiaShop
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<User> _users = App.context.User.ToList();
        public MainWindow()
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(Settings.Default.SavedLogin))
            {
                EmailTbx.Text = Settings.Default.SavedLogin;
                if(!string.IsNullOrEmpty(Settings.Default.SavedPassword))
                {
                    PasswordPbx.Password = Settings.Default.SavedPassword;
                }
                RememberMeCb.IsChecked = true;
            }
        }

        private void EnterBtn_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrEmpty(EmailTbx.Text) && string.IsNullOrEmpty(PasswordPbx.Password))
            {
                FeedbackService.Error("Вы не заполнили поля, заполните почту и пароль и повторите попытку");
            }
            else if (string.IsNullOrEmpty(EmailTbx.Text))
            {
                FeedbackService.Error("Вы не заполнили поле Почта, заполните и повторите попытку");
            }
            else if (string.IsNullOrEmpty(PasswordPbx.Password))
            {
                FeedbackService.Error("Вы не заполнили поле Пароль, заполните и повторите попытку");
            }
            else
            {
                if (_users.FirstOrDefault(u => u.Email == EmailTbx.Text && u.Password == PasswordPbx.Password) != null )
                {
                    App.currentUser = _users.FirstOrDefault(u => u.Email == EmailTbx.Text && u.Password == PasswordPbx.Password);
                    FeedbackService.Information($"Добро пожаловать, {App.currentUser.FirstName}! Вы успешно авторизовались!");
                    if(RememberMeCb.IsChecked == true)
                    {
                        Settings.Default.SavedLogin = EmailTbx.Text;
                        Settings.Default.SavedPassword = PasswordPbx.Password; 
                    }
                    else
                    {
                        Settings.Default.SavedLogin = string.Empty;
                        Settings.Default.SavedPassword = string.Empty;
                    }
                    Settings.Default.Save();
                    StoreWindow storeWindow = new StoreWindow();
                    storeWindow.Show();
                    Close();
                }
                if (_users.FirstOrDefault(u => u.Email == EmailTbx.Text && u.Password == PasswordPbx.Password) == null)
                {
                    FeedbackService.Error("Вы неправильно ввели пароль или почту. Заполните их заново и повторите попытку");
                    PasswordPbx.Password = "";
                }
            }
        }

        private void RegistrationBtn_Click(object sender, RoutedEventArgs e)
        {
            RegistrationWindow registrationWindow = new RegistrationWindow();
            registrationWindow.Show();
            Close();
        }

    }
}
