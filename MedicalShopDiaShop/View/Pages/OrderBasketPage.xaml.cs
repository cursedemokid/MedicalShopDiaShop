using MedicalShopDiaShop.AppData;
using MedicalShopDiaShop.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MedicalShopDiaShop.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для OrderBasketPage.xaml
    /// </summary>
    public partial class OrderBasketPage : Page
    {
        List<Order> _orders = App.context.Order.ToList();
        List<ProductOrder> _productOrders = App.context.ProductOrder.ToList();
        List<DeliveryType> _deliveryTypes = App.context.DeliveryType.ToList();
        List<PaymentType> _paymentTypes = App.context.PaymentType.ToList();
        List<Product> _products = App.context.Product.ToList();
        Order order = new Order();
        public OrderBasketPage()
        {
            InitializeComponent();

            order = _orders.FirstOrDefault(o => o.UserId == App.currentUser.Id);
            TotalCostRecalculate();

            ProductLb.ItemsSource = _productOrders.Where(pO => pO.OrderId == order.Id);
            TotalCostTbl.Text = order.TotalCost.ToString();
            DeliveryTypeCmb.ItemsSource = _deliveryTypes;
            DeliveryTypeCmb.SelectedIndex = order.DeliveryTypeId - 1;
            PaymentTypeCmb.ItemsSource = _paymentTypes;
            PaymentTypeCmb.SelectedIndex = order.PaymentTypeId - 1;

        }

        private void DeletBtn_Click(object sender, RoutedEventArgs e)
        {
            ProductOrder selectedProductOrder = ProductLb.SelectedItem as ProductOrder;
            if (selectedProductOrder != null)
            {
                if (FeedbackService.Question("Вы уверены, что хотите удалить товар из корзины?") == MessageBoxResult.Yes)
                {

                    Product selectedProduct = _products.FirstOrDefault(p => p.Id == selectedProductOrder.ProductId);
                    ProductOrder productOrder = _productOrders.FirstOrDefault(pO => pO.ProductId == selectedProduct.Id);
                    App.context.ProductOrder.Remove(productOrder);
                    _productOrders.Remove(productOrder);
                    App.context.SaveChanges();
                    _orders = App.context.Order.ToList();
                    _productOrders = App.context.ProductOrder.ToList();
                    order = _orders.FirstOrDefault(o => o.UserId == App.currentUser.Id);
                    TotalCostRecalculate();
                    ProductLb.ItemsSource = _productOrders.Where(pO => pO.OrderId == order.Id);
                    TotalCostTbl.Text = order.TotalCost.ToString();
                }
            }

           
        }

        private void PlusBtn_Click(object sender, RoutedEventArgs e)
        {
            ProductOrder selectedProductOrder = ProductLb.SelectedItem as ProductOrder;
            if (selectedProductOrder != null)
            {
                Product selectedProduct = _products.FirstOrDefault(p => p.Id == selectedProductOrder.ProductId);
                ProductOrder productOrder = _productOrders.FirstOrDefault(pO => pO.ProductId == selectedProduct.Id);
                productOrder.Quantity += 1;
                productOrder.TotalCost = productOrder.Product.Cost * productOrder.Quantity;
                App.context.SaveChanges();
                order = _orders.FirstOrDefault(o => o.UserId == App.currentUser.Id);
                _productOrders = App.context.ProductOrder.ToList();
                TotalCostRecalculate();
                TotalCostTbl.Text = order.TotalCost.ToString();
                ProductLb.ItemsSource = _productOrders.Where(pO => pO.OrderId == order.Id);
            }
            else
            {
                FeedbackService.Error("Вы не выбрали товар из списка");
            }
        }

        private void MinusBtn_Click(object sender, RoutedEventArgs e)
        {
            ProductOrder selectedProductOrder = ProductLb.SelectedItem as ProductOrder;
            if (selectedProductOrder != null)
            {
                Product selectedProduct = _products.FirstOrDefault(p => p.Id == selectedProductOrder.ProductId);
                ProductOrder productOrder = _productOrders.FirstOrDefault(pO => pO.ProductId == selectedProduct.Id);
                if (productOrder.Quantity == 1)
                {
                    if (FeedbackService.Question("Вы уверены, что хотите удалить товар из корзины?") == MessageBoxResult.Yes)
                    {
                        App.context.ProductOrder.Remove(productOrder);
                        _productOrders.Remove(productOrder);
                        App.context.SaveChanges();
                        _orders = App.context.Order.ToList();
                        order = _orders.FirstOrDefault(o => o.UserId == App.currentUser.Id);
                        ProductLb.ItemsSource = _productOrders.Where(pO => pO.OrderId == order.Id);
                        _productOrders = App.context.ProductOrder.ToList();
                        TotalCostRecalculate();
                        TotalCostTbl.Text = order.TotalCost.ToString();

                    }
                }
                else
                {
                    productOrder.Quantity -= 1;
                    productOrder.TotalCost = productOrder.Product.Cost * productOrder.Quantity;
                    ProductLb.ItemsSource = _productOrders.Where(pO => pO.OrderId == order.Id);
                    App.context.SaveChanges();
                    order = _orders.FirstOrDefault(o => o.UserId == App.currentUser.Id);
                    _productOrders = App.context.ProductOrder.ToList();
                    TotalCostRecalculate();
                    TotalCostTbl.Text = order.TotalCost.ToString();
                }


            }

        }

        private void PaymentTypeCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            order.PaymentTypeId = Convert.ToInt32(PaymentTypeCmb.SelectedValue);
            App.context.SaveChanges();
        }

        private void DeliveryTypeCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            order.DeliveryTypeId = Convert.ToInt32(DeliveryTypeCmb.SelectedValue);
            App.context.SaveChanges();
        }

        private void TotalCostRecalculate()
        {
            decimal totalCost = 0;
            foreach (ProductOrder productOrder in _productOrders)
            {
                totalCost += Convert.ToDecimal(productOrder.Product.Cost * productOrder.Quantity);
            }
            order.TotalCost = totalCost;
            App.context.SaveChanges();
        }
    }
}

