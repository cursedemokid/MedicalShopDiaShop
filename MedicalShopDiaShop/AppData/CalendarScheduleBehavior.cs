using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;

namespace MedicalShopDiaShop.AppData
{
    public static class CalendarScheduleBehavior
    {
        private const double ScheduledHighlightOpacity = 0.35;

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

        private static Color GetPrimaryColor()
        {
            if (Application.Current?.TryFindResource("AppPrimaryBrush") is SolidColorBrush brush)
                return brush.Color;
            return Color.FromRgb(103, 58, 183);
        }

        private static Color GetCalendarSurfaceColor(Calendar calendar)
        {
            if (calendar.Background is SolidColorBrush calendarBackground)
                return calendarBackground.Color;
            return Colors.White;
        }

        private static Color BlendColors(Color foreground, double foregroundOpacity, Color background)
        {
            double a = foregroundOpacity;
            return Color.FromRgb(
                (byte)(foreground.R * a + background.R * (1 - a)),
                (byte)(foreground.G * a + background.G * (1 - a)),
                (byte)(foreground.B * a + background.B * (1 - a)));
        }

        private static double GetColorChannelLuminance(byte channel)
        {
            double value = channel / 255.0;
            return value <= 0.03928 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
        }

        private static double GetRelativeLuminance(Color color) =>
            0.2126 * GetColorChannelLuminance(color.R)
            + 0.7152 * GetColorChannelLuminance(color.G)
            + 0.0722 * GetColorChannelLuminance(color.B);

        private static bool IsDarkColor(Color color) => GetRelativeLuminance(color) < 0.5;

        private static Brush GetContrastBrush(Color backgroundColor) =>
            IsDarkColor(backgroundColor) ? Brushes.White : Brushes.Black;

        private static void ApplySelectionAssist(CalendarDayButton button, Color selectionColor)
        {
            var selectionBrush = new SolidColorBrush(selectionColor);
            selectionBrush.Freeze();
            CalendarAssist.SetSelectionColor(button, selectionBrush);
            CalendarAssist.SetSelectionForegroundColor(button,
                GetContrastBrush(selectionColor));
        }

        private static void ClearDayButtonHighlight(CalendarDayButton button)
        {
            button.ClearValue(Control.BackgroundProperty);
            button.ClearValue(Control.ForegroundProperty);
            button.ClearValue(CalendarAssist.SelectionColorProperty);
            button.ClearValue(CalendarAssist.SelectionForegroundColorProperty);
        }

        private static void ApplyHighlight(Calendar calendar)
        {
            if (calendar == null) return;

            var scheduledDates = GetScheduledDates(calendar)?.Select(d => d.Date).ToHashSet()
                                 ?? new HashSet<DateTime>();
            var toolTipSelector = GetDateToolTipSelector(calendar);
            var primaryColor = GetPrimaryColor();
            var surfaceColor = GetCalendarSurfaceColor(calendar);

            foreach (var button in FindVisualChildren<CalendarDayButton>(calendar))
            {
                if (button.DataContext is DateTime date)
                {
                    bool isScheduled = scheduledDates.Contains(date.Date);
                    bool isSelected = button.IsSelected;

                    ClearDayButtonHighlight(button);

                    if (isSelected)
                    {
                        // Выбранная дата: MaterialDesign + контрастный текст к цвету выделения
                        ApplySelectionAssist(button, primaryColor);
                    }
                    else if (isScheduled)
                    {
                        // День со сменой: подсветка с учётом смешения с фоном календаря
                        var effectiveBackground = BlendColors(primaryColor, ScheduledHighlightOpacity, surfaceColor);
                        button.Background = new SolidColorBrush(primaryColor) { Opacity = ScheduledHighlightOpacity };
                        button.Foreground = GetContrastBrush(effectiveBackground);
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
