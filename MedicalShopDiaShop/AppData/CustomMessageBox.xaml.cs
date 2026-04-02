using System.Windows;
using MaterialDesignThemes.Wpf;

namespace MedicalShopDiaShop.AppData
{
    public partial class CustomMessageBox : Window
    {
        private MessageBoxResult _result = MessageBoxResult.None;

        public CustomMessageBox(string message, string caption, MessageBoxButton buttons, MessageBoxImage icon)
        {
            InitializeComponent();
            MessageText.Text = message;
            TitleText.Text = caption;
            Owner = Application.Current.MainWindow;

            PackIconKind iconKind;
            switch (icon)
            {
                case MessageBoxImage.Information:
                    iconKind = PackIconKind.Information;
                    break;
                case MessageBoxImage.Error:
                    iconKind = PackIconKind.Error;
                    break;
                case MessageBoxImage.Warning:
                    iconKind = PackIconKind.Warning;
                    break;
                case MessageBoxImage.Question:
                    iconKind = PackIconKind.Help;
                    break;
                default:
                    iconKind = PackIconKind.Information;
                    break;
            }
            Icon.Kind = iconKind;

            switch (buttons)
            {
                case MessageBoxButton.OK:
                    YesButton.Visibility = Visibility.Collapsed;
                    NoButton.Visibility = Visibility.Collapsed;
                    OkButton.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.OKCancel:
                    YesButton.Visibility = Visibility.Collapsed;
                    OkButton.Visibility = Visibility.Visible;
                    NoButton.Visibility = Visibility.Visible;
                    NoButton.Content = "Отмена";
                    break;
                case MessageBoxButton.YesNo:
                    YesButton.Visibility = Visibility.Visible;
                    NoButton.Visibility = Visibility.Visible;
                    OkButton.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.YesNoCancel:
                    YesButton.Visibility = Visibility.Visible;
                    NoButton.Visibility = Visibility.Visible;
                    OkButton.Content = "Отмена";
                    break;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            _result = MessageBoxResult.OK;
            Close();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            _result = MessageBoxResult.Yes;
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            _result = MessageBoxResult.No;
            Close();
        }

        public static MessageBoxResult Show(string message, string caption = "",
            MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None)
        {
            var dialog = new CustomMessageBox(message, caption, buttons, icon);
            dialog.ShowDialog();
            return dialog._result;
        }
    }
}