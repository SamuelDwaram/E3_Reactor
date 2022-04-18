using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace E3Tech.RecipeBuilding.Converters
{
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
           System.Globalization.CultureInfo culture)
        {
            bool result = false;
            if (targetType != typeof(bool) && bool.TryParse(value.ToString(),out result) == false)
                throw new InvalidOperationException("The target must be a boolean");

            return result == false;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            bool result = false;
            if (targetType != typeof(bool) && bool.TryParse(value.ToString(), out result) == false)
                throw new InvalidOperationException("The target must be a boolean");

            return result == false;
        }
    }
}
