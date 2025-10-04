using MedicalShopDiaShop.AppData;
using MedicalShopDiaShop.Model;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MedicalShopDiaShop.View.Windows
{
    /// <summary>
    /// Логика взаимодействия для RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        List<User> _users = App.context.User.ToList();
        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void RegistrationBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(EmailTbx.Text) || string.IsNullOrEmpty(PasswordPbx.Password) || string.IsNullOrEmpty(ConfirmPasswordPbx.Password))
            {
                FeedbackService.Error("Заполните все поля и повторите попытку!");
            }
            else if (PasswordPbx.Password != ConfirmPasswordPbx.Password)
            {
                FeedbackService.Error("Пароли не совпадают! Проверьте корректность введенных данных и повторите попытку");
            }
            else
            {

                if (_users.FirstOrDefault(u => u.Email == EmailTbx.Text) == null)
                {
                    App.context.User.Add(new User
                    {
                        Email = EmailTbx.Text,
                        Password = PasswordPbx.Password,
                        FirstName = "Пользователь",
                        LastName = "Новый",
                    });
                    App.context.SaveChanges();
                    FeedbackService.Information("Вы успешно зарегистрировались!");
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    Close();
                }
            }
        }
    }
}
