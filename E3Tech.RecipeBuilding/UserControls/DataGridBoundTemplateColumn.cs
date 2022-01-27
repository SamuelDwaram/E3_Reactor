using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace E3Tech.RecipeBuilding.UserControls
{
    public class DataGridBoundTemplateColumn : DataGridBoundColumn
    {
        public DataTemplate CellTemplate { get; set; }

        public DataTemplate CellEditingTemplate { get; set; }

        protected override System.Windows.FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            return Generate(dataItem, CellEditingTemplate);
        }

        private FrameworkElement Generate(object dataItem, DataTemplate template)
        {
            ContentControl contentControl = new ContentControl { ContentTemplate = template };
            BindingOperations.SetBinding(contentControl, ContentControl.ContentProperty, Binding);
            return contentControl;
        }

        protected override System.Windows.FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            return Generate(dataItem, CellTemplate);
        }
    }
}

