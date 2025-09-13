using MedicalShopDiaShop.View.Windows;
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

namespace MedicalShopDiaShop
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ForgotPasswordBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EnterBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RegistrationBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ChooseRoleBtn_Click(object sender, RoutedEventArgs e)
        {
            ChooseRoleWindow chooseRoleWindow = new ChooseRoleWindow();
            chooseRoleWindow.Show();
            Close();
        }
    }
}
