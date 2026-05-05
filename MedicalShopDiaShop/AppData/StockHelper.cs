using MedicalShopDiaShop.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalShopDiaShop.AppData
{
    public class StockItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int InStock { get; set; }  // Остаток
        public int Reserved { get; set; } // Зарезервировано в заказах (не отгружено)
        public int Available => InStock - Reserved;
        public bool IsExpired => ExpirationDate < DateTime.Today;
        public bool IsExpiringSoon => ExpirationDate > DateTime.Today && ExpirationDate <= DateTime.Today.AddDays(30);
    }

    public static class StockHelper
    {
        /// <summary>
        /// Получить остатки по всем товарам с группировкой по партиям (сроку годности)
        /// </summary>
        public static List<StockItem> GetCurrentStock(int storeId)
        {
            using (var context = new DiaShopEntities())
            {
                // Поставки (приход)
                var supplies = context.SupplyProduct
                    .Where(sp => sp.Supply.User.StoreId == storeId)
                    .Select(sp => new
                    {
                        sp.ProductId,
                        sp.Product.Name,
                        sp.ExpirationDate,
                        sp.Quantity
                    })
                    .ToList();

                // Заказы (расход) – только не отменённые и не доставленные? Учитываем только заказы в статусе "В обработке" и "Ожидает курьера"
                var orders = context.ProductOrder
                    .Where(po => po.Order.User.StoreId == storeId &&
                                 (po.Order.Status == (int)Status.OrderStatus.InProcess ||
                                  po.Order.Status == (int)Status.OrderStatus.WaitCourier))
                    .Select(po => new
                    {
                        po.ProductId,
                        po.Quantity
                    })
                    .ToList();

                // Группируем приход по товару и сроку годности
                var stockIn = supplies
                    .GroupBy(s => new { s.ProductId, ExpirationDate = s.ExpirationDate ?? DateTime.MaxValue })
                    .Select(g => new StockItem
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.First().Name,
                        ExpirationDate = g.Key.ExpirationDate,
                        InStock = g.Sum(x => x.Quantity),
                        Reserved = 0
                    })
                    .ToList();

                // Группируем расход по товару (без учёта сроков – FIFO будет позже)
                var stockOut = orders
                    .GroupBy(o => o.ProductId)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

                // FIFO: списываем сначала те партии, у которых срок годности истекает раньше
                foreach (var item in stockIn.OrderBy(i => i.ExpirationDate))
                {
                    if (stockOut.TryGetValue(item.ProductId, out int needed))
                    {
                        int take = Math.Min(needed, item.InStock);
                        item.Reserved = take;
                        stockOut[item.ProductId] = needed - take;
                        if (stockOut[item.ProductId] == 0)
                            stockOut.Remove(item.ProductId);
                    }
                }

                // Возвращаем только те позиции, где есть остаток или скоро истекает
                return stockIn.Where(i => i.Available > 0 || i.IsExpiringSoon || i.IsExpired).ToList();
            }
        }

        /// <summary>
        /// Проверить, достаточно ли товара на складе для заказа (с учётом FIFO)
        /// </summary>
        public static bool CheckAvailability(int productId, int quantity, int storeId)
        {
            var stock = GetCurrentStock(storeId);
            int totalAvailable = stock.Where(s => s.ProductId == productId).Sum(s => s.Available);
            return totalAvailable >= quantity;
        }

        /// <summary>
        /// Создать уведомления о товарах с истекающим сроком годности
        /// </summary>
        public static void CheckExpiringProducts()
        {
            using (var context = new DiaShopEntities())
            {
                var stores = context.Store.Select(s => s.Id).ToList();
                foreach (var storeId in stores)
                {
                    var expiring = GetCurrentStock(storeId)
                        .Where(s => s.IsExpiringSoon && !s.IsExpired)
                        .GroupBy(s => s.ProductId)
                        .Select(g => g.First())
                        .ToList();

                    foreach (var item in expiring)
                    {
                        string message = $"Товар '{item.ProductName}' истекает {item.ExpirationDate:dd.MM.yyyy}. Остаток: {item.Available} шт.";
                        // Уведомляем всех сотрудников магазина
                        var employees = context.User.Where(u => u.StoreId == storeId && u.Role != (int)Status.Role.Client);
                        foreach (var emp in employees)
                        {
                            if (!context.Notification.Any(n => n.UserId == emp.Id && n.Text == message && !n.IsRead))
                            {
                                context.Notification.Add(new Notification
                                {
                                    UserId = emp.Id,
                                    Text = message,
                                    IsRead = false
                                });
                            }
                        }
                    }
                }
                context.SaveChanges();
            }
        }
    }
}