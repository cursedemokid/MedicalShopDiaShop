using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MedicalShopDiaShop.MainView.Controls
{
    public partial class SearchBarControl : UserControl
    {
        public SearchBarControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
            nameof(PlaceholderText),
            typeof(string),
            typeof(SearchBarControl),
            new PropertyMetadata("Поиск"));

        public string PlaceholderText
        {
            get => (string)GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        public event TextChangedEventHandler FilterTextChanged;
        public event RoutedEventHandler SearchClicked;

        public string Text
        {
            get => SearchTextBox.Text;
            set => SearchTextBox.Text = value ?? string.Empty;
        }

        private void SearchTextBox_OnTextChanged(object sender, TextChangedEventArgs e) =>
            FilterTextChanged?.Invoke(sender, e);

        private void SearchButton_Click(object sender, RoutedEventArgs e) =>
            SearchClicked?.Invoke(this, e);

        private void SearchTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchClicked?.Invoke(this, e);
                e.Handled = true;
            }
        }
    }
}
