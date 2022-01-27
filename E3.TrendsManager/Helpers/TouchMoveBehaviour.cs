using LiveCharts.Wpf;
using System.Windows;
using System.Windows.Input;

namespace E3.TrendsManager.Helpers
{
    public class TouchMoveBehaviour
    {
        #region The dependecy Property
        /// <summary>
        /// The Dependency property. To allow for Binding, a dependency
        /// property must be used.
        /// </summary>
        public static readonly DependencyProperty TouchMoveCommandProperty =
                    DependencyProperty.RegisterAttached
                    (
                        "TouchMoveCommand",
                        typeof(ICommand),
                        typeof(TouchMoveBehaviour),
                        new PropertyMetadata(TouchMoveCommandPropertyChangedCallBack)
                    );
        #endregion

        #region The getter and setter
        /// <summary>
        /// The setter. This sets the value of the PreviewTouchMoveCommandProperty
        /// Dependency Property. It is expected that you use this only in XAML
        ///
        /// This appears in XAML with the "Set" stripped off.
        /// XAML usage:
        ///
        /// <Grid helpers:TouchMoveBehavior.TouchMoveCommand="{Binding TouchMoveCommand}" />
        ///
        /// </summary>
        /// <param name="inUIElement">A UIElement object. In XAML this is automatically passed
        /// in, so you don't have to enter anything in XAML.</param>
        /// <param name="inCommand">An object that implements ICommand.</param>
        public static void SetTouchMoveCommand(CartesianChart inUIElement, ICommand inCommand)
        {
            inUIElement.SetValue(TouchMoveCommandProperty, inCommand);
        }

        /// <summary>
        /// Gets the PreviewTouchMoveCommand assigned to the PreviewTouchMoveCommandProperty
        /// DependencyProperty. As this is only needed by this class, it is private.
        /// </summary>
        /// <param name="inUIElement">A UIElement object.</param>
        /// <returns>An object that implements ICommand.</returns>
        public static ICommand GetTouchMoveCommand(CartesianChart inUIElement)
        {
            return (ICommand)inUIElement.GetValue(TouchMoveCommandProperty);
        }
        #endregion

        #region The PropertyChangedCallBack method
        /// <summary>
        /// The OnCommandChanged method. This event handles the initial binding and future
        /// binding changes to the bound ICommand
        /// </summary>
        /// <param name="inDependencyObject">A DependencyObject</param>
        /// <param name="inEventArgs">A DependencyPropertyChangedEventArgs object.</param>
        public static void TouchMoveCommandPropertyChangedCallBack(
            DependencyObject inDependencyObject, DependencyPropertyChangedEventArgs inEventArgs)
        {
            CartesianChart uiElement = inDependencyObject as CartesianChart;
            if (null == uiElement)
            {
                return;
            }

            uiElement.TouchMove += (sender, args) =>
            {
                CartesianChart parent = (CartesianChart)sender;
                GetTouchMoveCommand(uiElement).Execute(new TouchMoveCommandParameters()
                {
                    args = args,
                    Sender = parent,
                });
                args.Handled = true;
            };
        }
        #endregion
    }

    public class TouchMoveCommandParameters
    {
        public TouchEventArgs args { get; set; }

        public CartesianChart Sender { get; internal set; }
    }
}
