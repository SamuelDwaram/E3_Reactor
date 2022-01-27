using LiveCharts.Wpf;
using System.Windows;
using System.Windows.Input;

namespace E3.TrendsManager.Helpers
{
    public class MouseMoveBehaviour
    {
        #region The dependecy Property
        /// <summary>
        /// The Dependency property. To allow for Binding, a dependency
        /// property must be used.
        /// </summary>
        public static readonly DependencyProperty MouseMoveCommandProperty =
                    DependencyProperty.RegisterAttached
                    (
                        "MouseMoveCommand",
                        typeof(ICommand),
                        typeof(MouseMoveBehaviour),
                        new PropertyMetadata(MouseMoveCommandPropertyChangedCallBack)
                    );
        #endregion

        #region The getter and setter
        /// <summary>
        /// The setter. This sets the value of the PreviewMouseMoveCommandProperty
        /// Dependency Property. It is expected that you use this only in XAML
        ///
        /// This appears in XAML with the "Set" stripped off.
        /// XAML usage:
        ///
        /// <Grid helpers:MouseMoveBehavior.MouseMoveCommand="{Binding MouseMoveCommand}" />
        ///
        /// </summary>
        /// <param name="inUIElement">A UIElement object. In XAML this is automatically passed
        /// in, so you don't have to enter anything in XAML.</param>
        /// <param name="inCommand">An object that implements ICommand.</param>
        public static void SetMouseMoveCommand(CartesianChart inUIElement, ICommand inCommand)
        {
            inUIElement.SetValue(MouseMoveCommandProperty, inCommand);
        }

        /// <summary>
        /// Gets the PreviewMouseMoveCommand assigned to the PreviewMouseMoveCommandProperty
        /// DependencyProperty. As this is only needed by this class, it is private.
        /// </summary>
        /// <param name="inUIElement">A UIElement object.</param>
        /// <returns>An object that implements ICommand.</returns>
        public static ICommand GetMouseMoveCommand(CartesianChart inUIElement)
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
        public static void MouseMoveCommandPropertyChangedCallBack(
            DependencyObject inDependencyObject, DependencyPropertyChangedEventArgs inEventArgs)
        {
            CartesianChart uiElement = inDependencyObject as CartesianChart;
            if (null == uiElement)
            {
                return;
            }

            uiElement.MouseMove += (sender, args) =>
            {
                CartesianChart parent = (CartesianChart)sender;
                GetMouseMoveCommand(uiElement).Execute(new MouseMoveCommandParameters()
                {
                    args = args,
                    Sender = parent,
                });
                args.Handled = true;
            };
        }
        #endregion
    }

    public class MouseMoveCommandParameters
    {
        public MouseEventArgs args { get; set; }

        public CartesianChart Sender { get; internal set; }
    }
}
