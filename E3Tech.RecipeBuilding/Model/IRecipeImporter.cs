using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E3Tech.RecipeBuilding.Model
{
    public interface IRecipeImporter
    {
        IList<RecipeStep> Import();

        IList<SeqRecipeModel> ImportSeqList();

        string ImportWithFileName(out string fileName);

        Dictionary<SeqRecipeModel, IList<RecipeStep>> ImportRecipeFromSeqList(IList<SeqRecipeModel> seqRecipeModels);
        List<SeqRecipeModel> ReloadSeqRecipes();
    }
}
