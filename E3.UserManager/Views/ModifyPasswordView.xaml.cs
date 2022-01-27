using E3.UserManager.ViewModels;
using System.Windows.Controls;

namespace E3.UserManager.Views
{
    /// <summary>
    /// Interaction logic for ModifyPasswordView.xaml
    /// </summary>
    public partial class ModifyPasswordView : UserControl
    {
        public ModifyPasswordView(ModifyPasswordViewModel modifyPasswordViewModel)
        {
            InitializeComponent();
            DataContext = modifyPasswordViewModel;
        }
    }
}
