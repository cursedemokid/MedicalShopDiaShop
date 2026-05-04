using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MedicalShopDiaShop.MainView.Dto
{
    public class OrderDto : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string ClientFullName { get; set; }
        public string WorkerFullName { get; set; }
        public string CourierFullName { get; set; }
        public DateTime? DeliveryStartDate { get; set; }
        public DateTime? DeliveryEndDate { get; set; }
        public string DeliveryDescription { get; set; }
        public int DeliveryType { get; set; }
        public decimal TotalCost { get; set; }
        public List<OrderItemDto> Items { get; set; }

        private int _status;
        public int Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusName)); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

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
                    case 9: return "У курьера";
                    default: return "Неизвестно";
                }
            }
        }

        public string DeliveryTypeName => DeliveryType == 1 ? "Курьер" : "Самовывоз";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}