using E3.UserManager.Model.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Anathem.Ui.Converter
{
    public class RoleCheckConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && parameter != null)
            {
                if (value is IList<Role>)
                {
                    IList<Role> Values = value as IList<Role>;
                    if (Values.Count > 0)
                    {
                        bool result = Values.First().ModulesAccessable.Any(x => x.Trim() == parameter.ToString().Trim());
                        return result;
                    }
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
