using E3.ReactorManager.Interfaces.DesignExperiment.Data;
using System.Collections.Generic;
using System.Data;
using System.Windows.Media.Imaging;

namespace E3Tech.RecipeBuilding.Model
{
    public interface IRecipeReportPrinter
    {
        void PrintReport(IList<DataTable> tables);

        void PrintReport(IList<RecipeStep> recipe);

        void PrintReport(IList<RecipeStep> recipe, Batch batch);

        void PrintReport(IList<RecipeStep> recipe, Batch batch, RenderTargetBitmap renderTargetBitmap);
    }
}
