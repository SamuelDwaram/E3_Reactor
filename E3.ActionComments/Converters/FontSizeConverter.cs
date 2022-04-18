using System;
using System.Globalization;
using System.Windows.Data;

namespace E3.ActionComments.Converters
{
    public class FontSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && System.Convert.ToInt32(value) > 0 )
            {
                return System.Convert.ToInt32(value) * 0.5;
            }

            //Return Default Font Size Converter
            return 12;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
