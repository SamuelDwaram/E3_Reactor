using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Unity;

namespace E3Tech.RecipeBuilding.Converters
{
    internal class BlockParameterProviderTemplateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            string templateName = value.GetType().Name.ToString() + "Template";

            IUnityContainer unityContainer = null;
            // keep disabled for now.
            //unityContainer = (IUnityContainer)parameter;
            if (unityContainer == null)
            {
                // load corresponding view of the block parameter being converted from the embedded resource dictionary.
                ResourceDictionary resourceDictionary = new ResourceDictionary
                {
                    Source =
                    new Uri("/E3Tech.RecipeBuilding;component/ParameterProviders/BlockParametersTemplateDictionary.xaml",
                            UriKind.RelativeOrAbsolute)
                };

                object resource = resourceDictionary[templateName];
                return resource;
            }
            else
            {
                // resolve an interface that provides the corresponding view of the block parameter being converted.
                //container.Resolve();
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
