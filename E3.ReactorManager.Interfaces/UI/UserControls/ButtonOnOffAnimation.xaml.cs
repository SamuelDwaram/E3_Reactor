using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace E3.ReactorManager.Interfaces.UI.UserControls
{
    /// <summary>
    /// Interaction logic for ButtonOnOffAnimation.xaml
    /// </summary>
    public partial class ButtonOnOffAnimation : UserControl
    {
        public ButtonOnOffAnimation()
        {
            InitializeComponent();
        }

        #region Label
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(ButtonOnOffAnimation), new PropertyMetadata(LabelChanged));

        private static void LabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ButtonOnOffAnimation buttonOnOffAnimation = d as ButtonOnOffAnimation;
            string label = e.NewValue.ToString();
            if (string.IsNullOrWhiteSpace(label))
            {
                buttonOnOffAnimation.PathControl.Visibility = Visibility.Visible;
                buttonOnOffAnimation.buttonLabel.Visibility = Visibility.Hidden;
            }
            else
            {
                buttonOnOffAnimation.PathControl.Visibility = Visibility.Hidden;
                buttonOnOffAnimation.buttonLabel.Visibility = Visibility.Visible;
                buttonOnOffAnimation.buttonLabel.Content = label;
            }
        }
        #endregion

        #region CommandInProgress
        public static string GetCommandInProgress(DependencyObject obj)
        {
            return (string)obj.GetValue(CommandInProgressProperty);
        }

        public static void SetCommandInProgress(DependencyObject obj, string value)
        {
            obj.SetValue(CommandInProgressProperty, value);
        }

        // Using a DependencyProperty as the backing store for CommandInProgress.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandInProgressProperty =
            DependencyProperty.RegisterAttached("CommandInProgress", typeof(string), typeof(ButtonOnOffAnimation), new PropertyMetadata(bool.FalseString,
                (dp, args) => (dp as ButtonOnOffAnimation).UpdateUi()));
        #endregion

        #region Failure
        public static string GetFailure(DependencyObject obj)
        {
            return (string)obj.GetValue(FailureProperty);
        }

        public static void SetFailure(DependencyObject obj, string value)
        {
            obj.SetValue(FailureProperty, value);
        }

        // Using a DependencyProperty as the backing store for Failure.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FailureProperty =
            DependencyProperty.RegisterAttached("Failure", typeof(string), typeof(ButtonOnOffAnimation), new PropertyMetadata(bool.FalseString,
                (dp, args) => (dp as ButtonOnOffAnimation).UpdateUi()));
        #endregion

        #region Status
        public static string GetStatus(DependencyObject obj)
        {
            return (string)obj.GetValue(StatusProperty);
        }

        public static void SetStatus(DependencyObject obj, string value)
        {
            obj.SetValue(StatusProperty, value);
        }

        // Using a DependencyProperty as the backing store for Status.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.RegisterAttached("Status", typeof(string), typeof(ButtonOnOffAnimation), new PropertyMetadata(bool.FalseString,
                (dp, args) => (dp as ButtonOnOffAnimation).UpdateUi()));
        #endregion

        public void UpdateUi()
        {
            bool.TryParse(GetStatus(this), out bool status);
            bool.TryParse(GetFailure(this), out bool failure);
            bool.TryParse(GetCommandInProgress(this), out bool commandInProgress);

            if (failure)
            {
                IsEnabled = false;
                EllipseControl.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                PathControl.Fill = new SolidColorBrush(Color.FromRgb(142, 148, 161));
                PathControl.Data = Geometry.Parse("M-378.5,1221.8L-378.5,1221.8c0.3,0,0.6,0.3,0.6,0.6v2.6c0,0.3-0.2,0.6-0.6,0.6l0,0c-0.3,0-0.6-0.3-0.6-0.6v-2.6C-379,1222.1-378.8,1221.8-378.5,1221.8zM-378.5,1230.2c-0.7,0-1.4-0.2-2-0.5c-1.9-1.1-2.6-3.6-1.5-5.5c0.4-0.6,0.9-1.1,1.5-1.5c0.3-0.2,0.6-0.1,0.8,0.2c0.2,0.3,0.1,0.6-0.2,0.8c-0.4,0.3-0.8,0.6-1.1,1.1c-0.8,1.4-0.3,3.2,1.1,4c0.7,0.4,1.5,0.5,2.2,0.3c0.8-0.2,1.4-0.7,1.8-1.4c0.4-0.7,0.5-1.5,0.3-2.2c-0.2-0.8-0.7-1.4-1.4-1.8c-0.3-0.2-0.4-0.5-0.2-0.8c0.2-0.3,0.5-0.4,0.8-0.2c0.9,0.5,1.6,1.4,1.9,2.5s0.1,2.1-0.4,3.1c-0.5,0.9-1.4,1.6-2.5,1.9C-377.8,1230.1-378.1,1230.2-378.5,1230.2z");
            }
            else if (commandInProgress)
            {
                IsEnabled = false;
                EllipseControl.Fill = new SolidColorBrush(Color.FromRgb(255, 165, 0));
                PathControl.Fill = new SolidColorBrush(Color.FromRgb(142, 148, 161));
                PathControl.Data = Geometry.Parse("M-378.5,1221.8L-378.5,1221.8c0.3,0,0.6,0.3,0.6,0.6v2.6c0,0.3-0.2,0.6-0.6,0.6l0,0c-0.3,0-0.6-0.3-0.6-0.6v-2.6C-379,1222.1-378.8,1221.8-378.5,1221.8zM-378.5,1230.2c-0.7,0-1.4-0.2-2-0.5c-1.9-1.1-2.6-3.6-1.5-5.5c0.4-0.6,0.9-1.1,1.5-1.5c0.3-0.2,0.6-0.1,0.8,0.2c0.2,0.3,0.1,0.6-0.2,0.8c-0.4,0.3-0.8,0.6-1.1,1.1c-0.8,1.4-0.3,3.2,1.1,4c0.7,0.4,1.5,0.5,2.2,0.3c0.8-0.2,1.4-0.7,1.8-1.4c0.4-0.7,0.5-1.5,0.3-2.2c-0.2-0.8-0.7-1.4-1.4-1.8c-0.3-0.2-0.4-0.5-0.2-0.8c0.2-0.3,0.5-0.4,0.8-0.2c0.9,0.5,1.6,1.4,1.9,2.5s0.1,2.1-0.4,3.1c-0.5,0.9-1.4,1.6-2.5,1.9C-377.8,1230.1-378.1,1230.2-378.5,1230.2z");
            }
            else if (status)
            {
                IsEnabled = true;
                EllipseControl.Fill = new SolidColorBrush(Color.FromRgb(91, 201, 208));
                PathControl.Fill = new SolidColorBrush(Color.FromRgb(30, 36, 50));
                PathControl.Data = Geometry.Parse("M-378.5,1230.2c-0.7,0-1.4-0.2-2-0.5c-1.9-1.1-2.6-3.6-1.5-5.5c0.4-0.6,0.9-1.1,1.5-1.5c0.3-0.2,0.6-0.1,0.8,0.2c0.2,0.3,0.1,0.6-0.2,0.8c-0.4,0.3-0.8,0.6-1.1,1.1c-0.8,1.4-0.3,3.2,1.1,4c0.7,0.4,1.5,0.5,2.2,0.3c0.8-0.2,1.4-0.7,1.8-1.4c0.4-0.7,0.5-1.5,0.3-2.2c-0.2-0.8-0.7-1.4-1.4-1.8c-0.3-0.2-0.4-0.5-0.2-0.8c0.2-0.3,0.5-0.4,0.8-0.2c0.9,0.5,1.6,1.4,1.9,2.5s0.1,2.1-0.4,3.1c-0.5,0.9-1.4,1.6-2.5,1.9C-377.8,1230.1-378.1,1230.2-378.5,1230.2zM-378.5,1221.8L-378.5,1221.8c0.3,0,0.6,0.3,0.6,0.6v2.6c0,0.3-0.2,0.6-0.6,0.6l0,0c-0.3,0-0.6-0.3-0.6-0.6v-2.6C-379,1222.1-378.8,1221.8-378.5,1221.8z");
            }
            else
            {
                IsEnabled = true;
                EllipseControl.Fill = new SolidColorBrush(Color.FromRgb(30, 36, 50));
                PathControl.Fill = new SolidColorBrush(Color.FromRgb(142, 148, 161));
                PathControl.Data = Geometry.Parse("M-378.5,1221.8L-378.5,1221.8c0.3,0,0.6,0.3,0.6,0.6v2.6c0,0.3-0.2,0.6-0.6,0.6l0,0c-0.3,0-0.6-0.3-0.6-0.6v-2.6C-379,1222.1-378.8,1221.8-378.5,1221.8zM-378.5,1230.2c-0.7,0-1.4-0.2-2-0.5c-1.9-1.1-2.6-3.6-1.5-5.5c0.4-0.6,0.9-1.1,1.5-1.5c0.3-0.2,0.6-0.1,0.8,0.2c0.2,0.3,0.1,0.6-0.2,0.8c-0.4,0.3-0.8,0.6-1.1,1.1c-0.8,1.4-0.3,3.2,1.1,4c0.7,0.4,1.5,0.5,2.2,0.3c0.8-0.2,1.4-0.7,1.8-1.4c0.4-0.7,0.5-1.5,0.3-2.2c-0.2-0.8-0.7-1.4-1.4-1.8c-0.3-0.2-0.4-0.5-0.2-0.8c0.2-0.3,0.5-0.4,0.8-0.2c0.9,0.5,1.6,1.4,1.9,2.5s0.1,2.1-0.4,3.1c-0.5,0.9-1.4,1.6-2.5,1.9C-377.8,1230.1-378.1,1230.2-378.5,1230.2z");
            }
        }
    }
}
