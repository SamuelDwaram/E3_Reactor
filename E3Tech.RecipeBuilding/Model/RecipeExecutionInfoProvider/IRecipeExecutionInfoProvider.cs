using System.Collections.Generic;

namespace E3Tech.RecipeBuilding.Model.RecipeExecutionInfoProvider
{
    public interface IRecipeExecutionInfoProvider
    {
        void SaveRecipeExecutionInfo(IList<RecipeStep> recipeSteps, string deviceId);
    }
}
