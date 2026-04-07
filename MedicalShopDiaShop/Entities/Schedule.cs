using System;

namespace MedicalShopDiaShop.Entities
{
    public class Schedule
    {
        public int Id { get; set; }
        public DateTime DateStart { get; set; }
        public int UserId { get; set; }
        public int Hours { get; set; }
        public DateTime FactStartAt { get; set; }
        public DateTime FactExitAt { get; set; }
        public int FactHours { get; set; }

        public User User { get; set; }
    }
}