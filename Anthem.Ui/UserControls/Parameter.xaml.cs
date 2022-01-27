using Anathem.Ui.Helpers;
using Prism.Commands;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Anathem.Ui.UserControls
{
    /// <summary>
    /// Interaction logic for Parameter.xaml
    /// </summary>
    public partial class Parameter : UserControl
    {
        public Parameter()
        {
            InitializeComponent();
            Loaded += Parameter_Loaded;
        }

        private void Parameter_Loaded(object sender, RoutedEventArgs e)
        {
            BindingOperations.SetBinding(this, LiveDataProperty, new Binding("Parameters") { 
                Source = DataContext,
                Mode = BindingMode.OneWay,
                Converter = new ParameterExtractorConverter(),
                ConverterParameter = Tag.ToString()
            });
        }

        #region Live Data
        public static readonly DependencyProperty LiveDataProperty =
           DependencyProperty.Register("LiveData", typeof(string), typeof(Parameter), 
               new PropertyMetadata("NC", new PropertyChangedCallback((dp, args) => (dp as Parameter).OnLiveDataReceived(args.NewValue.ToString()))));

        public string LiveData
        {
            get { return (string)GetValue(LiveDataProperty); }
            set { SetValue(LiveDataProperty, value); }
        }

        private void OnLiveDataReceived(string newValue)
        {
            if (bool.TryParse(newValue, out bool parseResult))
            {
                if (parseResult)
                {
                    paramLabel.Background = new SolidColorBrush(Color.FromRgb(91, 201, 208));
                    paramLabel.Foreground = Brushes.White;
                }
                else
                {
                    paramLabel.Background = new SolidColorBrush(Color.FromRgb(30, 36, 50));
                    paramLabel.Foreground = new SolidColorBrush(Color.FromRgb(142, 148, 161));
                }
            }
        }
        #endregion

        #region Param Label
        public static readonly DependencyProperty ParamLabelProperty =
           DependencyProperty.Register("ParamLabel", typeof(string), typeof(Parameter), new
              PropertyMetadata(string.Empty, new PropertyChangedCallback((dp, args) => (dp as Parameter).paramLabel.Content = args.NewValue.ToString())));

        public string ParamLabel
        {
            get { return (string)GetValue(ParamLabelProperty); }
            set { SetValue(ParamLabelProperty, value); }
        }
        #endregion

        #region OnClick
        public static ICommand GetOnClick(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(OnClickProperty);
        }

        public static void SetOnClick(DependencyObject obj, ICommand value)
        {
            obj.SetValue(OnClickProperty, value);
        }

        // Using a DependencyProperty as the backing store for OnClick.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OnClickProperty =
            DependencyProperty.RegisterAttached("OnClick", typeof(ICommand), typeof(Parameter), new PropertyMetadata(new DelegateCommand(() => { }), OnClickChanged));

        private static void OnClickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Parameter))
            {
                return;
            }

            (d as Parameter).PreviewMouseDown += (sender, args) => {
                Parameter parameter = sender as Parameter;
                GetOnClick(parameter).Execute($"{parameter.Tag}|{parameter.LiveData}");
            };
        }
        #endregion
    }
}
