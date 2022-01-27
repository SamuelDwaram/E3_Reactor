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
    /// stringeraction logic for ProgressBarAnimation.xaml
    /// </summary>
    public partial class ProgressBarAnimation : UserControl
    {
        public ProgressBarAnimation()
        {
            InitializeComponent();
        }

        #region Positive Maximum & Negative Maximum
        public static readonly DependencyProperty PositiveMaximumValueProperty =
           DependencyProperty.Register("PositiveMaximumValue", typeof(string), typeof(ProgressBarAnimation), new
              PropertyMetadata("200", new PropertyChangedCallback(OnMaximumValueChanged)));

        public string PositiveMaximumValue
        {
            get { return (string)GetValue(PositiveMaximumValueProperty); }
            set { SetValue(PositiveMaximumValueProperty, value); }
        }

        public static readonly DependencyProperty NegativeMaximumValueProperty =
           DependencyProperty.Register("NegativeMaximumValue", typeof(string), typeof(ProgressBarAnimation), new
              PropertyMetadata("90", new PropertyChangedCallback(OnMaximumValueChanged)));

        public string NegativeMaximumValue
        {
            get { return (string)GetValue(NegativeMaximumValueProperty); }
            set { SetValue(NegativeMaximumValueProperty, value); }
        }

        private static void OnMaximumValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ProgressBarAnimation toggleButtonUserControl = d as ProgressBarAnimation;
            toggleButtonUserControl.OnMaximumValueChanged(e);
        }

        private void OnMaximumValueChanged(DependencyPropertyChangedEventArgs e)
        {
            //Do something when this property changed
            float neg = Convert.ToSingle(NegativeMaximumValue);
            float pos = Convert.ToSingle(PositiveMaximumValue);
            NegativeProgressBarArea.Width = (float)neg / (neg + pos) * 117;
            PositiveProgressBarArea.Width = (float)pos / (neg + pos) * 117;
        }
        #endregion

        #region Current Value
        public static readonly DependencyProperty CurrentValueProperty =
           DependencyProperty.Register("CurrentValue", typeof(string), typeof(ProgressBarAnimation), new
              PropertyMetadata("0", new PropertyChangedCallback(OnCurrentValueChanged)));

        public string CurrentValue
        {
            get { return (string)GetValue(CurrentValueProperty); }
            set { SetValue(CurrentValueProperty, value); }
        }

        private static void OnCurrentValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ProgressBarAnimation toggleButtonUserControl = d as ProgressBarAnimation;
            toggleButtonUserControl.OnCurrentValueChanged();
        }

        private void OnCurrentValueChanged()
        {
            float cur = Convert.ToSingle(string.IsNullOrWhiteSpace(CurrentValue) ? "0" : CurrentValue);
            float neg = Convert.ToSingle(NegativeMaximumValue);
            float pos = Convert.ToSingle(PositiveMaximumValue);
            
            if(cur >= -1 * neg && cur <= pos)
            {
                /*
                 * Update the Current Value only if the value is in between Positive Maximum and Negative maximum
                 */
                if (cur < 0)
                {
                    ProgressBarPositiveGraphic.Width = PositiveProgressBarArea.Width;
                    ProgressBarNegativeGraphic.Width = Math.Abs(cur / neg) * NegativeProgressBarArea.Width;
                }
                else if (cur > 0)
                {
                    ProgressBarPositiveGraphic.Width = Math.Abs(PositiveProgressBarArea.Width - (cur / pos * PositiveProgressBarArea.Width));
                    ProgressBarNegativeGraphic.Width = 0;
                }
                else if (cur == 0)
                {
                    ProgressBarPositiveGraphic.Width = PositiveProgressBarArea.Width;
                    ProgressBarNegativeGraphic.Width = 0;
                }
            }
            else
            {
                ProgressBarPositiveGraphic.Width = PositiveProgressBarArea.Width;
                ProgressBarNegativeGraphic.Width = 0;
            }
        }
        #endregion
    }
}
