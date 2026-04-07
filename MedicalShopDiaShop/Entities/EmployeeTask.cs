using System;

namespace MedicalShopDiaShop.Entities
{
    public class EmployeeTask
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public int UserId { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public int AuthorId { get; set; }

        public User User { get; set; }       // исполнитель
        public User Author { get; set; }     // автор задачи
    }
}
