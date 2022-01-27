using System.Collections.Generic;

namespace E3Tech.RecipeBuilding.Model
{
    public interface IRecipeExporter
    {
        void Export(IList<RecipeStep> recipe);

        void SaveSeqRecipe(IList<SeqRecipeModel> seqRecipeList);

        void SaveSeqRecipeWhileExecuting(IList<SeqRecipeModel> SeqRecipeList);
        void DeleteSeqRecipe();
    }
}
