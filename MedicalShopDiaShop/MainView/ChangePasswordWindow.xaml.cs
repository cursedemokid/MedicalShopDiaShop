using MedicalShopDiaShop.AppData;
using MedicalShopDiaShop.Database;
using System;
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

            if (App.currentUser.Id == _userId)
            {
                if (!PasswordHelper.VerifyPassword(oldPwd, user.Password))
                {
                    FeedbackService.Error("Неверный старый пароль.");
                    return;
                }
            }

            user.Password = PasswordHelper.HashPassword(newPwd);
            App.context.SaveChanges();

            // Уведомление пользователю
            NotificationHelper.CreateNotification(_userId,
                $"Ваш пароль был успешно изменён.");

            // Если администратор меняет пароль другому сотруднику
            if (App.currentUser.Id != _userId)
            {
                NotificationHelper.CreateNotification(App.currentUser.Id,
                    $"Вы изменили пароль пользователю {user.LastName} {user.FirstName}.");
            }

    (Application.Current.MainWindow as MainWindow)?.UpdateNotificationBadge();

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