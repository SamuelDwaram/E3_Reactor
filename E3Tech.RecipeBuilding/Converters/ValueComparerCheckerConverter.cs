using System;
using System.Globalization;
using System.Windows.Data;

namespace E3Tech.RecipeBuilding.Converters
{
    public class ValueComparerCheckerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && parameter != null)
            {
                if (value.ToString() == parameter.ToString())
                {
                    return bool.TrueString;
                }
            }

            return bool.FalseString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }
    }
}
