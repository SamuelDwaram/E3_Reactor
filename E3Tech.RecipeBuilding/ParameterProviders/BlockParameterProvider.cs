using E3Tech.RecipeBuilding.Helpers;
using E3Tech.RecipeBuilding.Model;
using E3Tech.RecipeBuilding.Model.Blocks;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            if (view.ViewModel.Parameters is BaseBlockParameters)
            {
                var baseBlockParameter = view.ViewModel.Parameters as BaseBlockParameters;
                baseBlockParameter.OnSourceChanged += OnSourceChanged;
            }
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

        private void OnSourceChanged(object sender, SourceChangesEventErgs e)
        {
            if (e != null)
            {
                List<string> destinationList = SourceDestinationMapping.SourceDestinationPair[e.Source];
                if (sender is BaseBlockParameters)
                {
                    var convertedSender = sender as BaseBlockParameters;
                    convertedSender.FilterDestination = new ObservableCollection<string>(destinationList);
                }
            }
        }

        public T Parameters { get => parameters; set => parameters = value; }
    }
}
