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

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        private int _status;
        public int Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusString));
                OnPropertyChanged(nameof(StatusName));
            }
        }

        public string StatusString
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
                    default: return "В обработке";
                }
            }
            set
            {
                switch (value)
                {
                    case "В обработке": Status = 1; break;
                    case "Ожидает курьера": Status = 2; break;
                    case "Доставлен": Status = 3; break;
                    case "Ожидает оплаты": Status = 4; break;
                    case "В истории": Status = 5; break;
                    case "У курьера": Status = 9; break;
                    default: Status = 1; break;
                }
            }
        }

        public string StatusName => StatusString;
        public string DeliveryTypeName => DeliveryType == 1 ? "Курьер" : "Самовывоз";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}