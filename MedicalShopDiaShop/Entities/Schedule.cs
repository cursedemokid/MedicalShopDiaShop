using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalShopDiaShop.Entities
{
    internal class Schedule
    {
        public long Id { get; set; }
        public DateTime DateStart { get; set; }
        public long UserId { get; set; }
        public double Hours { get; set; }
        public DateTime FactStartAt { get; set; }
        public DateTime FactExitAt { get; set; }
        public double FactHours { get; set; }

        public virtual User User { get; set; }
    }
}
