using E3.DialogServices.DialogDataContexts;
using System.Windows;

namespace E3.DialogServices.DialogTypes
{
    /// <summary>
    /// Interaction logic for AlertWindow.xaml
    /// </summary>
    public partial class AlertWindow : Window
    {
        public AlertWindow(Alert alertWindowDataContext)
        {
            InitializeComponent();
            DataContext = alertWindowDataContext;
        }

        private void Ok_Clicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
