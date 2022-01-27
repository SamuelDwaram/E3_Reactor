using E3Tech.RecipeBuilding.Model;
using Prism.Ioc;
using System;
using Unity;

namespace E3Tech.RecipeBuilding.ParameterProviders
{
    public class BlockParameterProvider<T> : IRecipeBlockParemeterProvider<T> where T : class
    {
        private readonly IUnityContainer containerProvider;
        private T parameters;

        public BlockParameterProvider(IUnityContainer containerProvider)
        {
            this.containerProvider = containerProvider;
            Parameters = Activator.CreateInstance<T>();
        }

        public bool PopulateParameters()
        {
            BlockParameterProviderView view = containerProvider.Resolve<BlockParameterProviderView>();
            view.ViewModel.Parameters = ((ICloneable)Parameters).Clone() as T;

            //Don't show BlockParameterProviderView for End Recipe block
            if ((view.ViewModel.Parameters.GetType().GetProperty("Name").GetValue(view.ViewModel.Parameters, null).ToString() == "End")
                || (view.ViewModel.Parameters.GetType().GetProperty("Name").GetValue(view.ViewModel.Parameters, null).ToString() == "Start"))
            {
                return true;
            }
            else
            {
                bool result = view.ShowDialog().Value;
                if (result)
                {
                    Parameters = (T)view.ViewModel.Parameters;
                }
                return result;
            }
        }


        public T Parameters { get => parameters; set => parameters = value; }
    }
}
