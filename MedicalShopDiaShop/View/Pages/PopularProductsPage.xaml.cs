using MedicalShopDiaShop.AppData;
using MedicalShopDiaShop.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static MedicalShopDiaShop.AppData.Status;

namespace MedicalShopDiaShop.View.Pages
{

    /// <summary>
    /// Логика взаимодействия для PopularProductsPage.xaml
    /// </summary>
    public partial class PopularProductsPage : Page
    {
        //List<ProductOrder> _productOrders = App.context.ProductOrder.ToList();
        //List<Order> _orders = App.context.Order.ToList();
        //List<User> _users = App.context.User.ToList();
        public PopularProductsPage()
        {
            InitializeComponent();
            //LoadProducts();
        }

        //public PopularProductsPage(int id)
        //{
        //    InitializeComponent();
        //    var products = App.context.Product.Where(x => x.Id == id).ToList();
        //    ProductLb.ItemsSource = GetProductsWithFavoriteStatus(products);
        //}

        //public PopularProductsPage(PageType type)
        //{
        //    InitializeComponent();
        //    DateTime targetDate = DateTime.Now.AddDays(30);
        //    List<Product> products = new List<Product>();

        //    if (type == PageType.New)
        //    {
        //        products = App.context.Product.Where(x => x.AddDate <= targetDate).ToList();
        //    }
        //    else if (type == PageType.LowCarbohydrates)
        //    {
        //        products = App.context.Product.Where(x => x.CategoryId == 5).ToList();
        //    }
        //    else if (type == PageType.Popular)
        //    {
        //        var popularProducts = App.context.ProductOrder
        //            .GroupBy(po => po.ProductId)
        //            .Select(g => new
        //            {
        //                ProductId = g.Key,
        //                Count = g.Count()
        //            })
        //            .OrderByDescending(x => x.Count)
        //            .Take(10)
        //            .Join(App.context.Product,
        //                  popular => popular.ProductId,
        //                  product => product.Id,
        //                  (popular, product) => product)
        //            .ToList();

        //        products = popularProducts;
        //    }
        //    else if (type == PageType.WithoutGluten)
        //    {
        //        products = App.context.Product.Where(x => x.CategoryId == 8).ToList();
        //    }
        //    else if(type == PageType.Favorite)
        //    {
        //        products = App.context.Favorites
        //            .Where(f => f.UserId == App.currentUser.Id)
        //            .Select(f => f.Product)
        //            .ToList();
        //    }

        //    ProductLb.ItemsSource = GetProductsWithFavoriteStatus(products);
        //}

        //private void LoadProducts()
        //{
        //    var products = App.context.Product.ToList();
        //    ProductLb.ItemsSource = GetProductsWithFavoriteStatus(products);
        //}

        //private List<ProductWithFavorite> GetProductsWithFavoriteStatus(List<Product> products)
        //{
        //    var result = new List<ProductWithFavorite>();

        //    foreach (var product in products)
        //    {
        //        bool isFavorite = App.context.Favorites
        //            .Any(f => f.ProductId == product.Id && f.UserId == App.currentUser.Id);

        //        result.Add(new ProductWithFavorite
        //        {
        //            Product = product,
        //            IsFavorite = isFavorite
        //        });
        //    }

        //    return result;
        //}


