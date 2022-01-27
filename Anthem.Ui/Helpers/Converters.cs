using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Anathem.Ui.Helpers
{
    public class ParameterExtractorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null || string.IsNullOrWhiteSpace(parameter.ToString()))
            {
                return string.Empty;
            }

            Dictionary<string, string> dict = value as Dictionary<string, string>;
            string fieldPoint = parameter.ToString();
            return dict.ContainsKey(fieldPoint) ? dict[fieldPoint] : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
