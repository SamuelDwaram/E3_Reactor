using E3Tech.RecipeBuilding.ParameterProviders;
using Prism.Ioc;
using System;
using System.ComponentModel;
using System.Linq;
using Unity;

namespace E3Tech.RecipeBuilding.Model
{
    public class ParameterizedRecipeBlock<T> : IParameterizedRecipeBlock<T> where T : class
    {
        private static string name;
        private static string uiLabel;
        private string guidId;

        public ParameterizedRecipeBlock()
        {
            Parameters = Activator.CreateInstance<T>();
            GuidID = Guid.NewGuid().ToString();
        }

        public override string ToString()
        {
            return this.Name;
        }

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateParameterValue(string parameterName, string parameterValue)
        {
            if (Parameters.GetType().GetProperties().Any(property => property.Name == parameterName))
            {
                Parameters.GetType().GetProperty(parameterName).SetValue(Parameters, parameterValue, null);
            }
        }

        public bool Configure(IUnityContainer containerProvider)
        {
            BlockParameterProvider<T> recipeBlockParemeterProvider = containerProvider.Resolve<BlockParameterProvider<T>>();
            recipeBlockParemeterProvider.Parameters = Parameters;
            bool result = recipeBlockParemeterProvider.PopulateParameters();
            if (result)
            {
                this.Parameters = recipeBlockParemeterProvider.Parameters;
            }
            return result;
        }

        public string GetParameterValue(string parameterName)
        {
            if (Parameters.GetType().GetProperties().Any(property => property.Name == parameterName))
            {
                return Parameters.GetType().GetProperty(parameterName).GetValue(Parameters, null).ToString();
            }

            return string.Empty;
        }

        private T parameters;
        public T Parameters
        {
            get => parameters; private set
            {
                parameters = value;
                RaisePropertyChanged("Parameters");
            }
        }

        public string Name
        {
            get => name ?? GetParameterValue("Name");
            set
            {
                name = value;
                RaisePropertyChanged("Name");
            }
        }

        public string UiLabel
        {
            get => uiLabel ?? GetParameterValue("UiLabel");
            set
            {
                uiLabel = value;
                RaisePropertyChanged("UiLabel");
            }
        }

        public string GuidID
        {
            get => guidId ?? GetParameterValue(nameof(GuidID));
            set
            {
                guidId = value;
                RaisePropertyChanged(nameof(GuidID));
            }
        }

        private int index;
        public int Index
        {
            get => index;
            set
            {
                index = value;
                RaisePropertyChanged(nameof(index));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
