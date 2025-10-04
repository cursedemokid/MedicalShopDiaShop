using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MedicalShopDiaShop.AppData
{
    internal class FeedbackService
    {
        public static void Information(string message, string caption = "Информация")
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        public static void Error(string message, string caption = "Ошибка")
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        public static void Warning(string message, string caption = "Предупреждение")
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        public static MessageBoxResult Question(string message, string caption = "Вопрос")
        {
            return MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question);
        }
        public static void Error(Exception exception)
        {
            MessageBox.Show(exception.Message, exception.HelpLink, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
