using LiveCharts.Wpf;
using System;
using System.Security.Authentication.ExtendedProtection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace E3.TrendsManager.Helpers
{
    public class UiHelpers
    {
        #region CheckIfTextContainsOnlyNumber
        public static object GetCheckIfTextContainsOnlyNumber(DependencyObject obj)
        {
            return obj.GetValue(CheckIfTextContainsOnlyNumberProperty);
        }

        public static void SetCheckIfTextContainsOnlyNumber(DependencyObject obj, object value)
        {
            obj.SetValue(CheckIfTextContainsOnlyNumberProperty, value);
        }

        public static readonly DependencyProperty CheckIfTextContainsOnlyNumberProperty =
            DependencyProperty.RegisterAttached("CheckIfTextContainsOnlyNumber", typeof(object), typeof(UiHelpers), new PropertyMetadata(PropChangedCallback));

        private static void PropChangedCallback(DependencyObject uiElement, DependencyPropertyChangedEventArgs e)
        {
            (uiElement as TextBox).PreviewTextInput += (sender, args) => {
                TextBox textBox = sender as TextBox;
                Regex regex = new Regex("[^0-9]+");
                args.Handled = regex.IsMatch(args.Text);
            };
        }
        #endregion

        #region Clicked Command
        public static ICommand GetClicked(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(ClickedProperty);
        }

        public static void SetClicked(DependencyObject obj, ICommand value)
        {
            obj.SetValue(ClickedProperty, value);
        }

        // Using a DependencyProperty as the backing store for Clicked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClickedProperty =
            DependencyProperty.RegisterAttached("Clicked", typeof(ICommand), typeof(UiHelpers), new PropertyMetadata(ClickedCommandChanged));

        private static void ClickedCommandChanged(DependencyObject uiElement, DependencyPropertyChangedEventArgs e)
        {
            if (!(uiElement is UIElement))
            {
                return;
            }

            (uiElement as Button).Click += (sender, args) =>
            {
                CartesianChart cartesianChart = (CartesianChart)(sender as Button).CommandParameter;
                GetClicked(uiElement).Execute(new Axis[] { cartesianChart.AxisX[0], cartesianChart.AxisY[0] });
            };
        }
        #endregion
    }
}
