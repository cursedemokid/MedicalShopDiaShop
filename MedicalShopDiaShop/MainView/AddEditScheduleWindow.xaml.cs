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
                StartDatePicker.SelectedDate = DateTime.Now.Date;
                StartTimePicker.SelectedTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 9, 0, 0);
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
            StartDatePicker.SelectedDate = _editingSchedule.DateStart.Date;
            StartTimePicker.SelectedTime = _editingSchedule.DateStart;
            HoursTb.Text = _editingSchedule.Hours.ToString();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (StartDatePicker.SelectedDate == null || StartTimePicker.SelectedTime == null)
            {
                FeedbackService.Error("Выберите дату и время начала.");
                return;
            }
            if (!int.TryParse(HoursTb.Text, out int hours) || hours <= 0)
            {
                FeedbackService.Error("Введите корректное количество часов (целое положительное число).");
                return;
            }

            // Объединяем дату и время
            DateTime selectedDate = StartDatePicker.SelectedDate.Value;
            DateTime selectedTime = StartTimePicker.SelectedTime.Value;
            DateTime startDateTime = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day,
                                                  selectedTime.Hour, selectedTime.Minute, 0);

            if (_editingSchedule == null)
            {
                var schedule = new Schedule
                {
                    DateStart = startDateTime,
                    UserId = _userId,
                    Hours = hours,
                    FactStartAt = startDateTime,
                    FactExitAt = startDateTime.AddHours(hours),
                    FactHours = 0
                };
                App.context.Schedule.Add(schedule);
                App.context.SaveChanges();

                string startTimeStr = startDateTime.ToString("dd.MM.yyyy HH:mm");
                NotificationHelper.CreateNotification(_userId,
                    $"Новая смена: {startTimeStr}, продолжительность {hours} ч.");

                (Application.Current.MainWindow as MainWindow)?.UpdateNotificationBadge();
            }
            else
            {
                _editingSchedule.DateStart = startDateTime;
                _editingSchedule.Hours = hours;
                _editingSchedule.FactStartAt = startDateTime;
                _editingSchedule.FactExitAt = startDateTime.AddHours(hours);
                App.context.SaveChanges();
            }

            DialogResult = true;
            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}