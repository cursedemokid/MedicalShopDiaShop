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

        private static void OnScheduledDatesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Calendar calendar)
            {
                calendar.Loaded -= Calendar_Loaded;
                calendar.DisplayDateChanged -= Calendar_DisplayDateChanged;
                calendar.Loaded += Calendar_Loaded;
                calendar.DisplayDateChanged += Calendar_DisplayDateChanged;

                if (calendar.IsLoaded)
                    ApplyHighlight(calendar);
            }
        }

        private static void OnToolTipSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Calendar calendar && calendar.IsLoaded)
                ApplyHighlight(calendar);
        }

        private static void Calendar_Loaded(object sender, RoutedEventArgs e)
            => ApplyHighlight(sender as Calendar);

        private static void Calendar_DisplayDateChanged(object sender, CalendarDateChangedEventArgs e)
        {
            if (sender is Calendar calendar)
                calendar.Dispatcher.BeginInvoke(new Action(() => ApplyHighlight(calendar)),
                    DispatcherPriority.Background);
        }

        private static void ApplyHighlight(Calendar calendar)
        {
            if (calendar == null) return;

            var scheduledDates = GetScheduledDates(calendar)?.Select(d => d.Date).ToHashSet()
                                 ?? new HashSet<DateTime>();
            var toolTipSelector = GetDateToolTipSelector(calendar);

            foreach (var button in FindVisualChildren<CalendarDayButton>(calendar))
            {
                if (button.DataContext is DateTime date)
                {
                    bool isScheduled = scheduledDates.Contains(date.Date);

                    // Используем акцентный цвет MaterialDesign (PrimaryHueLightBrush) с прозрачностью
                    var accentBrush = Application.Current.TryFindResource("PrimaryHueLightBrush") as Brush;
                    if (accentBrush is SolidColorBrush solidBrush)
                    {
                        button.Background = isScheduled
                            ? new SolidColorBrush(solidBrush.Color) { Opacity = 0.3 }
                            : Brushes.Transparent;
                    }
                    else
                    {
                        button.Background = isScheduled
                            ? new SolidColorBrush(Color.FromRgb(255, 235, 200))
                            : Brushes.Transparent;
                    }

                    button.FontWeight = isScheduled ? FontWeights.SemiBold : FontWeights.Normal;
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