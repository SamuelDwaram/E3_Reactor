using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace E3Tech.RecipeBuilding.Helpers
{
    /// <summary>
    /// This is an Attached Behavior and is intended for use with
    /// XAML objects to enable binding a Mouse Move event to
    /// an ICommand.
    /// </summary>
    public class DataGridMouseMoveBehavior
    {
        #region The dependecy Property
        /// <summary>
        /// The Dependency property. To allow for Binding, a dependency
        /// property must be used.
        /// </summary>
        public static readonly DependencyProperty MouseMoveCommandProperty =
                    DependencyProperty.RegisterAttached
                    (
                        "MouseMove",
                        typeof(ICommand),
                        typeof(DataGridMouseMoveBehavior),
                        new PropertyMetadata(MouseMovePropertyChangedCallBack)
                    );
        #endregion

        #region The getter and setter
        /// <summary>
        /// The setter. This sets the value of the MouseMoveProperty
        /// Dependency Property. It is expected that you use this only in XAML
        ///
        /// This appears in XAML with the "Set" stripped off.
        /// XAML usage:
        ///
        /// <Grid helpers:DropBehavior.MouseMove="{Binding MouseMove}" />
        ///
        /// </summary>
        /// <param name="inUIElement">A UIElement object. In XAML this is automatically passed
        /// in, so you don't have to enter anything in XAML.</param>
        /// <param name="inCommand">An object that implements ICommand.</param>
        public static void SetMouseMove(UIElement inUIElement, ICommand inCommand)
        {
            inUIElement.SetValue(MouseMoveCommandProperty, inCommand);
        }

        /// <summary>
        /// Gets the MouseMove assigned to the MouseMoveProperty
        /// DependencyProperty. As this is only needed by this class, it is private.
        /// </summary>
        /// <param name="inUIElement">A UIElement object.</param>
        /// <returns>An object that implements ICommand.</returns>
        public static ICommand GetMouseMove(UIElement inUIElement)
        {
            return (ICommand)inUIElement.GetValue(MouseMoveCommandProperty);
        }
        #endregion

        #region The PropertyChangedCallBack method
        /// <summary>
        /// The OnCommandChanged method. This event handles the initial binding and future
        /// binding changes to the bound ICommand
        /// </summary>
        /// <param name="inDependencyObject">A DependencyObject</param>
        /// <param name="inEventArgs">A DependencyPropertyChangedEventArgs object.</param>
        private static void MouseMovePropertyChangedCallBack(
            DependencyObject inDependencyObject, DependencyPropertyChangedEventArgs inEventArgs)
        {
            if (!(inDependencyObject is UIElement uiElement))
            {
                return;
            }

            uiElement.MouseMove += (sender, args) =>
            {
                var dataGrid = sender as DataGrid;

                GetMouseMove(uiElement).Execute(new MouseMoveParameters()
                {
                    Sender = sender,
                    EventArgs = args
                });
                args.Handled = true;
            };
        }
        #endregion
    }
    public class MouseMoveParameters
    {
        public object Sender { get; set; }

        public EventArgs EventArgs { get; set; }
    }

    public delegate void MouseMoveDelegate(MouseMoveParameters parameters);
}
