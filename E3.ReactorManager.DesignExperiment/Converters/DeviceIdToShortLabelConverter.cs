using System;
using System.Globalization;
using System.Windows.Data;

namespace E3.ReactorManager.DesignExperiment.Converters
{
    public class DeviceIdToShortLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrWhiteSpace(System.Convert.ToString(value)))
            {
                return string.Empty;
            }

            string deviceId = System.Convert.ToString(value);
            return deviceId[0] + " - " + deviceId.Split('_')[1];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
