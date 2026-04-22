using System;
using System.Data.Entity;
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
        /// <param name="context">передает контекст, в котором был найден пользователь из авторизации</param>
        /// <returns>true, если пароль верен</returns>
        public static bool VerifyAndUpgradePassword(string enteredPassword, User user, DbContext context)
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
                    context.SaveChanges();
                    return true;
                }
                return false;
            }
        }

        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(enteredPassword) || string.IsNullOrEmpty(storedHash))
                return false;

            // Если сохранённый пароль не похож на хэш (длина не 44 или не заканчивается на '='), считаем его открытым текстом
            bool isHashFormat = storedHash.Length == 44 && storedHash.EndsWith("=");
            if (isHashFormat)
            {
                string hashOfEntered = HashPassword(enteredPassword);
                return hashOfEntered == storedHash;
            }
            else
            {
                // Старый формат – открытый текст
                return enteredPassword == storedHash;
            }
        }
    }
}