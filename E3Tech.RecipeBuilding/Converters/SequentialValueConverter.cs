using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;

namespace E3Tech.RecipeBuilding.Converters
{
    public class SequentialValueConverter : Collection<IValueConverter>, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object returnValue = value;
            foreach (IValueConverter converter in this)
            {
                returnValue = converter.Convert(returnValue, targetType, parameter, culture);
            }

            return returnValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
