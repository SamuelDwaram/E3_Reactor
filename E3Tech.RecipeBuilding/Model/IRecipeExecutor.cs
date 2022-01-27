using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E3Tech.RecipeBuilding.Model
{
    public interface IRecipeExecutor
    {
        bool GetRecipeStatus(string deviceId);

        void Execute(string deviceId, IList<RecipeStep> recipeSteps);

        void UpdateBlock(int stepIndex, IRecipeBlock block, string deviceId);

        void AbortBlockExecution(int stepIndex, IRecipeBlock block, string deviceId);

        void EditBlockExecution(string deviceId);

        void SaveUpdatedBlockExecution(string devideId);

        void ClearRecipe(string deviceId);

        void AbortRecipeExecution(string deviceId);

        event UpdateRecipe UpdateRecipe;
    }

    public delegate void UpdateRecipe(string deviceId, int stepIndex, RecipeStep recipeStep);
}
