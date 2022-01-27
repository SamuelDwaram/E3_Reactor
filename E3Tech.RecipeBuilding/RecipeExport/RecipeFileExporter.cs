using E3Tech.IO.FileAccess;
using E3Tech.RecipeBuilding.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace E3Tech.RecipeBuilding.RecipceExport
{
    public class RecipeFileExporter : IRecipeExporter
    {
        private readonly IFileBrowser fileBrowser;

        public RecipeFileExporter(IFileBrowser fileBrowser)
        {
            this.fileBrowser = fileBrowser;
        }

        public void Export(IList<RecipeStep> recipe)
        {
            string fileName = fileBrowser.SaveFile("Recipe1", ".json");
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                JsonSerializer serializer = new JsonSerializer();
                StreamWriter sw = new StreamWriter(fileName);
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, recipe);
                }
            }
        }
    }
}
