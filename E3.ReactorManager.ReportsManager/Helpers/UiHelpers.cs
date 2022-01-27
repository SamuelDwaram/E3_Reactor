using System;
using System.Windows;
using System.Windows.Controls;

namespace E3.ReactorManager.ReportsManager.Helpers
{
    public class UiHelpers
    {
        public static string GetReportSource(DependencyObject obj)
        {
            return (string)obj.GetValue(ReportSourceProperty);
        }

        public static void SetReportSource(DependencyObject obj, string value)
        {
            obj.SetValue(ReportSourceProperty, value);
        }

        // Using a DependencyProperty as the backing store for ReportSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReportSourceProperty =
            DependencyProperty.RegisterAttached("ReportSource", typeof(string), typeof(UiHelpers), new PropertyMetadata(ReportSourceChangedCallback));

        private static void ReportSourceChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string fileName = GetReportSource(d);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                (d as UIElement).Visibility = Visibility.Hidden;
                return;
            }
            WebBrowser webBrowser = d as WebBrowser;
            webBrowser.Navigate(new Uri(GetReportSource(d)));
            webBrowser.Visibility = Visibility.Visible;
        }
    }
}
