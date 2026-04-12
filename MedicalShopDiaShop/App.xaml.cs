using MedicalShopDiaShop.Database;
using MedicalShopDiaShop.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MedicalShopDiaShop
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static DiaShopEntities context = new DiaShopEntities();
        public static Database.User currentUser = new Database.User();
        //public static MedicalShopDiaShopEntities1 context = new MedicalShopDiaShopEntities1();
        public static Button currentButton = new Button();
    }

    public enum PageType
    {
        Popular,
        New,
        LowCarbohydrates,
        WithoutGluten,
        Favorite,
        History
    }
}
