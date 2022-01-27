using System;
using System.Globalization;
using System.Windows.Data;

namespace E3Tech.RecipeBuilding.Converters
{
    public class RecipeBlockBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                switch (value.ToString())
                {
                    case "Start":
                        return "LightSlateGray";
                    case "HeatCool":
                        return "LightBlue";
                    case "Stirrer":
                        return "LightSalmon";
                    case "Dosing":
                        return "LightCoral";
                    case "Wait":
                        return "LightPink";
                    case "Fill":
                        return "LightSteelBlue";
                    case "Transfer":
                        return "LightGoldenrodYellow";
                    case "End":
                        return "DarkCyan";
                    default:
                        return "Gray";
                }
            }

            return "Gray";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
