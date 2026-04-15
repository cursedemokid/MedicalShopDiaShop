using MedicalShopDiaShop.Database;
using System;
using System.Linq;

namespace MedicalShopDiaShop.AppData
{
    public static class NotificationHelper
    {
        public static void CreateNotification(int userId, string text, int? taskId = null, int? supplyId = null)
        {
            using (var context = new DiaShopEntities3())
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Text = text,
                    TaskId = taskId,
                    SupplyId = supplyId,
                    IsRead = false
                };
                context.Notification.Add(notification);
                context.SaveChanges();
            }
        }

        public static void NotifyAllStoreEmployees(int storeId, string text, int? taskId = null, int? supplyId = null)
        {
            using (var context = new DiaShopEntities3())
            {
                var employees = context.User
                    .Where(u => u.StoreId == storeId && u.IsDeleted != true)
                    .Select(u => u.Id)
                    .ToList();

                foreach (var empId in employees)
                {
                    CreateNotification(empId, text, taskId, supplyId);
                }
            }
        }
    }
}