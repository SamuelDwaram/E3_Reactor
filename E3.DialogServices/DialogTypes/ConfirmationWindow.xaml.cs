using E3.DialogServices.DialogDataContexts;
using System.Windows;

namespace E3.DialogServices.DialogTypes
{
    /// <summary>
    /// Interaction logic for ConfirmationWindow.xaml
    /// </summary>
    public partial class ConfirmationWindow : Window
    {
        public ConfirmationWindow(Confirmation confirmationWindowDataContext)
        {
            InitializeComponent();
            DataContext = confirmationWindowDataContext;
        }

        private void Cancel_Clicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Ok_Clicked(object sender, RoutedEventArgs e)
        {
            (DataContext as Confirmation).Confirmed = true;
            Close();
        }
    }
}
