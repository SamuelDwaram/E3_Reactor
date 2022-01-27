using System;
using System.Windows;
using System.Windows.Controls;

namespace E3.AuditTrailManager.Helpers
{
    public class UiHelpers
    {
        #region TimeStamp to String
        public static DateTime GetTimeStamp(DependencyObject obj)
        {
            return (DateTime)obj.GetValue(TimeStampProperty);
        }
        public static void SetTimeStamp(DependencyObject obj, DateTime value)
        {
            obj.SetValue(TimeStampProperty, value);
        }

        // Using a DependencyProperty as the backing store for TimeStamp.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeStampProperty =
            DependencyProperty.RegisterAttached("TimeStamp", typeof(DateTime), typeof(UiHelpers), new PropertyMetadata(TimeStampChanged));

        private static void TimeStampChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ContentControl))
            {
                return;
            }

            ContentControl element = d as ContentControl;
            DateTime dateTime = (DateTime)e.NewValue;
            element.SetValue(ContentControl.ContentProperty, dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }
        #endregion
    }
}
