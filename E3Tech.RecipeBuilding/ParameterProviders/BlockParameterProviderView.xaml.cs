using E3Tech.RecipeBuilding.ViewModels;
using System.Windows;

namespace E3Tech.RecipeBuilding.ParameterProviders
{
    /// <summary>
    /// Interaction logic for StartBlockParameterProvider.xaml
    /// </summary>
    public partial class BlockParameterProviderView : Window
    {
        public BlockParameterProviderViewModel ViewModel { get; private set; }

        public BlockParameterProviderView(BlockParameterProviderViewModel viewModel)
        {
            InitializeComponent();
            DataContext = ViewModel = viewModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
