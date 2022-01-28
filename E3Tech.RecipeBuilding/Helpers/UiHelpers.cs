using E3.ReactorManager.DesignExperiment.Model.Data;
using E3.ReactorManager.DesignExperiment.ViewModels;
using E3Tech.RecipeBuilding.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace E3Tech.RecipeBuilding.Helpers
{
    public class UiHelpers
    {
        #region Password
        public static object GetPassword(DependencyObject obj)
        {
            return obj.GetValue(PasswordProperty);
        }

        public static void SetPassword(DependencyObject obj, object value)
        {
            obj.SetValue(PasswordProperty, value);
        }

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.RegisterAttached("Password", typeof(object), typeof(UiHelpers), new PropertyMetadata(PasswordChangedCallBack));

        private static void PasswordChangedCallBack(DependencyObject uiElement, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox passwordBox = uiElement as PasswordBox;
            passwordBox.PasswordChanged += (sender, args) => {
                RecipeBuilderViewModel dataContext = (RecipeBuilderViewModel)GetPassword(uiElement);
                dataContext.AdminCredential.PasswordHash = (sender as PasswordBox).Password;
            };
        }
        #endregion

        #region CommandParameter for navigation to Reactor Control
        public static Batch GetCommandParameter(DependencyObject obj)
        {
            return (Batch)obj.GetValue(CommandParameterProperty);
        }

        public static void SetCommandParameter(DependencyObject obj, Batch value)
        {
            obj.SetValue(CommandParameterProperty, value);
        }

        // Using a DependencyProperty as the backing store for CommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter", typeof(Batch), typeof(UiHelpers), new PropertyMetadata(CommandParameterChanged));

        private static void CommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Button))
            {
                return;
            }
            else
            {
                (d as Button).Loaded += (sender, args) => {
                    Button button = sender as Button;
                    Batch dataContext = GetCommandParameter(button);
                    button.CommandParameter = $"Reactor|{dataContext.FieldDeviceIdentifier}|{dataContext.FieldDeviceLabel}";
                };
            }
        }
        #endregion
    }
}
