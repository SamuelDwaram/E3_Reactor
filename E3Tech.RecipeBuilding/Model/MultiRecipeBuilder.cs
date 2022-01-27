using E3Tech.RecipeBuilding.ViewModels;
using System.Collections.Generic;
using Unity;

namespace E3Tech.RecipeBuilding.Model
{
    public class MultiRecipeBuilder : IMultiRecipeBuilder
    {
        private IUnityContainer containerProvider;
        private Dictionary<string, RecipeBuilderViewModel> recipes;

        public MultiRecipeBuilder(IUnityContainer containerProvider)
        {
            this.containerProvider = containerProvider;
            recipes = new Dictionary<string, RecipeBuilderViewModel>();
        }

        public bool AddRecipe(string deviceId)
        {
            if (IsDeviceIdValid(deviceId))
            {
                var vm = containerProvider.Resolve<RecipeBuilderViewModel>();
                vm.SetDeviceId(deviceId);
                recipes.Add(deviceId, vm);

                return true;
            }

            return false;
        }

        public bool IsDeviceIdValid(string deviceId)
        {
            if (recipes.ContainsKey(deviceId))
            {
                /*
                 * If DeviceId already exists in the Recipes then it is considered as invalid
                 * because RecipeBuilderViewModel was already created for that DeviceId
                 */
                return false;
            }

            return true;
        }

        public void RemoveRecipe(string deviceId)
        {
            if (recipes.ContainsKey(deviceId))
            {
                /*
                 * check if recipes contain deviceId and remove it
                 */
                recipes.Remove(deviceId);
            }
        }

        public Dictionary<string, RecipeBuilderViewModel> Recipes => recipes;
    }
}
