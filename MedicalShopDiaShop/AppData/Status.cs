using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalShopDiaShop.AppData
{
    public class Status
    {
        public enum OrderStatus
        {
            Path = 0, //В  пути
            History = 1, //В истории
            Wait = 2, //В ожидании
            MakingOrder = 3 //Оформление
        }
    }
}
