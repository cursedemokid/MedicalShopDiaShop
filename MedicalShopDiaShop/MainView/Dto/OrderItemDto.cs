namespace MedicalShopDiaShop.MainView.Dto
{
    public class OrderItemDto
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal PricePerUnit { get; set; }
        public string ImagePath { get; set; }
        public decimal TotalPrice => PricePerUnit * Quantity;
    }
}