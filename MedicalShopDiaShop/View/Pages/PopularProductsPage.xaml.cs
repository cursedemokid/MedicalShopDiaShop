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
    /// Логика взаимодействия для PopularProductsPage.xaml
    /// </summary>
    public partial class PopularProductsPage : Page
    {
        List<ProductOrder> _productOrders = App.context.ProductOrder.ToList();
        List<Order> _orders = App.context.Order.ToList();
        List<User> _users = App.context.User.ToList();
        public PopularProductsPage()
        {
            InitializeComponent();

            ProductLb.ItemsSource = App.context.Product.ToList();
        }


        private void AddToBasketBtn_Click(object sender, RoutedEventArgs e)
        {
            Product product = ProductLb.SelectedItem as Product;
            if (product != null)
            {
                if (_orders.FirstOrDefault(o => o.UserId == App.currentUser.Id) != null)
                {
                    if (_productOrders.FirstOrDefault(pO => pO.ProductId == product.Id && pO.OrderId == _orders.FirstOrDefault(o => o.UserId == App.currentUser.Id).Id) != null)
                    {
                        ProductOrder productOrder = _productOrders.FirstOrDefault(pO => pO.ProductId == product.Id && pO.OrderId == _orders.FirstOrDefault(o => o.UserId == App.currentUser.Id).Id);
                        Order order = _orders.FirstOrDefault(o => o.UserId == App.currentUser.Id);
                        order.TotalCost += productOrder.Product.Cost;
                        productOrder.Quantity += 1;
                        FeedbackService.Information("Количество товара в корзине увеличено");
                    }
                    else
                    {
                        try
                        {
                            ProductOrder productOrder = new ProductOrder()
                            {
                                ProductId = product.Id,
                                OrderId = _orders.FirstOrDefault(o => o.UserId == App.currentUser.Id).Id,
                                Quantity = 1,
                                TotalCost = product.Cost
                            };
                            Order order = _orders.FirstOrDefault(o => o.UserId == App.currentUser.Id);
                            App.context.ProductOrder.Add(productOrder);
                            App.context.SaveChanges();
                            FeedbackService.Information("Товар успешно добавлен в корзину");
                            order.TotalCost += productOrder.Product.Cost;
                        }
                        catch (Exception ex)
                        {
                            FeedbackService.Error(ex);
                        }
                    }
                }
                else
                {
                    try
                    {
                        Order order = new Order()
                        {
                            Date = DateTime.Now,
                            DeliveryTypeId = 1,
                            PaymentTypeId = 1,
                            Address = App.currentUser.Email,
                            UserId = App.currentUser.Id,
                            TotalCost = 0,
                        };
                        App.context.Order.Add(order);
                        App.context.SaveChanges();
                        FeedbackService.Information("Новая корзина создана");
                        ProductOrder productOrder = new ProductOrder()
                        {
                            ProductId = product.Id,
                            OrderId = order.Id,
                            Quantity = 1,
                            TotalCost = product.Cost
                        };
                        App.context.ProductOrder.Add(productOrder);
                        FeedbackService.Information("Товар успешно добавлен в корзину");
                        App.context.SaveChanges();
                        order.TotalCost += productOrder.Product.Cost;
                    }
                    catch (Exception ex)
                    {
                        FeedbackService.Error(ex);
                    }
                }
            }
            else
            {
                FeedbackService.Error("Выберите товар из списка");
            }
        }
    }
}
