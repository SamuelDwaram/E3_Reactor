using E3Tech.RecipeBuilding.Model.Blocks;
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

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string warningContent = " Warning: Please fill all the details";
            string TimeIntervalWarning = " Warning: Please fill the Numeric in Time Interval";
            int convert;
            if (ViewModel.Parameters is StirrerBlockParameters)
            {
                var stirrerBlockParameters = ViewModel.Parameters as StirrerBlockParameters;
                if (stirrerBlockParameters.SetPoint == null || stirrerBlockParameters.Destination == null)
                {
                    Warning.Content = warningContent;
                    Warning.Visibility = Visibility.Visible;
                    return;
                }
                else if (int.TryParse(stirrerBlockParameters.SetPoint, out convert) == false)
                {
                    Warning.Content = " Warning: Please fill the Numeric in Set point";
                    Warning.Visibility = Visibility.Visible;
                    return;
                }
            }
            else if (ViewModel.Parameters is WaitBlockParameters)
            {
                var waitBlockParameters = ViewModel.Parameters as WaitBlockParameters;
                if (waitBlockParameters.IntervalType == null || waitBlockParameters.TimeInterval == null)
                {
                    Warning.Content = warningContent;
                    Warning.Visibility = Visibility.Visible;
                    return;
                }
                else if (int.TryParse(waitBlockParameters.TimeInterval, out convert) == false)
                {
                    Warning.Content = TimeIntervalWarning;
                    Warning.Visibility = Visibility.Visible;
                    return;
                }
            }
            //else if (ViewModel.Parameters is N2PurgeBlockParameters)
            //{
             
            //    var n2PurgeBlockParameters = ViewModel.Parameters as N2PurgeBlockParameters;
            //    if (n2PurgeBlockParameters.IntervalType == null || n2PurgeBlockParameters.TimeInterval == null || n2PurgeBlockParameters.Source == null)
            //    {
            //        Warning.Content = warningContent;
            //        Warning.Visibility = Visibility.Visible;
            //        return;
            //    }
            //    else if (int.TryParse(n2PurgeBlockParameters.TimeInterval, out convert) == false)
            //    {
            //        Warning.Content = TimeIntervalWarning;
            //        Warning.Visibility = Visibility.Visible;
            //        return;
            //    }
            //}
            else if (ViewModel.Parameters is TransferBlockParameters)
            {
                var transferBlockParameters = ViewModel.Parameters as TransferBlockParameters;
                if (transferBlockParameters.Source == null || transferBlockParameters.Destination == null)
                {
                    Warning.Visibility = Visibility.Visible;
                    return;
                }
                else if (transferBlockParameters.TransferMode == bool.TrueString)
                {
                    if (transferBlockParameters.TimeInterval == null || transferBlockParameters.IntervalType == null)
                    {
                        Warning.Content = warningContent;
                        Warning.Visibility = Visibility.Visible;
                        return;
                    }
                    else if (int.TryParse(transferBlockParameters.TimeInterval, out convert) == false)
                    {
                        Warning.Content = TimeIntervalWarning;
                        Warning.Visibility = Visibility.Visible;
                        return;
                    }
                }
            }
            else if (ViewModel.Parameters is DrainBlockParameters)
            {
                var drainBlockParameters = ViewModel.Parameters as DrainBlockParameters;
                if (/*drainBlockParameters.IntervalType == null || drainBlockParameters.TimeInterval == null ||*/ drainBlockParameters.Source == null)
                {
                    Warning.Content = warningContent;
                    Warning.Visibility = Visibility.Visible;
                    return;
                }
                //else if (int.TryParse(drainBlockParameters.TimeInterval, out convert) == false)
                //{
                //    Warning.Content = TimeIntervalWarning;
                //    Warning.Visibility = Visibility.Visible;
                //    return;
                //}
            }
            this.DialogResult = true;
        }
    }
}
