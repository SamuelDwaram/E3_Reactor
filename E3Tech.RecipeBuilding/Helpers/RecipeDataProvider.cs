using System.Windows;

namespace E3Tech.RecipeBuilding.Helpers
{
    public class RecipeDataProvider
    {
        #region The dependecy Property
        public static readonly DependencyProperty RecipeDataProperty =
                    DependencyProperty.RegisterAttached
                    (
                        "RecipeData",
                        typeof(object),
                        typeof(RecipeDataProvider),
                        new PropertyMetadata(RecipeDataPropertyChangedCallBack)
                    );
        #endregion

        #region The getter and setter
        public static void SetRecipeData(UIElement inUIElement, object value)
        {
            inUIElement.SetValue(RecipeDataProperty, value);
        }

        /// <summary>
        /// Gets the PreviewDropCommand assigned to the PreviewDropCommandProperty
        /// DependencyProperty. As this is only needed by this class, it is private.
        /// </summary>
        /// <param name="inUIElement">A UIElement object.</param>
        /// <returns>An object that implements ICommand.</returns>
        public static object GetRecipeData(UIElement inUIElement)
        {
            return inUIElement.GetValue(RecipeDataProperty);
        }
        #endregion

        #region The PropertyChangedCallBack method
        private static void RecipeDataPropertyChangedCallBack(
            DependencyObject inDependencyObject, DependencyPropertyChangedEventArgs inEventArgs)
        {
            /* My code was not reaching this function except at start up */
            if (!(inDependencyObject is UIElement uiElement))
            {
                return;
            }

            inDependencyObject.SetValue(RecipeDataProperty, inEventArgs.NewValue);
        }
        #endregion
    }
}
