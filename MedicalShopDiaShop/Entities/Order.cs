using System;
using System.Collections.Generic;

namespace MedicalShopDiaShop.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public int ClientId { get; set; }
        public decimal TotalCost { get; set; }
        public int DeliveryType { get; set; }
        public int WorkerId { get; set; }

        public User Client { get; set; }
        public User Worker { get; set; }
        public ICollection<ProductOrder> ProductOrders { get; set; }
    }

    public enum DeliveryType
    {
        Courier = 1,
        Self = 2
    }
}
