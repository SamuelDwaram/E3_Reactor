using E3Tech.RecipeBuilding.ViewModels;
using System.Collections.Generic;

namespace E3Tech.RecipeBuilding.Model
{
    public interface IMultiRecipeBuilder
    {
        Dictionary<string, RecipeBuilderViewModel> Recipes { get; }

        bool AddRecipe(string deviceId);

        void RemoveRecipe(string deviceId);

        bool IsDeviceIdValid(string deviceId);
    }
}
