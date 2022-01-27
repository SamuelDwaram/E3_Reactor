using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace E3Tech.RecipeBuilding.Helpers
{
    public class MouseButtonBehavior
    {
        #region The dependecy Property
        /// <summary>
        /// The Dependency property. To allow for Binding, a dependency
        /// property must be used.
        /// </summary>
        public static readonly DependencyProperty MouseButtonCommandProperty =
                    DependencyProperty.RegisterAttached
                    (
                        "MouseButtonCommand",
                        typeof(ICommand),
                        typeof(MouseButtonBehavior),
                        new PropertyMetadata(MouseButtonCommandPropertyChangedCallBack)
                    );
        #endregion

        #region The getter and setter
        /// <summary>
        /// The setter. This sets the value of the PreviewMouseButtonCommandProperty
        /// Dependency Property. It is expected that you use this only in XAML
        ///
        /// This appears in XAML with the "Set" stripped off.
        /// XAML usage:
        ///
        /// <Grid helpers:MouseButtonBehavior.MouseButtonCommand="{Binding MouseButtonCommand}" />
        ///
        /// </summary>
        /// <param name="inUIElement">A UIElement object. In XAML this is automatically passed
        /// in, so you don't have to enter anything in XAML.</param>
        /// <param name="inCommand">An object that implements ICommand.</param>
        public static void SetMouseButtonCommand(ListBox inUIElement, ICommand inCommand)
        {
            inUIElement.SetValue(MouseButtonCommandProperty, inCommand);
        }

        /// <summary>
        /// Gets the PreviewMouseButtonCommand assigned to the PreviewMouseButtonCommandProperty
        /// DependencyProperty. As this is only needed by this class, it is private.
        /// </summary>
        /// <param name="inUIElement">A UIElement object.</param>
        /// <returns>An object that implements ICommand.</returns>
        public static ICommand GetMouseButtonCommand(ListBox inUIElement)
        {
            return (ICommand)inUIElement.GetValue(MouseButtonCommandProperty);
        }
        #endregion

        #region The PropertyChangedCallBack method
        /// <summary>
        /// The OnCommandChanged method. This event handles the initial binding and future
        /// binding changes to the bound ICommand
        /// </summary>
        /// <param name="inDependencyObject">A DependencyObject</param>
        /// <param name="inEventArgs">A DependencyPropertyChangedEventArgs object.</param>
        public static void MouseButtonCommandPropertyChangedCallBack(
            DependencyObject inDependencyObject, DependencyPropertyChangedEventArgs inEventArgs)
        {
            ListBox uiElement = inDependencyObject as ListBox;
            if (null == uiElement)
            {
                return;
            }

            uiElement.PreviewMouseLeftButtonDown += (sender, args) =>
            {
                ListBox parent = (ListBox)sender;
                object data = parent.GetDataFromListBox(args.GetPosition(parent));
                if (data == null)
                {
                    return;
                }
                GetMouseButtonCommand(uiElement).Execute(new MouseButtonCommandParameters()
                {
                    Data = data,
                    Sender = parent,
                });
                args.Handled = true;
            };
        }
        #endregion
    }

    public class MouseButtonCommandParameters
    {
        public object Data { get; internal set; }
        public ListBox Sender { get; internal set; }
    }
}

