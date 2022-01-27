using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace E3Tech.RecipeBuilding.UserControls
{
    /// <summary>
    /// Interaction logic for ToggleButton.xaml
    /// </summary>
    public partial class ToggleButton : UserControl
    {
        SolidColorBrush Off = new SolidColorBrush(Color.FromRgb(160, 160, 160));
        SolidColorBrush On = new SolidColorBrush(Color.FromRgb(130, 190, 125));
        private bool Toggled = false;

        public ToggleButton()
        {
            InitializeComponent();

            if (Toggled)
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

        public bool Toggled1 { get => Toggled; set => Toggled = value; }

        private void Dot_Click(object sender, RoutedEventArgs e)
        {
            if (!Toggled)
            {
                Back.Fill = On;
                Toggled = true;
                Dot.HorizontalAlignment = HorizontalAlignment.Right;

                IsToggled = true;
            }
            else
            {
                Back.Fill = Off;
                Toggled = false;
                Dot.HorizontalAlignment = HorizontalAlignment.Left;

                IsToggled = false;
            }
        }

        private void Back_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Toggled)
            {
                Back.Fill = On;
                Toggled = true;
                Dot.HorizontalAlignment = HorizontalAlignment.Right;

                IsToggled = true;
            }
            else
            {
                Back.Fill = Off;
                Toggled = false;
                Dot.HorizontalAlignment = HorizontalAlignment.Left;

                IsToggled = false;
            }
        }

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
            if (bool.Parse(e.NewValue.ToString()))
            {
                this.toggleButtonViewBox.IsEnabled = true;
            }
            else
            {
                this.toggleButtonViewBox.IsEnabled = false;
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
            Toggled = (bool)e.NewValue;
            if (Toggled)
            {
                Back.Fill = On;
                Dot.HorizontalAlignment = HorizontalAlignment.Right;
            }
            else
            {
                Back.Fill = Off;
                Dot.HorizontalAlignment = HorizontalAlignment.Left;
            }
        }
    }
}
