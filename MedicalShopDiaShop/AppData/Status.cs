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
    }
}
