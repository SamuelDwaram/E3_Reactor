using System;
using System.Globalization;
using System.Windows.Data;

namespace E3Tech.RecipeBuilding.Converters
{
    public class BlockParameterProviderViewTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? value.ToString() + " Block Parameters" : "Block Parameters";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
