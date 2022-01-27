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
    /// Interaction logic for ToggleButton.xaml
    /// </summary>
    public partial class ToggleButton : UserControl
    {
        SolidColorBrush Off = new SolidColorBrush(Color.FromRgb(30, 36, 50));
        SolidColorBrush On = new SolidColorBrush(Color.FromRgb(30, 36, 50));
        private bool Toggled = false;

        public ToggleButton()
        {
            InitializeComponent();
            Back.Fill = Off;
            Toggled = false;
            Dot.HorizontalAlignment = HorizontalAlignment.Left;
        }

        public bool Toggled1 { get => Toggled; set => Toggled = value; }

        public static readonly DependencyProperty IsToggleButtonEnabledProperty =
           DependencyProperty.Register("IsToggleButtonEnabled", typeof(bool), typeof(ToggleButton), new
              PropertyMetadata(false, new PropertyChangedCallback(OnIsToggleButtonEnabledChanged)));

        public bool IsToggleButtonEnabled
        {
            get { return (bool)GetValue(IsToggleButtonEnabledProperty); }
            set { SetValue(IsToggleButtonEnabledProperty, value); }
        }

        private static void OnIsToggleButtonEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToggleButton toggleButtonUserControl = d as ToggleButton;
            toggleButtonUserControl.OnIsToggleButtonEnabledChanged(e);
        }

        private void OnIsToggleButtonEnabledChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                if (bool.Parse(e.NewValue.ToString()))
                {
                    this.toggleButtonViewBox.IsEnabled = true;
                }
                else
                {
                    this.toggleButtonViewBox.IsEnabled = false;
                }
            }
        }

        public static readonly DependencyProperty IsToggledProperty =
           DependencyProperty.Register("IsToggled", typeof(bool), typeof(ToggleButton), new
              PropertyMetadata(false, new PropertyChangedCallback(OnIsToggledChanged)));

        public bool IsToggled
        {
            get { return (bool)GetValue(IsToggledProperty); }
            set { SetValue(IsToggledProperty, value); }
        }

        private static void OnIsToggledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToggleButton toggleButtonUserControl = d as ToggleButton;
            toggleButtonUserControl.OnIsToggledChanged(e);
        }

        private void OnIsToggledChanged(DependencyPropertyChangedEventArgs e)
        {
            //Do something when this property changed
            if (IsToggled)
            {
                Back.Fill = On;
                Toggled = true;
                Dot.HorizontalAlignment = HorizontalAlignment.Right;
            }
            else
            {
                Back.Fill = Off;
                Toggled = false;
                Dot.HorizontalAlignment = HorizontalAlignment.Left;
            }
        }

        public static readonly DependencyProperty ButtonCommandProperty =
           DependencyProperty.Register("ButtonCommand", typeof(ICommand), typeof(ToggleButton), new
              PropertyMetadata(null, new PropertyChangedCallback(OnButtonCommandChanged)));

        public ICommand ButtonCommand
        {
            get { return (ICommand)GetValue(ButtonCommandProperty); }
            set { SetValue(ButtonCommandProperty, value); }
        }

        private static void OnButtonCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToggleButton toggleButtonUserControl = d as ToggleButton;
            toggleButtonUserControl.OnButtonCommandChanged(e);
        }

        private void OnButtonCommandChanged(DependencyPropertyChangedEventArgs e)
        {
            //Do something when this property changed
            if (e.NewValue != null)
            {
                Dot.Command = ButtonCommand;
            }
        }

        public static readonly DependencyProperty ButtonCommandParameterProperty =
           DependencyProperty.Register("ButtonCommandParameter", typeof(object), typeof(ToggleButton), new
              PropertyMetadata(null, new PropertyChangedCallback(OnButtonCommandParameterChanged)));

        public object ButtonCommandParameter
        {
            get { return (object)GetValue(ButtonCommandParameterProperty); }
            set { SetValue(ButtonCommandParameterProperty, value); }
        }

        private static void OnButtonCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToggleButton toggleButtonUserControl = d as ToggleButton;
            toggleButtonUserControl.OnButtonCommandParameterChanged(e);
        }

        private void OnButtonCommandParameterChanged(DependencyPropertyChangedEventArgs e)
        {
            //Do something when this property changed
            if (e.NewValue != null)
            {
                Dot.CommandParameter = ButtonCommandParameter;
            }
        }
    }
}
