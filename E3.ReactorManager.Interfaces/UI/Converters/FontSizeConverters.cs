using System;
using System.Globalization;
using System.Windows.Data;

namespace E3.ReactorManager.Interfaces.UI.Converters
{
    public class FontSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && double.Parse(value.ToString()) > 0)
            {
                if (parameter != null)
                {
                    return double.Parse(value.ToString()) * double.Parse(parameter.ToString());
                }
                else
                {
                    return double.Parse(value.ToString()) * 0.5;
                }
            }

            // Default Font Size
            return 12;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
