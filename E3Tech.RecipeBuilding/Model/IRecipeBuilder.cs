using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace E3Tech.RecipeBuilding.Model
{
    public interface IRecipeBuilder
    {
        RecipeStep[] RecipeSteps { get; }

        string DeviceId { get; set; }

        bool UpdateStep(RecipeStep recipeStep, int blockIndex, IRecipeBlock block);        

        void Export();

        IList<RecipeStep> ReturnRecipeSteps();

        bool AddNewStep(RecipeStep step, IRecipeBlock recipeBlock);

        void AddNewBlankStep(RecipeStep Step, int index);

        bool AddNewStepWhileRunningRecipe(RecipeStep currentRecipeStep, RecipeStep newRecipeStep, IRecipeBlock block, int toBeAddedStepIndex);

        bool RemoveStep(RecipeStep step);

        void RemoveBlockFromStep(RecipeStep step, IRecipeBlock obj);

        bool CheckEndBlockInRecipe(IList<RecipeStep> recipeSteps);

        void Clear();

        bool CheckIfRecipeStepContainsAnyExecutingBlock(RecipeStep recipeStep);

        RecipeStep[] Import();

        SeqRecipeModel ImportWithFile();

        void SaveSeqRecipeWhileExecuting(IList<SeqRecipeModel> SeqRecipeList);

        void SaveSeqRecipe();

        bool DeleteRecipeFromSeq(SeqRecipeModel seqRecipeModel);

        SeqRecipeModel InsertRecipeInSeq(int index);

        void ReloadRecipeSteps(Action<Task> action, TaskScheduler taskScheduler);

        List<SeqRecipeModel> ImportSeqReciepe();

        void ClearSeqRecipe();

        bool ValidateSeqRecipe();

        void UpdateRecipeList(IList<RecipeStep> recipeSteps);

        Dictionary<SeqRecipeModel, IList<RecipeStep>> LoadSeqRecipeList();

        List<SeqRecipeModel> ReloadSeqRecipes();
        void DeleteSeqRecipe();
    }
}
