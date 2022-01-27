using E3.ReactorManager.Interfaces.UI.UserControls;
using E3.UserManager.Model.Data;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Anathem.Ui.Helpers
{
    public class UiHelpers
    {
        #region AllowedForUser
        public static object GetAllowedForUser(DependencyObject obj)
        {
            return obj.GetValue(AllowedForUserProperty);
        }

        public static void SetAllowedForUser(DependencyObject obj, object value)
        {
            obj.SetValue(AllowedForUserProperty, value);
        }

        public static readonly DependencyProperty AllowedForUserProperty =
            DependencyProperty.RegisterAttached("AllowedForUser", typeof(object), typeof(UiHelpers), new PropertyMetadata(AllowedForUserChangedCallBack));

        private static void AllowedForUserChangedCallBack(DependencyObject uiElement, DependencyPropertyChangedEventArgs e)
        {
            User loggedInUser = (User)e.NewValue;
            Type type = uiElement.GetType();
            string moduleToBeValidated = type.GetProperty("Tag").GetValue(uiElement).ToString();
            bool validationResult = loggedInUser.Roles.Any(role => role.ModulesAccessable.Contains(moduleToBeValidated));
            if (type == typeof(Button))
            {
                type.GetProperty("Visibility").SetValue(uiElement, validationResult ? Visibility.Visible : Visibility.Collapsed);
            }
            else if (type == typeof(TextBox))
            {
                type.GetProperty("IsReadOnly").SetValue(uiElement, !validationResult);
            }
        }
        #endregion

        #region Send Command To Device
        public static ICommand GetCommandToDevice(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(CommandToDeviceProperty);
        }

        public static void SetCommandToDevice(DependencyObject obj, ICommand value)
        {
            obj.SetValue(CommandToDeviceProperty, value);
        }

        public static readonly DependencyProperty CommandToDeviceProperty =
            DependencyProperty.RegisterAttached("CommandToDevice", typeof(ICommand), typeof(UiHelpers), new PropertyMetadata(SendCommandToDevice));

        private static void SendCommandToDevice(DependencyObject uiElement, DependencyPropertyChangedEventArgs e)
        {
            if (!(uiElement is UIElement))
            {
                return;
            }
            if (uiElement.GetType() == typeof(ButtonOnOffAnimation))
            {
                ButtonOnOffAnimation ba = uiElement as ButtonOnOffAnimation;
                if (!ba.IsLoaded)
                {
                    ba.Loaded += (sender, args) => {
                        ButtonOnOffAnimation button = sender as ButtonOnOffAnimation;
                        button.SetBinding(ButtonOnOffAnimation.StatusProperty, new Binding("ParameterDictionary")
                        {
                            Source = button.DataContext,
                            Converter = new ParameterExtractorConverter(),
                            ConverterParameter = button.Tag.ToString()
                        });
                    };
                }

                ba.MouseLeftButtonDown += (sender, args) => {
                    ButtonOnOffAnimation button = sender as ButtonOnOffAnimation;
                    GetCommandToDevice(button).Execute($"{button.Tag}|bool|{!Convert.ToBoolean(ButtonOnOffAnimation.GetStatus(button) ?? bool.FalseString)}");
                };
            }
        }
        #endregion
    }
}
