using System;
using System.Windows;

namespace MedicalShopDiaShop.AppData
{
    internal class FeedbackService
    {
        public static void Information(string message, string caption = "Информация")
        {
            CustomMessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void Error(string message, string caption = "Ошибка")
        {
            CustomMessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void Warning(string message, string caption = "Предупреждение")
        {
            CustomMessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public static MessageBoxResult Question(string message, string caption = "Вопрос")
        {
            return CustomMessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question);
        }

        public static void Error(Exception exception)
        {
            CustomMessageBox.Show(exception.Message, exception.HelpLink ?? "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}