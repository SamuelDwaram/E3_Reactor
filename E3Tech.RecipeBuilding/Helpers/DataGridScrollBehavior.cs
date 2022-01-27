using System;
using System.Windows;
using System.Windows.Controls;

namespace E3Tech.RecipeBuilding.Helpers
{
    public class DataGridScrollBehavior
    {
        public static readonly DependencyProperty SelectingItemIndexProperty = DependencyProperty.RegisterAttached(
               "SelectingItemIndex",
               typeof(int),
               typeof(DataGridScrollBehavior),
               new PropertyMetadata(0, OnSelectingItemChanged));

        public static int GetSelectingItemIndex(DependencyObject target)
        {
            return (int)target.GetValue(SelectingItemIndexProperty);
        }

        public static void SetSelectingItemIndex(DependencyObject target, int value)
        {
            target.SetValue(SelectingItemIndexProperty, value);
        }

        private static void OnSelectingItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is DataGrid grid) || grid.SelectedItem == null)
            {
                return;
            }

            // Works with .Net 4.5
            grid.Dispatcher.InvokeAsync(() =>
            {
                grid.UpdateLayout();
                if (grid.SelectedItem != null)
                {
                    grid.ScrollIntoView(grid.SelectedItem, null);
                }
            });

            // Works with .Net 4.0
            grid.Dispatcher.BeginInvoke((Action)(() =>
            {
                grid.UpdateLayout();
                if (grid.SelectedItem != null)
                    grid.ScrollIntoView(grid.SelectedItem, null);
            }));
        }
    }
}
