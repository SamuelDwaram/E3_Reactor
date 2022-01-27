using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace E3.ReactorManager.Interfaces.UI.UserControls
{
    /// <summary>
    /// Interaction logic for ToggleButtonUserControl.xaml
    /// </summary>
    public partial class ToggleButtonUserControl : UserControl, INotifyPropertyChanged
    {
        public ToggleButtonUserControl()
        {
            InitializeComponent();

            /* Initially Update the toggle button content */
            toggleButton.Content = ButtonContent;
        }

        #region Property Changed Handlers
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public static readonly DependencyProperty ButtonContentProperty =
           DependencyProperty.Register("ButtonContent", typeof(string), typeof(ButtonOnOffAnimation), new
              PropertyMetadata("Toggle Here", new PropertyChangedCallback(OnButtonContentChanged)));

        public string ButtonContent
        {
            get { return (string)GetValue(ButtonContentProperty); }
            set { SetValue(ButtonContentProperty, value); toggleButton.Content = ButtonContent; OnPropertyChanged(); }
        }

        private static void OnButtonContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToggleButtonUserControl toggleButtonUserControl = d as ToggleButtonUserControl;
            toggleButtonUserControl.OnButtonContentChanged(e);
        }

        private void OnButtonContentChanged(DependencyPropertyChangedEventArgs e)
        {
            toggleButton.Content = e.NewValue.ToString();
        }

        public static readonly DependencyProperty IsToggledProperty =
           DependencyProperty.Register("IsToggled", typeof(bool), typeof(ButtonOnOffAnimation), new
              PropertyMetadata(false, new PropertyChangedCallback(OnIsToggledChanged)));

        public bool IsToggled
        {
            get { return (bool)GetValue(IsToggledProperty); }
            set { SetValue(IsToggledProperty, value); OnPropertyChanged(); }
        }

        private static void OnIsToggledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToggleButtonUserControl toggleButtonUserControl = d as ToggleButtonUserControl;
            toggleButtonUserControl.OnIsToggledChanged(e);
        }

        private void OnIsToggledChanged(DependencyPropertyChangedEventArgs e)
        {
            if (IsToggled)
            {
                //Update the UI
                toggleButton.Background = new SolidColorBrush(Color.FromRgb(47, 81, 93));
                toggleButton.BorderBrush = new SolidColorBrush(Color.FromRgb(76, 220, 220));
                toggleButton.BorderThickness = new Thickness(1);
            }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            /* Negate toggled status and update the UI */
            IsToggled = !IsToggled;

            if (IsToggled)
            {
                //Update the UI
                toggleButton.Background = new SolidColorBrush(Color.FromRgb(47, 81, 93));
                toggleButton.BorderBrush = new SolidColorBrush(Color.FromRgb(76, 220, 220));
                toggleButton.BorderThickness = new Thickness(1);
            }
            else
            {
                //Update the UI
                toggleButton.Background = new SolidColorBrush(Color.FromRgb(30, 36, 50));
                toggleButton.BorderBrush = new SolidColorBrush(Color.FromRgb(76, 220, 220));
                toggleButton.BorderThickness = new Thickness(0);
            }
        }

        public static readonly DependencyProperty ButtonCommandProperty =
           DependencyProperty.Register("ButtonCommand", typeof(ICommand), typeof(ToggleButtonUserControl), new
              PropertyMetadata(null, new PropertyChangedCallback(OnButtonCommandChanged)));

        public ICommand ButtonCommand
        {
            get { return (ICommand)GetValue(ButtonCommandProperty); }
            set { SetValue(ButtonCommandProperty, value); }
        }

        private static void OnButtonCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToggleButtonUserControl toggleButtonUserControl = d as ToggleButtonUserControl;
            toggleButtonUserControl.OnButtonCommandChanged(e);
        }

        private void OnButtonCommandChanged(DependencyPropertyChangedEventArgs e)
        {
            //Do something when this property changed
            if (e.NewValue != null)
            {
                toggleButton.Command = ButtonCommand;
            }
        }

        public static readonly DependencyProperty ButtonCommandParameterProperty =
           DependencyProperty.Register("ButtonCommandParameter", typeof(object), typeof(ToggleButtonUserControl), new
              PropertyMetadata(null, new PropertyChangedCallback(OnButtonCommandParameterChanged)));

        public object ButtonCommandParameter
        {
            get { return (object)GetValue(ButtonCommandParameterProperty); }
            set { SetValue(ButtonCommandParameterProperty, value); }
        }

        private static void OnButtonCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToggleButtonUserControl toggleButtonUserControl = d as ToggleButtonUserControl;
            toggleButtonUserControl.OnButtonCommandParameterChanged(e);
        }

        private void OnButtonCommandParameterChanged(DependencyPropertyChangedEventArgs e)
        {
            //Do something when this property changed
            if (e.NewValue != null)
            {
                toggleButton.CommandParameter = ButtonCommandParameter;
            }
        }
    }
}
