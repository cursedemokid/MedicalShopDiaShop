using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace MedicalShopDiaShop.AppData
{
    public static class CalendarScheduleBehavior
    {
        #region ScheduledDates Property
        public static readonly DependencyProperty ScheduledDatesProperty =
            DependencyProperty.RegisterAttached(
                "ScheduledDates",
                typeof(IEnumerable<DateTime>),
                typeof(CalendarScheduleBehavior),
                new PropertyMetadata(null, OnScheduledDatesChanged));

        public static IEnumerable<DateTime> GetScheduledDates(DependencyObject obj)
            => (IEnumerable<DateTime>)obj.GetValue(ScheduledDatesProperty);

        public static void SetScheduledDates(DependencyObject obj, IEnumerable<DateTime> value)
            => obj.SetValue(ScheduledDatesProperty, value);
        #endregion

        #region DateToolTipSelector Property
        public static readonly DependencyProperty DateToolTipSelectorProperty =
            DependencyProperty.RegisterAttached(
                "DateToolTipSelector",
                typeof(Func<DateTime, object>),
                typeof(CalendarScheduleBehavior),
                new PropertyMetadata(null, OnToolTipSelectorChanged));

        public static Func<DateTime, object> GetDateToolTipSelector(DependencyObject obj)
            => (Func<DateTime, object>)obj.GetValue(DateToolTipSelectorProperty);

        public static void SetDateToolTipSelector(DependencyObject obj, Func<DateTime, object> value)
            => obj.SetValue(DateToolTipSelectorProperty, value);
        #endregion

        private static void AttachCalendarHandlers(Calendar calendar)
        {
            calendar.Loaded -= Calendar_Loaded;
            calendar.DisplayDateChanged -= Calendar_DisplayDateChanged;
            calendar.SelectedDatesChanged -= Calendar_SelectedDatesChanged;
            calendar.Loaded += Calendar_Loaded;
            calendar.DisplayDateChanged += Calendar_DisplayDateChanged;
            calendar.SelectedDatesChanged += Calendar_SelectedDatesChanged;
        }

        private static void OnScheduledDatesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Calendar calendar)
            {
                AttachCalendarHandlers(calendar);
                if (calendar.IsLoaded)
                    ApplyHighlight(calendar);
            }
        }

        private static void OnToolTipSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Calendar calendar)
            {
                AttachCalendarHandlers(calendar);
                if (calendar.IsLoaded)
                    ApplyHighlight(calendar);
            }
        }

        private static void Calendar_Loaded(object sender, RoutedEventArgs e)
            => ApplyHighlight(sender as Calendar);

        private static void Calendar_DisplayDateChanged(object sender, CalendarDateChangedEventArgs e)
        {
            if (sender is Calendar calendar)
                calendar.Dispatcher.BeginInvoke(new Action(() => ApplyHighlight(calendar)),
                    DispatcherPriority.Background);
        }

        private static void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is Calendar calendar)
                ApplyHighlight(calendar);
        }

        private static SolidColorBrush GetPrimaryBrush()
        {
            if (Application.Current?.TryFindResource("PrimaryHueMidBrush") is SolidColorBrush brush)
                return brush;
            return new SolidColorBrush(Color.FromRgb(103, 58, 183));
        }

        private static SolidColorBrush GetPrimaryDarkBrush()
        {
            if (Application.Current?.TryFindResource("PrimaryHueDarkBrush") is SolidColorBrush brush)
                return brush;
            return new SolidColorBrush(Color.FromRgb(81, 45, 168));
        }

        private static Brush GetOnPrimaryBrush()
        {
            return Application.Current?.TryFindResource("PrimaryHueMidForegroundBrush") as Brush ?? Brushes.White;
        }

        private static void ApplyHighlight(Calendar calendar)
        {
            if (calendar == null) return;

            var scheduledDates = GetScheduledDates(calendar)?.Select(d => d.Date).ToHashSet()
                                 ?? new HashSet<DateTime>();
            var toolTipSelector = GetDateToolTipSelector(calendar);
            var primaryBrush = GetPrimaryBrush();
            var primaryDarkBrush = GetPrimaryDarkBrush();
            var onPrimaryBrush = GetOnPrimaryBrush();

            foreach (var button in FindVisualChildren<CalendarDayButton>(calendar))
            {
                if (button.DataContext is DateTime date)
                {
                    bool isScheduled = scheduledDates.Contains(date.Date);
                    bool isSelected = button.IsSelected;

                    if (isScheduled || isSelected)
                    {
                        button.Background = (isScheduled && isSelected) || isSelected
                            ? primaryDarkBrush
                            : primaryBrush;
                        button.Foreground = onPrimaryBrush;
                        button.FontWeight = isScheduled ? FontWeights.SemiBold : FontWeights.Normal;
                    }
                    else
                    {
                        button.ClearValue(Control.BackgroundProperty);
                        button.ClearValue(Control.ForegroundProperty);
                        button.FontWeight = FontWeights.Normal;
                    }

                    button.ToolTip = isScheduled && toolTipSelector != null
                        ? toolTipSelector(date.Date)
                        : null;
                }
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield break;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T tChild)
                    yield return tChild;
                foreach (var childOfChild in FindVisualChildren<T>(child))
                    yield return childOfChild;
            }
        }
    }
}
