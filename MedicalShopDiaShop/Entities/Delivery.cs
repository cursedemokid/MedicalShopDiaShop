using System;

namespace MedicalShopDiaShop.Entities
{
    public class Delivery
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int CourierId { get; set; }

        public User Courier { get; set; }
    }
}