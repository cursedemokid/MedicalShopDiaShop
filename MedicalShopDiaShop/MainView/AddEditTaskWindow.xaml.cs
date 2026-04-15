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

            if (_editingTask == null)
            {
                var task = new Task
                {
                    Description = DescriptionTb.Text.Trim(),
                    UserId = (int)UserCmb.SelectedValue,
                    StartAt = StartDatePicker.SelectedDate.Value,
                    EndAt = StartDatePicker.SelectedDate.Value, // Добавлено: устанавливаем начальное значение EndAt
                    Deadline = DeadlinePicker.SelectedDate,
                    AuthorId = App.currentUser.Id,
                    IsCompleted = false
                };
                App.context.Task.Add(task);
                App.context.SaveChanges();

                NotificationHelper.CreateNotification(task.UserId,
                    $"Новая задача: {task.Description}. Дедлайн: {task.Deadline:dd.MM.yyyy}",
                    taskId: task.Id);
            }
            else
            {
                _editingTask.Description = DescriptionTb.Text.Trim();
                _editingTask.StartAt = StartDatePicker.SelectedDate.Value;
                _editingTask.Deadline = DeadlinePicker.SelectedDate;
                App.context.SaveChanges();
            }

            DialogResult = true;
            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}