        private void AddToBasketBtn_Click(object sender, RoutedEventArgs e)
        {
            //    Button button = sender as Button;

            //    ProductWithFavorite productWithFavorite = button.DataContext as ProductWithFavorite;

            //    if (productWithFavorite != null)
            //    {
            //        Product product = productWithFavorite.Product;
            //        if (product != null)
            //        {
            //            if (_orders.FirstOrDefault(o => o.UserId == App.currentUser.Id && 
            //                o.Status == (int)OrderStatus.MakingOrder) != null)
            //            {
            //                if (_productOrders
            //                    .FirstOrDefault(pO => pO.OrderId == _orders
            //                    .FirstOrDefault(o => o.UserId == App.currentUser.Id).Id &&
            //                    pO.ProductId == product.Id) != null)
            //                {
            //                    ProductOrder productOrder = _productOrders
            //                        .FirstOrDefault(pO => pO.ProductId == product.Id && pO.OrderId == _orders
            //                        .FirstOrDefault(o => o.UserId == App.currentUser.Id).Id);
            //                    Order order = _orders.FirstOrDefault(o => o.UserId == App.currentUser.Id && o.Status == (int)OrderStatus.MakingOrder);
            //                    order.TotalCost += productOrder.Product.Cost;
            //                    productOrder.TotalCost += productOrder.Product.Cost;
            //                    productOrder.Quantity += 1;
            //                    FeedbackService.Information("Количество товара в корзине увеличено");
            //                    App.context.SaveChanges();
            //                    _orders = App.context.Order.ToList();
            //                    _productOrders = App.context.ProductOrder.ToList();
            //                }
            //                else
            //                {
            //                    try
            //                    {
            //                        ProductOrder productOrder = new ProductOrder()
            //                        {
            //                            ProductId = product.Id,
            //                            OrderId = _orders
            //                            .FirstOrDefault(o => o.UserId == App.currentUser.Id && o.Status == (int)OrderStatus.MakingOrder).Id,
            //                            Quantity = 1,
            //                            TotalCost = product.Cost
            //                        };
            //                        Order order = _orders
            //                            .FirstOrDefault(o => o.UserId == App.currentUser.Id &&
            //                            o.Status == (int)OrderStatus.MakingOrder);
            //                        App.context.ProductOrder.Add(productOrder);
            //                        App.context.SaveChanges();
            //                        FeedbackService.Information("Товар успешно добавлен в корзину");
            //                        order.TotalCost += productOrder.Product.Cost;
            //                        _orders = App.context.Order.ToList();
            //                        _productOrders = App.context.ProductOrder.ToList();
            //                    }
            //                    catch (Exception ex)
            //                    {
            //                        FeedbackService.Error(ex);
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                try
            //                {
            //                    Order order = new Order()
            //                    {
            //                        Date = DateTime.Now,
            //                        DeliveryTypeId = 1,
            //                        PaymentTypeId = 1,
            //                        Address = App.currentUser.Email,
            //                        UserId = App.currentUser.Id,
            //                        TotalCost = 0,
            //                        Status = (int)OrderStatus.MakingOrder
            //                    };
            //                    App.context.Order.Add(order);
            //                    App.context.SaveChanges();
            //                    FeedbackService.Information("Новая корзина создана");
            //                    ProductOrder productOrder = new ProductOrder()
            //                    {
            //                        ProductId = product.Id,
            //                        OrderId = order.Id,
            //                        Quantity = 1,
            //                        TotalCost = product.Cost
            //                    };
            //                    App.context.ProductOrder.Add(productOrder);
            //                    FeedbackService.Information("Товар успешно добавлен в корзину");
            //                    App.context.SaveChanges();
            //                    order.TotalCost += productOrder.Product.Cost;
            //                    _orders = App.context.Order.ToList();
            //                    _productOrders = App.context.ProductOrder.ToList();
            //                }
            //                catch (Exception ex)
            //                {
            //                    FeedbackService.Error(ex);
            //                }
            //            }
            //        }
            //        else
            //        {
            //            FeedbackService.Error("Выберите товар из списка");
            //        }
            //    }
        }

        private void IsFavorite_Click(object sender, RoutedEventArgs e)
        {
            //    ProductWithFavorite productWithFavorite = ProductLb.SelectedItem as ProductWithFavorite;

            //    if (productWithFavorite != null)
            //    {
            //        Product product = productWithFavorite.Product;

            //        var favorite = App.context.Favorites
            //            .FirstOrDefault(f => f.UserId == App.currentUser.Id && f.ProductId == product.Id);

            //        if (favorite != null)
            //        {
            //            App.context.Favorites.Remove(favorite);
            //            productWithFavorite.IsFavorite = false;
            //            FeedbackService.Information("Товар удален из избранного");
            //        }
            //        else
            //        {
            //            App.context.Favorites.Add(new Favorites
            //            {
            //                Id = App.context.Favorites.ToList().Count + 1,
            //                UserId = App.currentUser.Id,
            //                ProductId = product.Id,
            //            });
            //            productWithFavorite.IsFavorite = true;
            //            FeedbackService.Information("Товар добавлен в избранное");
            //        }

            //        App.context.SaveChanges();

            //        ProductLb.Items.Refresh();
            //    }
            //    else
            //    {
            //        FeedbackService.Error("Вы не выбрали товар!");
            //    }
        }
    }

}
