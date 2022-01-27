using E3.UserManager.ViewModels;
using System.Windows.Controls;

namespace E3.UserManager.Views
{
    /// <summary>
    /// Interaction logic for ModifyUserView.xaml
    /// </summary>
    public partial class ModifyUserView : UserControl
    {
        public ModifyUserView(ModifyUserViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
