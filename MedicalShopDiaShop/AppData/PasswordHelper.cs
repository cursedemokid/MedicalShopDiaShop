using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using MedicalShopDiaShop.Database;

namespace MedicalShopDiaShop.AppData
{
    public static class PasswordHelper
    {
        /// <summary>
        /// Хэширует пароль с помощью SHA256 и возвращает Base64-строку.
        /// </summary>
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        /// <summary>
        /// Проверяет пароль, поддерживая как хэшированный, так и открытый формат.
        /// Если пароль в БД хранится в открытом виде и совпадает с введённым,
        /// он автоматически заменяется на хэш (повышение безопасности).
        /// </summary>
        /// <param name="enteredPassword">Пароль, введённый пользователем</param>
        /// <param name="user">Объект пользователя из БД</param>
        /// <returns>true, если пароль верен</returns>
        public static bool VerifyAndUpgradePassword(string enteredPassword, User user)
        {
            if (user == null || string.IsNullOrEmpty(enteredPassword))
                return false;

            string storedPassword = user.Password;
            if (string.IsNullOrEmpty(storedPassword))
                return false;

            // Проверка: является ли storedPassword хэшем SHA256 в Base64 (44 символа, заканчивается на =)
            bool isHashFormat = storedPassword.Length == 44 && storedPassword.EndsWith("=");

            if (isHashFormat)
            {
                // Формат хэша – сравниваем хэш введённого пароля с сохранённым
                string hashOfEntered = HashPassword(enteredPassword);
                return hashOfEntered == storedPassword;
            }
            else
            {
                // Старый формат – открытый текст
                if (enteredPassword == storedPassword)
                {
                    // Пароль верен – обновляем запись на хэш
                    user.Password = HashPassword(enteredPassword);
                    using (var context = new DiaShopEntities3())
                    {
                        context.Entry(user).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();
                    }
                    return true;
                }
                return false;
            }
        }
    }
}