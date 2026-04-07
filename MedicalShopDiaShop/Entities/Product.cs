using System.Collections.Generic;

namespace MedicalShopDiaShop.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Category { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; }

        public ICollection<ProductOrder> ProductOrders { get; set; }
        public ICollection<Favorite> Favorites { get; set; }
    }

    public enum Category
    {
        Glucometers = 1,
        TestStrips = 2,
        Syringes = 3,
        Creams = 4,
        Vitamins = 5
    }

}
