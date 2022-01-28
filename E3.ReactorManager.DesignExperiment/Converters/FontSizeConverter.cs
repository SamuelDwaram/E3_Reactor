using System;
using System.Globalization;
using System.Windows.Data;

namespace E3.ReactorManager.DesignExperiment.Converters
{
    public class FontSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && System.Convert.ToInt32(value) != 0)
            {
                return System.Convert.ToInt32(value) * 0.5;
            }

            //return default font size=12(just assumption)
            return 12;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
