using E3Tech.IO.FileAccess;
using E3Tech.RecipeBuilding.Helpers;
using E3Tech.RecipeBuilding.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity;

namespace E3Tech.RecipeBuilding.RecipceExport
{
    public class RecipeFileExporter : IRecipeExporter
    {
        private readonly IFileBrowser fileBrowser;
        private readonly IUnityContainer unityContainer;

        public RecipeFileExporter(IUnityContainer unityContainer, IFileBrowser fileBrowser)
        {
            this.fileBrowser = fileBrowser;
            this.unityContainer = unityContainer;
        }

        public void Export(IList<RecipeStep> recipe)
        {
            string fileName = fileBrowser.SaveFile("Recipe-1", ".json");

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.NullValueHandling = NullValueHandling.Ignore;
                settings.Converters.Add(new BlockCreationConverter<ParameterizedRecipeBlock<Object>>(unityContainer));
                File.WriteAllText(fileName, JsonConvert.SerializeObject(recipe, settings));
            }
        }

        public void SaveSeqRecipe(IList<SeqRecipeModel> SeqRecipeList)
        {
            string filePath = fileBrowser.SaveFile("SeqRecipeList-1", ".csv");
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                StringBuilder csv = new StringBuilder();
                foreach (var recipe in SeqRecipeList)
                {
                    string line = string.Format("{0},{1},{2}", recipe.RecipeName, recipe.RecipeGuidId, recipe.FileLocation);
                    csv.AppendLine(line);
                }

                File.WriteAllText(filePath, csv.ToString());
            }
        }

        public void SaveSeqRecipeWhileExecuting(IList<SeqRecipeModel> SeqRecipeList)
        {
            string filePath = fileBrowser.GetDefaultReceipeFileDirectiory() + "ExecutingRecipeList.csv";
            if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
            {
                StringBuilder csv = new StringBuilder();
                foreach (var recipe in SeqRecipeList)
                {
                    string line = string.Format("{0},{1},{2},{3},{4}", recipe.RecipeName, recipe.RecipeGuidId, recipe.FileLocation, recipe.IsExecuting, recipe.IsExecuted);
                    csv.AppendLine(line);
                }

                File.WriteAllText(filePath, csv.ToString());
            }
        }

        public void DeleteSeqRecipe()
        {
            string filePath = fileBrowser.GetDefaultReceipeFileDirectiory() + "ExecutingRecipeList.csv";
            if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
