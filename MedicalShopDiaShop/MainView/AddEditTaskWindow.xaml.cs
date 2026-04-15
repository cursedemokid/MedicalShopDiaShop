using MedicalShopDiaShop.AppData;
using MedicalShopDiaShop.Database;
using System;
using System.Linq;
using System.Windows;
using static MedicalShopDiaShop.AppData.Status;

namespace MedicalShopDiaShop.MainView
{
    public partial class AddEditTaskWindow : Window
    {
        private readonly int _userId;
        private readonly int? _taskId;
        private Task _editingTask;

        public AddEditTaskWindow(int userId, int? taskId = null)
        {
            InitializeComponent();
            _userId = userId;
            _taskId = taskId;
            LoadUsers();
            if (taskId.HasValue)
                LoadTaskData();
            else
            {
                TitleText.Text = "Новая задача";
                StartDatePicker.SelectedDate = DateTime.Now;
                DeadlinePicker.SelectedDate = DateTime.Now.AddDays(1);
            }
        }

        private void LoadUsers()
        {
            var user = App.context.User.Find(_userId);
            if (user != null)
            {
                UserCmb.ItemsSource = new[] { new { Id = user.Id, FullName = $"{user.LastName} {user.FirstName}" } };
                UserCmb.SelectedValue = user.Id;
            }
        }

        private void LoadTaskData()
        {
            _editingTask = App.context.Task.Find(_taskId);
            if (_editingTask == null)
            {
                FeedbackService.Error("Задача не найдена.");
                Close();
                return;
            }
            TitleText.Text = "Редактирование задачи";
            DescriptionTb.Text = _editingTask.Description;
            StartDatePicker.SelectedDate = _editingTask.StartAt;
            DeadlinePicker.SelectedDate = _editingTask.Deadline;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DescriptionTb.Text) || UserCmb.SelectedValue == null ||
                StartDatePicker.SelectedDate == null)
            {
                FeedbackService.Error("Заполните все поля.");
                return;
            }

            DateTime startDate = ToSqlDateTime(StartDatePicker.SelectedDate.Value);
            DateTime? deadline = DeadlinePicker.SelectedDate.HasValue
                ? ToSqlDateTime(DeadlinePicker.SelectedDate.Value)
                : (DateTime?)null;

            if (_editingTask == null)
            {
                var task = new Task
                {
                    Description = DescriptionTb.Text.Trim(),
                    UserId = (int)UserCmb.SelectedValue,
                    StartAt = startDate,
                    EndAt = null, // Фактическое окончание пока не известно
                    Deadline = deadline,
                    AuthorId = App.currentUser.Id,
                    IsCompleted = false
                };
                App.context.Task.Add(task);
                App.context.SaveChanges();

                NotificationHelper.CreateNotification(task.UserId,
                    $"Новая задача: {task.Description}. Дедлайн: {(task.Deadline.HasValue ? task.Deadline.Value.ToString("dd.MM.yyyy") : "не указан")}",
                    taskId: task.Id);
            }
            else
            {
                _editingTask.Description = DescriptionTb.Text.Trim();
                _editingTask.StartAt = startDate;
                _editingTask.Deadline = deadline;
                // EndAt не трогаем при редактировании
                App.context.SaveChanges();
            }

            DialogResult = true;
            Close();
        }

        private DateTime ToSqlDateTime(DateTime date)
        {
            if (date < new DateTime(1753, 1, 1))
                return new DateTime(1753, 1, 1);
            if (date > new DateTime(9999, 12, 31))
                return new DateTime(9999, 12, 31);
            return date;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}