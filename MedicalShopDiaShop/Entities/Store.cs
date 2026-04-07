using System.Collections.Generic;

namespace MedicalShopDiaShop.Entities
{
    public class Store
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int City { get; set; }

        public ICollection<User> Users { get; set; }
    }

    public enum City
    {

    }
}
