using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E3Tech.RecipeBuilding.Model
{
    interface IRecipeRulesValidator
    {
        bool ValidateAddingNewStep(RecipeStep[] recipeSteps, IRecipeBlock newBlock, int blockIndex);

        bool ValidateAddingNewStepWhileRunningRecipe(RecipeStep[] recipeSteps, RecipeStep currentRecipeStep, RecipeStep newRecipeStep, IRecipeBlock newBlock, int currentRecipeStepsCount, int toBeAddedStepIndex);

        bool ValidateAddingNewBlockToStep(RecipeStep recipeStep, IRecipeBlock block, int blockIndex);

        bool ValidateDelete(List<RecipeStep> recipeSteps, RecipeStep step);

        bool ValidateDeleteBlock(RecipeStep step, IRecipeBlock block);

        bool CheckEndBlockInRecipe(IList<RecipeStep> recipeSteps);

        bool CheckAllSequenceRecipeExist(IList<SeqRecipeModel> seqRecipeModels,out IList<string> fileNotFound);
    }
}
