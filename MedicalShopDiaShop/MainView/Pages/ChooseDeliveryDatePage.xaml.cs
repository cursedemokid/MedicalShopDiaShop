using MedicalShopDiaShop.AppData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MedicalShopDiaShop.MainView.Pages
{
    /// <summary>
    /// Логика взаимодействия для ChooseDeliveryDatePage.xaml
    /// </summary>
    public partial class ChooseDeliveryDatePage : Page
    {
        private readonly AddSupplyWindow _parentWindow;
        public ChooseDeliveryDatePage(AddSupplyWindow parent)
        {
            InitializeComponent();
            _parentWindow = parent;
            if (_parentWindow.DeliveryDate.HasValue)
                DeliveryDatePicker.SelectedDate = _parentWindow.DeliveryDate.Value;
        }

        private void CreateSupplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (DeliveryDatePicker.SelectedDate.HasValue)
            {
                _parentWindow.DeliveryDate = DeliveryDatePicker.SelectedDate.Value;
                _parentWindow.GoToNextStep(); // вызовет CreateSupply()
            }
            else
            {
                FeedbackService.Information("Выберите дату доставки.");
            }
        }
    }
}
