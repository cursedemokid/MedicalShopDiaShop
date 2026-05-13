using System;

namespace MedicalShopDiaShop.AppData
{
    /// <summary>
    /// Сигнализирует об изменении данных в БД, чтобы открытые списки перезагрузились.
    /// </summary>
    public static class DataRefreshHub
    {
        public static event EventHandler DataChanged;

        public static void Notify()
        {
            DataChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}
