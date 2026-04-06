using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalShopDiaShop.Entities
{
    public class Store
    {
        public long Id { get; set; } 
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
    }

    public enum City
    {

    }
}
