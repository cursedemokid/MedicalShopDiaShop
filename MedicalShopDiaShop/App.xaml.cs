using MedicalShopDiaShop.Database;
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
        public static DiaShopEntities2 context = new DiaShopEntities2();
        public static Database.User currentUser = new Database.User();
        public static Button currentButton = new Button();
    }
}
