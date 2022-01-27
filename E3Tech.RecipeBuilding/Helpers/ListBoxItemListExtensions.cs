using E3Tech.RecipeBuilding.UserControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace E3Tech.RecipeBuilding.Helpers
{
    internal static class ListBoxItemListExtensions
    {
        public static object GetDataFromListBox(this ListBox source, Point point)
        {
            UIElement element = source.InputHitTest(point) as UIElement;
            if (element != null)
            {
                object data = DependencyProperty.UnsetValue;
                while (data == DependencyProperty.UnsetValue)
                {
                    data = source.ItemContainerGenerator.ItemFromContainer(element);

                    if (data == DependencyProperty.UnsetValue)
                    {
                        element = VisualTreeHelper.GetParent(element) as FrameworkElement;
                    }

                    if (element == source)
                    {
                        return null;
                    }
                }

                if (data != DependencyProperty.UnsetValue)
                {
                    return data;
                }
            }
            return null;
        }
    }
}
