using MedicalShopDiaShop.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalShopDiaShop.AppData
{
    public class ProductWithFavorite
    {
        public Product Product { get; set; }
        public bool IsFavorite { get; set; }

        public string Name => Product.Name;
        public decimal Cost => Product.Cost;
        public string Image => Product.Image;
        public int Id => Product.Id;
    }
}
