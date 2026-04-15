using MedicalShopDiaShop.AppData;
using MedicalShopDiaShop.Database;
using System;
using System.Windows;

namespace MedicalShopDiaShop.MainView
{
    public partial class AddEditScheduleWindow : Window
    {
        private readonly int _userId;
        private readonly int? _scheduleId;
        private Schedule _editingSchedule;

        public AddEditScheduleWindow(int userId, int? scheduleId = null)
        {
            InitializeComponent();
            _userId = userId;
            _scheduleId = scheduleId;
            if (scheduleId.HasValue)
                LoadScheduleData();
            else
            {
                TitleText.Text = "Новая смена";
                StartDatePicker.SelectedDate = DateTime.Now;
            }
        }

        private void LoadScheduleData()
        {
            _editingSchedule = App.context.Schedule.Find(_scheduleId);
            if (_editingSchedule == null)
            {
                FeedbackService.Error("Смена не найдена.");
                Close();
                return;
            }
            TitleText.Text = "Редактирование смены";
            StartDatePicker.SelectedDate = _editingSchedule.DateStart;
            HoursTb.Text = _editingSchedule.Hours.ToString();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (StartDatePicker.SelectedDate == null)
            {
                FeedbackService.Error("Выберите дату начала.");
                return;
            }
            if (!int.TryParse(HoursTb.Text, out int hours) || hours <= 0)
            {
                FeedbackService.Error("Введите корректное количество часов (целое положительное число).");
                return;
            }

            if (_editingSchedule == null)
            {
                // Новая смена
                var schedule = new Schedule
                {
                    DateStart = StartDatePicker.SelectedDate.Value,
                    UserId = _userId,
                    Hours = hours,
                    FactStartAt = StartDatePicker.SelectedDate.Value,
                    FactExitAt = StartDatePicker.SelectedDate.Value.AddHours(hours),
                    FactHours = 0
                };
                App.context.Schedule.Add(schedule);
                App.context.SaveChanges();

                // Отправка уведомления сотруднику
                string startTime = schedule.DateStart.ToString("dd.MM.yyyy HH:mm");
                NotificationHelper.CreateNotification(_userId,
                    $"Новая смена: {startTime}, продолжительность {hours} ч.");

                // Обновляем бейдж уведомлений в главном окне
                (Application.Current.MainWindow as MainWindow)?.UpdateNotificationBadge();
            }
            else
            {
                // Редактирование существующей смены
                _editingSchedule.DateStart = StartDatePicker.SelectedDate.Value;
                _editingSchedule.Hours = hours;
                _editingSchedule.FactStartAt = StartDatePicker.SelectedDate.Value;
                _editingSchedule.FactExitAt = StartDatePicker.SelectedDate.Value.AddHours(hours);
                App.context.SaveChanges();
                // При редактировании уведомление не отправляем (можно добавить при необходимости)
            }

            DialogResult = true;
            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}