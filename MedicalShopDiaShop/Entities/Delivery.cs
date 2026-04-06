using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalShopDiaShop.Entities
{
    public class Delivery
    {
        public long Id { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public long CourierId { get; set; }

        public virtual User Courier { get; set; }
    }
}
