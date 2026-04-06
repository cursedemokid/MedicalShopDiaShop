using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalShopDiaShop.Entities
{
    public class Task
    {
        public long Id { get; set; }
        public string Description { get; set; }
        public long UserId { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }

        public virtual User User { get; set; }
    }
}
