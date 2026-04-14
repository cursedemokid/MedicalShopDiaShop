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
        private readonly int? _employeeId;
        private readonly bool _isEditMode;
        private Database.User _editingUser;

        // Конструктор для добавления нового сотрудника
        public AddEditUserWindow()
        {
            InitializeComponent();
            _isEditMode = false;
            WindowName.Text = "Добавление пользователя";
            AddBtn.Visibility = Visibility.Visible;
            EditBtn.Visibility = Visibility.Collapsed;
        }

        // Конструктор для редактирования существующего сотрудника
        public AddEditUserWindow(int userId)
        {
            InitializeComponent();
            _employeeId = userId;
            _isEditMode = true;
            WindowName.Text = "Изменение данных пользователя";
            AddBtn.Visibility = Visibility.Collapsed;
            EditBtn.Visibility = Visibility.Visible;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRoles();
            LoadStores();

            if (_isEditMode && _employeeId.HasValue)
            {
                LoadEmployeeData(_employeeId.Value);
            }
        }

        // Заполнение комбобокса ролей из enum Role (исключаем Client, Supplier если не нужны)
        private void LoadRoles()
        {
            // Выбираем только роли, подходящие для сотрудников
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

        // Загрузка списка магазинов из БД
        private void LoadStores()
        {
            var stores = App.context.Store.ToList();
            StoreComboBox.ItemsSource = stores;
            StoreComboBox.DisplayMemberPath = "Name";
            StoreComboBox.SelectedValuePath = "Id";
        }

        // Загрузка данных сотрудника для редактирования
        private void LoadEmployeeData(int userId)
        {
            _editingUser = App.context.User.FirstOrDefault(u => u.Id == userId);
            if (_editingUser == null)
            {
                FeedbackService.Error("Сотрудник не найден.");
                Close();
                return;
            }

            LastNameTb.Text = _editingUser.LastName;
            FirstNameTb.Text = _editingUser.FirstName;
            MiddleNameTb.Text = _editingUser.MiddleName;
            PhoneNumberTb.Text = _editingUser.PhoneNumber;
            EmailTb.Text = _editingUser.Email;
            UserNameTb.Text = _editingUser.UserName;
            SalaryTb.Text = _editingUser.Salary?.ToString() ?? "";

            // Установка выбранной роли
            if (_editingUser.Role != 0)
                RoleComboBox.SelectedValue = _editingUser.Role;

            // Установка выбранного магазина
            if (_editingUser.StoreId != 0)
                StoreComboBox.SelectedValue = _editingUser.StoreId;
        }

        // Обработчик кнопки "Добавить"
        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;

            var newUser = new Database.User
            {
                LastName = LastNameTb.Text.Trim(),
                FirstName = FirstNameTb.Text.Trim(),
                MiddleName = MiddleNameTb.Text.Trim(),
                PhoneNumber = PhoneNumberTb.Text.Trim(),
                Email = EmailTb.Text.Trim(),
                UserName = UserNameTb.Text.Trim(),
                Password = "defaultPassword", // или сгенерировать / запросить отдельно
                Role = (int)RoleComboBox.SelectedValue,
                StoreId = (int)StoreComboBox.SelectedValue,
                Salary = decimal.TryParse(SalaryTb.Text, out var sal) ? sal : (decimal?)null,
                AvatarKey = null // или путь по умолчанию
            };

            App.context.User.Add(newUser);
            App.context.SaveChanges();

            FeedbackService.Information("Сотрудник успешно добавлен.");
            DialogResult = true;
            Close();
        }

        // Обработчик кнопки "Сохранить" (редактирование)
        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs() || _editingUser == null) return;

            _editingUser.LastName = LastNameTb.Text.Trim();
            _editingUser.FirstName = FirstNameTb.Text.Trim();
            _editingUser.MiddleName = MiddleNameTb.Text.Trim();
            _editingUser.PhoneNumber = PhoneNumberTb.Text.Trim();
            _editingUser.Email = EmailTb.Text.Trim();
            _editingUser.UserName = UserNameTb.Text.Trim();
            _editingUser.Role = (int)RoleComboBox.SelectedValue;
            _editingUser.StoreId = (int)StoreComboBox.SelectedValue;
            _editingUser.Salary = decimal.TryParse(SalaryTb.Text, out var sal) ? sal : (decimal?)null;

            App.context.SaveChanges();

            FeedbackService.Information("Изменения сохранены.");
            DialogResult = true;
            Close();
        }

        // Валидация обязательных полей
        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(LastNameTb.Text) ||
                string.IsNullOrWhiteSpace(FirstNameTb.Text) ||
                string.IsNullOrWhiteSpace(UserNameTb.Text) ||
                string.IsNullOrWhiteSpace(PhoneNumberTb.Text) ||
                RoleComboBox.SelectedValue == null ||
                StoreComboBox.SelectedValue == null)
            {
                FeedbackService.Error("Заполните все обязательные поля (Фамилия, Имя, Юзернейм, Телефон, Роль, Магазин).");
                return false;
            }
            return true;
        }

        // Кнопка "Отмена"
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}