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
    /// Interaction logic for StripedAnimation.xaml
    /// </summary>
    public partial class StripedAnimation : UserControl
    {
        public StripedAnimation()
        {
            InitializeComponent();
            BlackColourPoint.Point = new Point(0, 0);
        }

        public static readonly DependencyProperty MaximumValueProperty =
           DependencyProperty.Register("MaximumValue", typeof(string), typeof(StripedAnimation), new
              PropertyMetadata("200", new PropertyChangedCallback(OnValueChanged)));

        public string MaximumValue
        {
            get { return (string)GetValue(MaximumValueProperty); }
            set { SetValue(MaximumValueProperty, value); }
        }

        public static readonly DependencyProperty CurrentValueProperty =
           DependencyProperty.Register("CurrentValue", typeof(string), typeof(StripedAnimation), new
              PropertyMetadata("0", new PropertyChangedCallback(OnValueChanged)));

        public string CurrentValue
        {
            get { return (string)GetValue(CurrentValueProperty); }
            set { SetValue(CurrentValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StripedAnimation toggleButtonUserControl = d as StripedAnimation;
            toggleButtonUserControl.OnValueChanged(e);
        }

        private void OnValueChanged(DependencyPropertyChangedEventArgs e)
        {
            float cur = Convert.ToSingle(string.IsNullOrWhiteSpace(CurrentValue) ? "0" : CurrentValue);
            float max = Convert.ToSingle(MaximumValue);
            if (cur <= max && cur >= 0)
            {
                /*
                 * Update the BlackColourPoint.Point value only if the Current Value is in between 0 and Maximum Value
                 */
                BlackColourPoint.Point = new Point(cur / max * 77.5, 0);
            }
            else
            {
                BlackColourPoint.Point = new Point(0, 0);
            }
        }
    }
}
