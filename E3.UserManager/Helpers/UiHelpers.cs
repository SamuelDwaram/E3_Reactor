using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace E3.UserManager.Helpers
{
    public class UiHelpers
    {
        #region SelectionChanged
        public static ICommand GetSelectionChanged(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(SelectionChangedProperty);
        }

        public static void SetSelectionChanged(DependencyObject obj, ICommand value)
        {
            obj.SetValue(SelectionChangedProperty, value);
        }

        // Using a DependencyProperty as the backing store for SelectionChanged.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectionChangedProperty =
            DependencyProperty.RegisterAttached("SelectionChanged", typeof(ICommand), typeof(UiHelpers), new PropertyMetadata(SelectionChangedCommandChanged));

        private static void SelectionChangedCommandChanged(DependencyObject uiElement, DependencyPropertyChangedEventArgs e)
        {
            if (!(uiElement is UIElement))
            {
                return;
            }

            ComboBox comboBox = uiElement as ComboBox;
            comboBox.SelectionChanged += (sender, args) => {
                ICommand subscriber = (ICommand)e.NewValue;
                subscriber.Execute((sender as ComboBox).SelectedValue);
            };
        }
        #endregion

        #region Loaded
        public static ICommand GetLoaded(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(LoadedProperty);
        }

        public static void SetLoaded(DependencyObject obj, ICommand value)
        {
            obj.SetValue(LoadedProperty, value);
        }

        // Using a DependencyProperty as the backing store for Loaded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LoadedProperty =
            DependencyProperty.RegisterAttached("Loaded", typeof(ICommand), typeof(UiHelpers), new PropertyMetadata(LoadedCommandChanged));

        private static void LoadedCommandChanged(DependencyObject uiElement, DependencyPropertyChangedEventArgs e)
        {
            if (!(uiElement is UIElement))
            {
                return;
            }

            (uiElement as FrameworkElement).Loaded += (sender, args) => {
                GetLoaded(uiElement).Execute(args);
            };
        }
        #endregion

        #region Password
        public static object GetPassword(DependencyObject obj)
        {
            return obj.GetValue(PasswordProperty);
        }

        public static void SetPassword(DependencyObject obj, object value)
        {
            obj.SetValue(PasswordProperty, value);
        }

        // Using a DependencyProperty as the backing store for Password.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.RegisterAttached("Password", typeof(object), typeof(UiHelpers), new PropertyMetadata(PasswordChanged));

        private static void PasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is PasswordBox))
            {
                return;
            }

            (d as PasswordBox).PasswordChanged += (sender, args) => {
                PasswordBox passwordBox = sender as PasswordBox;
                object baseObject = GetPassword(passwordBox);
                baseObject.GetType().GetProperty(passwordBox.Tag.ToString()).SetValue(baseObject, passwordBox.Password);
            };
        }
        #endregion
    }
}
