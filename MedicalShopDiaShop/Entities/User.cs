using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalShopDiaShop.Entities
{
    public class User
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public Role Role { get; set; }
        public string AvatarKey { get; set; }
        public string FullName
        {
            get => $"{LastName} {FirstName} {MiddleName}";
        }
    }

    public enum Role
    {
        Admin = 1,
        Client = 2,
        Worker = 3,
        Courier = 4
    }
}
