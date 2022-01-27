using System.Windows;
using System.Windows.Input;

namespace E3.ReactorManager.Interfaces.Helpers
{
    public class UiHelpers
    {
        public static ICommand GetWindowLoadedCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(WindowLoadedCommandProperty);
        }

        public static void SetWindowLoadedCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(WindowLoadedCommandProperty, value);
        }

        public static readonly DependencyProperty WindowLoadedCommandProperty =
            DependencyProperty.RegisterAttached("WindowLoadedCommand", typeof(ICommand), typeof(UiHelpers), new PropertyMetadata(WindowLoadedCommandPropertyChangedCallback));

        private static void WindowLoadedCommandPropertyChangedCallback(DependencyObject uiElement, DependencyPropertyChangedEventArgs e)
        {
            if (!(uiElement is UIElement))
            {
                return;
            }

            (uiElement as FrameworkElement).Loaded += (sender, args) => GetWindowLoadedCommand(uiElement).Execute(args);
        }
    }
}
