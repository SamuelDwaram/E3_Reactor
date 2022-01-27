using System;
using System.Globalization;
using System.Windows.Data;

namespace E3Tech.RecipeBuilding.Converters
{
    public class RecipeBlockNameIdentifier : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }
    }
}
