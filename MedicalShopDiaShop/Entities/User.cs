using System.Collections.Generic;

namespace MedicalShopDiaShop.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public int Role { get; set; }
        public string AvatarKey { get; set; }
        public int StoreId { get; set; }
        public decimal? Salary { get; set; }

        public string FullName
        {
            get { return LastName + FirstName + MiddleName; }
        }

        // Навигационные свойства
        public Store Store { get; set; }
        public ICollection<Order> OrdersAsClient { get; set; }
        public ICollection<Order> OrdersAsWorker { get; set; }
        public ICollection<Delivery> Deliveries { get; set; }
        public ICollection<Favorite> Favorites { get; set; }
        public ICollection<Schedule> Schedules { get; set; }
        public ICollection<EmployeeTask> AssignedTasks { get; set; }   // задачи, назначенные пользователю
        public ICollection<EmployeeTask> CreatedTasks { get; set; }    // задачи, созданные пользователем
    }

    public enum Role
    {
        Admin = 1,
        Client = 2,
        Worker = 3,
        Courier = 4
    }
}
