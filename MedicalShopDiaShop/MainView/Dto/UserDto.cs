namespace MedicalShopDiaShop.MainView.Dto
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public int Role { get; set; }
        public string AvatarKey { get; set; }

        public bool IsSelected { get; set; }

        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

        public string RoleName
        {
            get
            {
                switch (Role)
                {
                    case 1: return "Администратор";
                    case 2: return "Клиент";
                    case 3: return "Сотрудник";
                    case 4: return "Курьер";
                    default: return "Неизвестно";
                }
            }
        }
    }
}