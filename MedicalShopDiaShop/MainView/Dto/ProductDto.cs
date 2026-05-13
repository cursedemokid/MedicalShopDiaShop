using System;

namespace MedicalShopDiaShop.MainView.Dto
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Category { get; set; }      // числовой код категории
        public decimal Price { get; set; }
        public string Image { get; set; }
        public int AvailableQuantity { get; set; }

        // UI-свойства
        public bool IsSelected { get; set; }

        public string CategoryName
        {
            get
            {
                switch (Category)
                {
                    case 1: return "Глюкометры";
                    case 2: return "Тест-полоски";
                    case 3: return "Шприцы";
                    case 4: return "Кремы";
                    case 5: return "Витамины";
                    default: return "Неизвестно";
                }
            }
            set { }
        }

        public string ShortDescription => Description?.Length > 50
            ? Description.Substring(0, 50) + "..."
            : Description;

        public string ImagePath => Image ?? "/Resources/default_product.png";
        public string AvailableText => $"{AvailableQuantity} шт.";
    }
}