using System;
using System.Linq;
using System.Windows;
using MedicalShopDiaShop.AppData;
using MedicalShopDiaShop.Database;
using static MedicalShopDiaShop.AppData.Status;

namespace MedicalShopDiaShop.MainView
{
    public partial class AddEditUserWindow : Window
    {
        private readonly int? _userId;
        private readonly bool _isEditMode;
        private readonly bool _isClientMode;
        private Database.User _editingUser;

        // Конструктор для добавления нового сотрудника
        public AddEditUserWindow() : this(false) { }

        public AddEditUserWindow(bool isClientMode)
        {
            InitializeComponent();
            _isClientMode = isClientMode;
            _isEditMode = false;
            WindowName.Text = isClientMode ? "Добавление клиента" : "Добавление сотрудника";
            AddBtn.Visibility = Visibility.Visible;
            EditBtn.Visibility = Visibility.Collapsed;

            if (isClientMode)
            {
                // Скрываем поля для сотрудника, показываем адрес
                RoleLabel.Visibility = Visibility.Collapsed;
                RoleBorder.Visibility = Visibility.Collapsed;
                SalaryLabel.Visibility = Visibility.Collapsed;
                SalaryBorder.Visibility = Visibility.Collapsed;
                StoreLabel.Visibility = Visibility.Collapsed;
                StoreBorder.Visibility = Visibility.Collapsed;
                AddressLabel.Visibility = Visibility.Visible;
                AddressBorder.Visibility = Visibility.Visible;
            }
            else
            {
                RoleLabel.Visibility = Visibility.Visible;
                RoleBorder.Visibility = Visibility.Visible;
                SalaryLabel.Visibility = Visibility.Visible;
                SalaryBorder.Visibility = Visibility.Visible;
                StoreLabel.Visibility = Visibility.Visible;
                StoreBorder.Visibility = Visibility.Visible;
                AddressLabel.Visibility = Visibility.Collapsed;
                AddressBorder.Visibility = Visibility.Collapsed;
            }
        }

        // Конструктор для редактирования существующего пользователя
        public AddEditUserWindow(int userId, bool isClientMode = false)
        {
            InitializeComponent();
            _userId = userId;
            _isClientMode = isClientMode;
            _isEditMode = true;
            WindowName.Text = isClientMode ? "Редактирование клиента" : "Редактирование сотрудника";
            AddBtn.Visibility = Visibility.Collapsed;
            EditBtn.Visibility = Visibility.Visible;

            if (isClientMode)
            {
                RoleLabel.Visibility = Visibility.Collapsed;
                RoleBorder.Visibility = Visibility.Collapsed;
                SalaryLabel.Visibility = Visibility.Collapsed;
                SalaryBorder.Visibility = Visibility.Collapsed;
                StoreLabel.Visibility = Visibility.Collapsed;
                StoreBorder.Visibility = Visibility.Collapsed;
                AddressLabel.Visibility = Visibility.Visible;
                AddressBorder.Visibility = Visibility.Visible;
            }
            else
            {
                RoleLabel.Visibility = Visibility.Visible;
                RoleBorder.Visibility = Visibility.Visible;
                SalaryLabel.Visibility = Visibility.Visible;
                SalaryBorder.Visibility = Visibility.Visible;
                StoreLabel.Visibility = Visibility.Visible;
                StoreBorder.Visibility = Visibility.Visible;
                AddressLabel.Visibility = Visibility.Collapsed;
                AddressBorder.Visibility = Visibility.Collapsed;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isClientMode)
            {
                LoadRoles();
                LoadStores();
            }

            if (_isEditMode && _userId.HasValue)
            {
                LoadUserData(_userId.Value);
            }
        }

        private void LoadRoles()
        {
            var roles = Enum.GetValues(typeof(Role))
                .Cast<Role>()
                .Select(r => new { Id = (int)r, Name = GetRoleDisplayName(r) })
                .ToList();
            RoleComboBox.ItemsSource = roles;
            RoleComboBox.DisplayMemberPath = "Name";
            RoleComboBox.SelectedValuePath = "Id";
        }

        private string GetRoleDisplayName(Role role)
        {
            switch (role)
            {
                case Role.Admin: return "Администратор";
                case Role.Worker: return "Сотрудник";
                case Role.Courier: return "Курьер";
                case Role.Supplier: return "Поставщик";
                case Role.Client: return "Клиент";
                default: return role.ToString();
            }
        }

