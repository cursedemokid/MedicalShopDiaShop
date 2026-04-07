using System;
using System.Collections.Generic;

namespace MedicalShopDiaShop.MainView.Pages
{
    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string ClientFullName { get; set; }
        public string WorkerFullName { get; set; }
        public string CourierFullName { get; set; }
        public DateTime? DeliveryStartDate { get; set; }
        public DateTime? DeliveryEndDate { get; set; }
        public string DeliveryDescription { get; set; }
        public int Status { get; set; }
        public string StatusName
        {
            get
            {
                switch (Status)
                {
                    case 1: return "В обработке";
                    case 2: return "Ожидает курьера";
                    case 3: return "Доставлен";
                    case 4: return "Ожидает оплаты";
                    case 5: return "В истории";
                    default: return "Неизвестно";
                }
            }
        }
        public decimal TotalCost { get; set; }
        public int DeliveryType { get; set; }
        public string DeliveryTypeName => DeliveryType == 1 ? "Курьер" : "Самовывоз";

        public bool IsSelected { get; set; }
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }

    public class OrderItemDto
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal PricePerUnit { get; set; }
        public decimal TotalPrice => Quantity * PricePerUnit;
        public string ImagePath { get; set; }
    }
}