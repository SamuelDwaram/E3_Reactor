using E3Tech.RecipeBuilding.Model;
using System.Collections.Generic;
using Unity;
using System.Windows.Input;
using Prism.Commands;
using System.Linq;
using System;
using Prism.Mvvm;
using Prism.Regions;
using System.Windows;

namespace E3Tech.RecipeBuilding.ViewModels
{
    public class MultiRecipeBuilderViewModel : BindableBase
    {
        private readonly IUnityContainer containerProvider;
        private readonly IRecipesManager recipesManager;
        private readonly IMultiRecipeBuilder multiRecipeBuilder;
        IRegionManager regionManager;

        public int SelectedTabIndex { get; set; }

        public MultiRecipeBuilderViewModel(IUnityContainer containerProvider, IRegionManager regionManager)
        {
            this.regionManager = regionManager;
            this.containerProvider = containerProvider;
            this.multiRecipeBuilder = containerProvider.Resolve<IMultiRecipeBuilder>();
            this.recipesManager = containerProvider.Resolve<IRecipesManager>();
            Recipes = new Dictionary<string, RecipeBuilderViewModel>();

            UpdateNavigationParameters();
            AddNewRecipesInRecipesManagerToMultiRecipeBuilder();
            RemoveExtraRecipesInMultiRecipeBuilder();
            LoadRecipes();
            UpdateFirstTabFieldDeviceParameters();
        }

        #region Commands
        private ICommand _tabItemLoadedCommand;
        public ICommand TabItemLoadedCommand
        {
            get => _tabItemLoadedCommand ?? (_tabItemLoadedCommand = new DelegateCommand(new Action(TabItemLoaded)));
            set => _tabItemLoadedCommand = value;
        }

        public void TabItemLoaded()
        {
            Recipes.ToList()[SelectedTabIndex].Value.UpdateRecipeParameters();
            Recipes.ToList()[SelectedTabIndex].Value.UpdateNavigationParameters(NavigationParameters);
        }
        #endregion

        public void UpdateNavigationParameters()
        {
            NavigationParameters = new NavigationParameters();
        }

        private void RemoveExtraRecipesInMultiRecipeBuilder()
        {
            /* remove extra recipes in the MultiRecipeBuilder which are not present in RecipeManager */
            foreach (var keyValuePair in multiRecipeBuilder.Recipes.ToList())
            {
                if (recipesManager.DevicesRunningRecipe.Contains(keyValuePair.Key))
                {
                    /* Keep it */
                }
                else
                {
                    /* remove it from MultiRecipeBuilder */
                    multiRecipeBuilder.RemoveRecipe(keyValuePair.Key);
                }
            }
        }

        private void AddNewRecipesInRecipesManagerToMultiRecipeBuilder()
        {
            foreach (var deviceId in recipesManager.DevicesRunningRecipe)
            {
                /* 
                 * Check if there are any new DeviceId's in the recipesManager
                 * and try to add them to MultiRecipeBuilder.Recipes
                 */
                bool isDeviceIdValid = multiRecipeBuilder.AddRecipe(deviceId);
                if (isDeviceIdValid)
                {
                    /* New Recipe added to multiRecipeBuilder */
                }
            }
        }

        private void UpdateFirstTabFieldDeviceParameters()
        {
            /* Check for the Recipes Count */
            if (Recipes.Count > 0)
            {
                /* Update Field Device Parameters for the First tab in MultiRecipeBuilderView */
                Recipes.ToList()[0].Value.UpdateRecipeParameters();
                Recipes.ToList()[0].Value.UpdateNavigationParameters(NavigationParameters);
            }
        }

        private void LoadRecipes()
        {
            /*
             * This function is useful when user navigates to some other page and came back
             */
            foreach (var recipe in multiRecipeBuilder.Recipes)
            {
                Recipes.Add(recipe.Key, recipe.Value);
            }
        }

        public Dictionary<string, RecipeBuilderViewModel> Recipes { get; }

        private NavigationParameters _navigationParameters;
        public NavigationParameters NavigationParameters
        {
            get => _navigationParameters ?? (_navigationParameters = new NavigationParameters());
            set
            {
                _navigationParameters = value;
                RaisePropertyChanged();
            }
        }
    }
}