        private void LoadStores()
        {
            var stores = App.context.Store.ToList();
            StoreComboBox.ItemsSource = stores;
            StoreComboBox.DisplayMemberPath = "Name";
            StoreComboBox.SelectedValuePath = "Id";
        }

        private void LoadUserData(int userId)
        {
            _editingUser = App.context.User.FirstOrDefault(u => u.Id == userId);
            if (_editingUser == null)
            {
                FeedbackService.Error("Пользователь не найден.");
                Close();
                return;
            }

            LastNameTb.Text = _editingUser.LastName;
            FirstNameTb.Text = _editingUser.FirstName;
            MiddleNameTb.Text = _editingUser.MiddleName;
            PhoneNumberTb.Text = _editingUser.PhoneNumber;
            EmailTb.Text = _editingUser.Email;
            UserNameTb.Text = _editingUser.UserName;

            if (_isClientMode)
            {
                AddressTb.Text = _editingUser.Address;
            }
            else
            {
                SalaryTb.Text = _editingUser.Salary?.ToString() ?? "";
                if (_editingUser.Role != 0)
                    RoleComboBox.SelectedValue = _editingUser.Role;
                if (_editingUser.StoreId != 0)
                    StoreComboBox.SelectedValue = _editingUser.StoreId;
            }
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;

            string generatedPassword = GenerateRandomPassword();
            string hashedPassword = PasswordHelper.HashPassword(generatedPassword);

            var newUser = new Database.User
            {
                LastName = LastNameTb.Text.Trim(),
                FirstName = FirstNameTb.Text.Trim(),
                MiddleName = MiddleNameTb.Text.Trim(),
                PhoneNumber = PhoneNumberTb.Text.Trim(),
                Email = EmailTb.Text.Trim(),
                UserName = UserNameTb.Text.Trim(),
                Password = hashedPassword,
                Role = _isClientMode ? (int)Role.Client : (int)RoleComboBox.SelectedValue,
                StoreId = _isClientMode ? App.currentUser.StoreId : (int)StoreComboBox.SelectedValue, // для клиента текущий магазин
                Salary = _isClientMode ? null : (decimal.TryParse(SalaryTb.Text, out var sal) ? sal : (decimal?)null),
                AvatarKey = null,
                Address = _isClientMode ? AddressTb.Text.Trim() : null,
                IsDeleted = false
            };

            App.context.User.Add(newUser);
            App.context.SaveChanges();

            FeedbackService.Information($"Пользователь успешно добавлен. Пароль: {generatedPassword}", "Регистрация");
            DialogResult = true;
            Close();
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs() || _editingUser == null) return;

            _editingUser.LastName = LastNameTb.Text.Trim();
            _editingUser.FirstName = FirstNameTb.Text.Trim();
            _editingUser.MiddleName = MiddleNameTb.Text.Trim();
            _editingUser.PhoneNumber = PhoneNumberTb.Text.Trim();
            _editingUser.Email = EmailTb.Text.Trim();
            _editingUser.UserName = UserNameTb.Text.Trim();

            if (_isClientMode)
            {
                _editingUser.Address = AddressTb.Text.Trim();
            }
            else
            {
                _editingUser.Role = (int)RoleComboBox.SelectedValue;
                _editingUser.StoreId = (int)StoreComboBox.SelectedValue;
                _editingUser.Salary = decimal.TryParse(SalaryTb.Text, out var sal) ? sal : (decimal?)null;
            }

            App.context.SaveChanges();

            FeedbackService.Information("Изменения сохранены.");
            DialogResult = true;
            Close();
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(LastNameTb.Text) ||
                string.IsNullOrWhiteSpace(FirstNameTb.Text) ||
                string.IsNullOrWhiteSpace(UserNameTb.Text) ||
                string.IsNullOrWhiteSpace(PhoneNumberTb.Text))
            {
                FeedbackService.Error("Заполните все обязательные поля (Фамилия, Имя, Юзернейм, Телефон).");
                return false;
            }

            if (!_isClientMode)
            {
                if (RoleComboBox.SelectedValue == null || StoreComboBox.SelectedValue == null)
                {
                    FeedbackService.Error("Выберите роль и магазин.");
                    return false;
                }
            }

            return true;
        }

        private string GenerateRandomPassword(int length = 8)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            var random = new Random();
            return new string(Enumerable.Repeat(valid, length)
                              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}