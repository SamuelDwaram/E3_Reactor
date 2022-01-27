using E3Tech.RecipeBuilding.Model.Blocks;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace E3Tech.RecipeBuilding.Model
{
    public class RecipeRulesValidator : IRecipeRulesValidator
    {
        public bool ValidateAddingNewStep(RecipeStep[] recipeSteps, IRecipeBlock newBlock, int blockIndex)
        {
            if (recipeSteps.Count() == 0)
            {
                // prevent adding any block other than Start to the first step.
                return newBlock is IParameterizedRecipeBlock<StartBlockParameters>;
            }
            else
            {
                // prevent adding a new step if the last block was End.
                if (recipeSteps.Last().BlockOne?.Name == "End")
                {
                    return false;
                }
                // allow adding any block other than Start to the new step.
                return !(newBlock is IParameterizedRecipeBlock<StartBlockParameters>);
            }
        }

        public bool ValidateAddingNewStepWhileRunningRecipe(RecipeStep[] recipeSteps, RecipeStep currentRecipeStep, RecipeStep newRecipeStep, IRecipeBlock newBlock, int currentRecipeStepsCount, int toBeAddedStepIndex)
        {
            if (newBlock.Name == "Start")
            {
                //prevent adding start block while running recipe
                return false;
            }
            else if (newBlock.Name == "End")
            {
                //prevent adding End block if recipe already contains End Block
                if (recipeSteps.Last().BlockOne?.Name == "End")
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else if (currentRecipeStepsCount > toBeAddedStepIndex)
            {
                // new step is added between First and last recipe steps
                return true;
            }
            else
            {
                if (currentRecipeStep.BlockOne?.Name == "End")
                {
                    //prevent adding a new step after step containing End block
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public bool ValidateAddingNewBlockToStep(RecipeStep recipeStep, IRecipeBlock block, int blockIndex)
        {
            if (recipeStep.BlockOne == null)
            {
                return true;
            }
            // prevent adding new blocks to the start step.
            if (recipeStep.BlockOne?.Name == "Start")
            {
                return false;
            }
            //check if start block is added to the existing recipe step
            else if (block?.Name == "Start")
            {
                return false;
            }
            // prevent adding new blocks to the End step.
            if (recipeStep.BlockOne?.Name == "End")
            {
                return false;
            }
            //check if end block is added to the existing recipe step
            else if (block?.Name == "End")
            {
                return false;
            }
            // prevent duplicating a block in the same step.
            if (!CheckIfBlockExistInStep(recipeStep, block))
            {
                return false;
            }
            // prevent overriding a block.
            switch (blockIndex)
            {

                case 0:
                    return recipeStep.BlockOne == null;
                default:
                    return false;
            }
        }

        private bool CheckIfBlockExistInStep(RecipeStep recipeStep, IRecipeBlock block)
        {
            // at some point in time, some of the blocks in a step may be empty thus the check for null before accessing.
            return recipeStep.BlockOne?.Name != block.Name;
        }

        public bool ValidateDelete(List<RecipeStep> recipeSteps, RecipeStep step)
        {
            if (recipeSteps.Count > 1 && recipeSteps.IndexOf(step) == 0)
            {
                return false;
            }

            return true;
        }

        public bool ValidateDeleteBlock(RecipeStep step, IRecipeBlock block)
        {
            if (step.BlockOne?.Name == "Start")
            {
                return false;
            }

            return true;
        }

        public bool CheckEndBlockInRecipe(IList<RecipeStep> recipeSteps)
        {
            if (recipeSteps.Count < 1)
            {
                return false;
            }

            RecipeStep lastStep = recipeSteps[recipeSteps.Count - 1];

            if (lastStep.BlockOne.Name == "End")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckAllSequenceRecipeExist(IList<SeqRecipeModel> seqRecipeModels, out IList<string> fileNotFound)
        {
            fileNotFound = new List<string>();
            if (seqRecipeModels.Count > 1)
            {
                foreach (var item in seqRecipeModels)
                {
                    if (File.Exists(item.FileLocation) == false)
                    {
                        fileNotFound.Add(item.RecipeName);
                    }
                }
            }
            if (fileNotFound.Count > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
