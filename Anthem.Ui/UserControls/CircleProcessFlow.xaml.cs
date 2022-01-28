using Anathem.Ui.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Anathem.Ui.UserControls
{
    /// <summary>
    /// Interaction logic for CircleProcessFlow.xaml
    /// </summary>
    public partial class CircleProcessFlow : UserControl
    {
        public CircleProcessFlow()
        {
            InitializeComponent();
            Loaded += Parameter_Loaded;

        }
        private void Parameter_Loaded(object sender, RoutedEventArgs e)
        {
            BindingOperations.SetBinding(this, LiveDataProperty, new Binding("Parameters")
            {
                Source = DataContext,
                Mode = BindingMode.OneWay,
                Converter = new ParameterExtractorConverter(),
                ConverterParameter = Tag.ToString()
            });
        }

        #region Live Data
        public static readonly DependencyProperty LiveDataProperty =
           DependencyProperty.Register("LiveData", typeof(string), typeof(CircleProcessFlow),
               new PropertyMetadata("NC", new PropertyChangedCallback((dp, args) => (dp as CircleProcessFlow).OnLiveDataReceived(args.NewValue))));

        public string LiveData
        {
            get { return (string)GetValue(LiveDataProperty); }
            set { SetValue(LiveDataProperty, value); }
        }

        private void OnLiveDataReceived(object obj)
        {
            string newValue = obj == null ? bool.FalseString : obj.ToString();
            if (bool.TryParse(newValue, out bool parseResult))
            {
                if (parseResult)
                {
                    //paramLabel.Background = new SolidColorBrush(Color.FromRgb(91, 201, 208));
                    paramLabel.Fill = new SolidColorBrush(Color.FromRgb(0, 154, 23));
                    //paramLabel.Fill = Brushes.White;
                }
                else
                {
                    paramLabel.Fill = Brushes.Red;
                    //paramLabel.Background = new SolidColorBrush(Color.FromRgb(30, 36, 50));
                    //paramLabel.Foreground = new SolidColorBrush(Color.FromRgb(142, 148, 161));
                }
            }
        }
        #endregion

        #region Param Label
        public static readonly DependencyProperty ParamLabelProperty =
           DependencyProperty.Register("ParamLabel", typeof(string), typeof(CircleProcessFlow));
        //new
        //      PropertyMetadata(string.Empty, new PropertyChangedCallback(dummyCommand));

        public string ParamLabel
        {
            get { return (string)GetValue(ParamLabelProperty); }
            set { SetValue(ParamLabelProperty, value); }
        }
        #endregion

        #region OnClick
        //public static ICommand GetOnClick(DependencyObject obj)
        //{
        //    return (ICommand)obj.GetValue(OnClickProperty);
        //}

        //public static void SetOnClick(DependencyObject obj, ICommand value)
        //{
        //    obj.SetValue(OnClickProperty, value);
        //}

        //// Using a DependencyProperty as the backing store for OnClick.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty OnClickProperty =
        //    DependencyProperty.RegisterAttached("OnClick", typeof(ICommand), typeof(CircleProcessFlow), new PropertyMetadata(new DelegateCommand(() => { }), OnClickChanged));

        //private static void OnClickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    if (!(d is BoolParameterProcessFlow))
        //    {
        //        return;
        //    }

        //    (d as BoolParameterProcessFlow).PreviewMouseDown += (sender, args) => {
        //        BoolParameterProcessFlow parameter = sender as BoolParameterProcessFlow;
        //        GetOnClick(parameter).Execute($"{parameter.Tag}|{parameter.LiveData}");
        //    };
        //}
        #endregion
    }
}
