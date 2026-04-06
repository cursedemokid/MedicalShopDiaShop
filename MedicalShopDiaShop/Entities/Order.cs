using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalShopDiaShop.Entities
{
    internal class Order
    {
        public long Id { get; set; }
        public DateTime DateTime { get; set; }
        public long ClientId { get; set; }
        public decimal TotalCost { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public long WorkerId { get; set; }

        public virtual User Client {  get; set; }
        public virtual User Worker {  get; set; }
    }

    public enum DeliveryType
    {
        Courier = 1,
        Self = 2
    }
}
