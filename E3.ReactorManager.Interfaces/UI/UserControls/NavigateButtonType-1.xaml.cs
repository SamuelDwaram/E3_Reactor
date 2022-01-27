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

namespace E3.ReactorManager.Interfaces.UI.UserControls
{
    /// <summary>
    /// Interaction logic for NavigateButtonType_1.xaml
    /// </summary>
    public partial class NavigateButtonType_1 : UserControl
    {
        public NavigateButtonType_1()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ButtonCommandProperty =
           DependencyProperty.Register("ButtonCommand", typeof(ICommand), typeof(NavigateButtonType_1), new
              PropertyMetadata(null, new PropertyChangedCallback(OnButtonCommandChanged)));

        public ICommand ButtonCommand
        {
            get { return (ICommand)GetValue(ButtonCommandProperty); }
            set { SetValue(ButtonCommandProperty, value); }
        }

        private static void OnButtonCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NavigateButtonType_1 toggleButtonUserControl = d as NavigateButtonType_1;
            toggleButtonUserControl.OnButtonCommandChanged(e);
        }

        private void OnButtonCommandChanged(DependencyPropertyChangedEventArgs e)
        {
            //Do something when this property changed
            if (e.NewValue != null)
            {
                NavigateButton.Command = ButtonCommand;
            }
        }

        public static readonly DependencyProperty ButtonCommandParameterProperty =
           DependencyProperty.Register("ButtonCommandParameter", typeof(object), typeof(NavigateButtonType_1), new
              PropertyMetadata(null, new PropertyChangedCallback(OnButtonCommandParameterChanged)));

        public object ButtonCommandParameter
        {
            get { return (object)GetValue(ButtonCommandParameterProperty); }
            set { SetValue(ButtonCommandParameterProperty, value); }
        }

        private static void OnButtonCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NavigateButtonType_1 toggleButtonUserControl = d as NavigateButtonType_1;
            toggleButtonUserControl.OnButtonCommandParameterChanged(e);
        }

        private void OnButtonCommandParameterChanged(DependencyPropertyChangedEventArgs e)
        {
            //Do something when this property changed
            if (e.NewValue != null)
            {
                NavigateButton.CommandParameter = ButtonCommandParameter;
            }
        }
    }
}
