using E3.DialogServices.DialogDataContexts;
using System.Windows;

namespace E3.DialogServices.DialogTypes
{
    /// <summary>
    /// Interaction logic for DynamicDialogWindow.xaml
    /// </summary>
    public partial class DynamicDialogWindow : Window
    {
        public DynamicDialogWindow(DynamicDialogDataContext dynamicDialogDataContext)
        {
            InitializeComponent();
            DataContext = dynamicDialogDataContext;
        }
    }
}
