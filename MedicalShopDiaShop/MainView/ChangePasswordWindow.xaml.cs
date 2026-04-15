using MedicalShopDiaShop.AppData;
using MedicalShopDiaShop.Database;
using System.Linq;
using System.Windows;

namespace MedicalShopDiaShop.MainView
{
    public partial class ChangePasswordWindow : Window
    {
        private readonly int _userId;

        public ChangePasswordWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            string oldPwd = OldPasswordBox.Password;
            string newPwd = NewPasswordBox.Password;
            string confirmPwd = ConfirmPasswordBox.Password;

            if (string.IsNullOrEmpty(oldPwd) || string.IsNullOrEmpty(newPwd) || string.IsNullOrEmpty(confirmPwd))
            {
                FeedbackService.Error("Заполните все поля.");
                return;
            }

            if (newPwd != confirmPwd)
            {
                FeedbackService.Error("Новый пароль и подтверждение не совпадают.");
                return;
            }

            if (newPwd.Length < 4)
            {
                FeedbackService.Error("Пароль должен содержать не менее 4 символов.");
                return;
            }

            var user = App.context.User.FirstOrDefault(u => u.Id == _userId);
            if (user == null)
            {
                FeedbackService.Error("Пользователь не найден.");
                Close();
                return;
            }

            // Для своего профиля проверяем старый пароль
            if (App.currentUser.Id == _userId && user.Password != oldPwd)
            {
                FeedbackService.Error("Неверный старый пароль.");
                return;
            }

            user.Password = newPwd;
            App.context.SaveChanges();

            FeedbackService.Information("Пароль успешно изменён.");
            DialogResult = true;
            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}