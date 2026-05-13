using System;
using System.Globalization;
using System.Windows.Data;

namespace MedicalShopDiaShop.AppData
{
    public class RelativeDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DateTime date))
                return string.Empty;

            var now = DateTime.Now;
            var diff = now - date;

            if (diff.TotalMinutes < 1)
                return "только что";
            if (diff.TotalHours < 1)
            {
                int minutes = (int)diff.TotalMinutes;
                return $"{minutes} мин назад";
            }
            if (diff.TotalHours < 24)
            {
                int hours = (int)diff.TotalHours;
                // Склонение: 1 час, 2-4 часа, 5+ часов
                string hourWord = GetHourWord(hours);
                return $"{hours} {hourWord} назад";
            }

            // Если прошло больше суток – выводим дату и время в российском формате
            return date.ToString("dd.MM.yyyy HH:mm");
        }

        private string GetHourWord(int hours)
        {
            if (hours == 1 || (hours % 10 == 1 && hours % 100 != 11))
                return "час";
            if (hours >= 2 && hours <= 4 || (hours % 10 >= 2 && hours % 10 <= 4 && hours % 100 != 12 && hours % 100 != 13 && hours % 100 != 14))
                return "часа";
            return "часов";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}