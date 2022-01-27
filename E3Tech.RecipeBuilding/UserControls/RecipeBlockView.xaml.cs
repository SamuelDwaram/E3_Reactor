using E3Tech.RecipeBuilding.Helpers;
using System.Windows;
using System.Windows.Controls;

namespace E3Tech.RecipeBuilding.UserControls
{
    /// <summary>
    /// Interaction logic for SequenceCell.xaml
    /// </summary>
    public partial class RecipeBlockView : UserControl
    {
        public RecipeBlockView()
        {
            InitializeComponent();
        }

        private void UserControl_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ((FrameworkElement)this.GetParentOfType<DataGridCell>()).Focus();
            e.Handled = false;
        }
    }

}
