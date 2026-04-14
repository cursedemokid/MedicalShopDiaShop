using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalShopDiaShop.AppData
{
    public class Status
    {
        public enum DeliveryType
        {
            Courier = 1,
            Self = 2
        }
        public enum OrderStatus
        {
            InProcess = 1,
            WaitCourier = 2,
            Delivered = 3,
            WaitPayment = 4,
            InHistory = 5,
            ChoosingSupplier = 6,   // Шаг 1
            ChoosingProducts = 7,   // Шаг 2
            ChoosingAroundTime = 8  // Шаг 3
        }

        public static OrderStatus GetOrderStatus(int statusId)
        {
            return Enum.IsDefined(typeof(OrderStatus), statusId)
                ? (OrderStatus)statusId
                : OrderStatus.InProcess;
        }

        public static int GetOrderStatusId(OrderStatus status)
        {
            return (int)status;
        }
        public enum Role
        {
            Admin = 1,
            Client = 2,
            Worker = 3,
            Courier = 4,
            Supplier = 5
        }
        public enum City
        {

        }
        public enum StoreType
        {
            DiaShop = 1,
            SupplierStore = 2,
        }
        public enum Category
        {
            Glucometers = 1,
            TestStrips = 2,
            Syringes = 3,
            Creams = 4,
            Vitamins = 5
        }
    }
}
