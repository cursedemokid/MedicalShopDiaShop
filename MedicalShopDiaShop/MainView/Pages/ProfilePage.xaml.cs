using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Логика взаимодействия для ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            LoadHistory();
            LoadTasks();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        public class OrderHistoryItem
        {
            public ObservableCollection<OrderItem> Items { get; set; }
            public DateTime OrderDate { get; set; }
            public decimal TotalOrderPrice { get; set; }
            public ICommand ViewDetailsCommand { get; set; }
        }

        public class OrderItem
        {
            public string ImageSource { get; set; }
            public string Name { get; set; }
            public int Quantity { get; set; }
            public decimal PricePerUnit { get; set; }
            public decimal TotalPrice => PricePerUnit * Quantity;
        }

        private void LoadHistory()
        {
            var history = new ObservableCollection<OrderHistoryItem>
    {
        new OrderHistoryItem
        {
            OrderDate = DateTime.Today.AddDays(-2),
            Items = new ObservableCollection<OrderItem>
            {
                new OrderItem
                {
                    ImageSource = "/Resources/ProductsImages/ChicoryDrink.jpg",
                    Name = "Напиток цикорий",
                    Quantity = 2,
                    PricePerUnit = 150.50m
                },
                new OrderItem
                {
                    ImageSource = "/Resources/ProductsImages/BodyLotion.jpg",
                    Name = "Лосьон для тела",
                    Quantity = 1,
                    PricePerUnit = 320.00m
                },
                new OrderItem
                {
                    ImageSource = "/Resources/ProductsImages/ContourPlus.jpg",
                    Name = "Контур Плюс",
                    Quantity = 5,
                    PricePerUnit = 45.00m
                }
            },
            TotalOrderPrice = 2*150.50m + 1*320.00m + 5*45.00m,
            ViewDetailsCommand = new RelayCommand(() => ShowDetails("Заказ #1"))
        },
        new OrderHistoryItem
        {
            OrderDate = DateTime.Today.AddDays(-7),
            Items = new ObservableCollection<OrderItem>
            {
                new OrderItem
                {
                    ImageSource = "/Resources/vitamins.png",
                    Name = "Витамины",
                    Quantity = 1,
                    PricePerUnit = 890.00m
                },
                new OrderItem
                {
                    ImageSource = "/Resources/thermometer.png",
                    Name = "Термометр",
                    Quantity = 1,
                    PricePerUnit = 250.00m
                }
            },
            TotalOrderPrice = 890.00m + 250.00m,
            ViewDetailsCommand = new RelayCommand(() => ShowDetails("Заказ #2"))
        }
    };
            HistoryListBox.ItemsSource = history;
        }

        public class RelayCommand : ICommand
        {
            private readonly Action _execute;
            public RelayCommand(Action execute) => _execute = execute;
            public event EventHandler CanExecuteChanged;
            public bool CanExecute(object parameter) => true;
            public void Execute(object parameter) => _execute();
        }

        private void ShowDetails(string orderId)
        {
            MessageBox.Show($"Открыть подробности: {orderId}");
        }

        private void LoadTasks()
        {
            var tasks = new ObservableCollection<TaskItem>
    {
        new TaskItem
        {
            Title = "Проверить остатки лекарств",
            StartDate = DateTime.Now.AddHours(2),
            Deadline = DateTime.Now.AddDays(1),
            IsCompleted = false,
            AuthorName = "BO$$"
        },
        new TaskItem
        {
            Title = "Создать отчёт по продажам",
            StartDate = DateTime.Now.AddDays(1).AddHours(9),
            Deadline = DateTime.Now.AddDays(2).AddHours(18),
            IsCompleted = false,
            AuthorName = "BO$$"
        },
        new TaskItem
        {
            Title = "Заказать партию витаминов",
            StartDate = DateTime.Now.AddDays(1).AddHours(10),
            Deadline = DateTime.Now.AddDays(3),
            IsCompleted = true,
            AuthorName = "BO$$"
        },
        new TaskItem
        {
            Title = "Обновить прайс-лист",
            StartDate = DateTime.Now.AddDays(2).AddHours(14),
            Deadline = DateTime.Now.AddDays(2).AddHours(17),
            IsCompleted = false,
            AuthorName = "BO$$"
        },
        new TaskItem
        {
            Title = "Провести инвентаризацию",
            StartDate = DateTime.Now.AddDays(3).AddHours(9),
            Deadline = DateTime.Now.AddDays(4).AddHours(18),
            IsCompleted = false,
            AuthorName = "BO$$"
        }
    };
            TasksListBox.ItemsSource = tasks;
        }

        public class TaskItem
        {
            public string Title { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime Deadline { get; set; }
            public bool IsCompleted { get; set; }
            public string AuthorName { get; set; }
        }
    }
}
