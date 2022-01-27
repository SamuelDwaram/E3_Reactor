using Anathem.Ui.Helpers;
using E3.ReactorManager.Interfaces.UI.UserControls;
using Prism.Commands;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Anathem.Ui.UserControls
{
    /// <summary>
    /// Interaction logic for ReactorParameter.xaml
    /// </summary>
    public partial class GraphicalParameter : UserControl
    {
        private static readonly Regex _regex = new Regex("[^0-9.-]+");

        public GraphicalParameter()
        {
            InitializeComponent();
            Loaded += GraphicalParameter_Loaded;
        }

        private void GraphicalParameter_Loaded(object sender, RoutedEventArgs e)
        {
            progressAnimation.SetBinding(ProgressBarAnimation.CurrentValueProperty, new Binding("ParameterDictionary")
            {
                Source = DataContext,
                Converter = new ParameterExtractorConverter(),
                ConverterParameter = Tag.ToString()
            });

            paramInput.SetBinding(TextBox.TextProperty, new Binding("ParameterDictionary")
            {
                Source = DataContext,
                Converter = new ParameterExtractorConverter(),
                ConverterParameter = Tag.ToString()
            });

            paramInput.PreviewTextInput += (obj, args) => {
                args.Handled = _regex.IsMatch(args.Text);
            };

            paramInput.KeyDown += (obj, args) => {
                if (args.Key == Key.Enter)
                {
                    Command.Execute($"{Tag}|{ParamDataType}|{paramInput.Text}");
                    paramInput.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }
            };

            paramInput.IsReadOnly = !IsEditable;
        }

        #region Command
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(GraphicalParameter), new PropertyMetadata(DummyCommand));
        #endregion

        #region Param Limits
        public string Limits
        {
            get { return (string)GetValue(LimitsProperty); }
            set { SetValue(LimitsProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Limits.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LimitsProperty =
            DependencyProperty.Register("Limits", typeof(string), typeof(GraphicalParameter), new PropertyMetadata("-90|200", 
                (dp, args) => {
                    if (!string.IsNullOrWhiteSpace(args.NewValue.ToString()))
                    {
                        string[] newLimits = args.NewValue.ToString().Split('|');

                        GraphicalParameter gp = dp as GraphicalParameter;
                        gp.leftLimit.Content = newLimits[0];
                        gp.rightLimit.Content = newLimits[1];
                        gp.progressAnimation.NegativeMaximumValue = newLimits[0];
                        gp.progressAnimation.PositiveMaximumValue = newLimits[1];
                    }
                }));
        #endregion

        #region Param Units
        public string Units
        {
            get { return (string)GetValue(UnitsProperty); }
            set { SetValue(UnitsProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Units.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnitsProperty =
            DependencyProperty.Register("Units", typeof(string), typeof(GraphicalParameter), new PropertyMetadata(string.Empty, 
                (dp, args) => (dp as GraphicalParameter).paramUnits.Content = args.NewValue.ToString()));
        #endregion

        #region Param Label
        public string ParamLabel
        {
            get { return (string)GetValue(ParamLabelProperty); }
            set { SetValue(ParamLabelProperty, value); }
        }
        public static readonly DependencyProperty ParamLabelProperty =
           DependencyProperty.Register("ParamLabel", typeof(string), typeof(GraphicalParameter), new
              PropertyMetadata(string.Empty, new PropertyChangedCallback((dp, args) => (dp as GraphicalParameter).paramLabel.Content = args.NewValue.ToString())));
        #endregion

        #region Param DataType
        public string ParamDataType
        {
            get { return (string)GetValue(ParamDataTypeProperty); }
            set { SetValue(ParamDataTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ParamDataType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ParamDataTypeProperty =
            DependencyProperty.Register("ParamDataType", typeof(string), typeof(GraphicalParameter), new PropertyMetadata("int"));
        #endregion

        public bool IsEditable { get; set; } = false;
        public static ICommand DummyCommand => new DelegateCommand(() => { });

    }
}
