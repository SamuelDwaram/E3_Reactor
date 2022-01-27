using E3Tech.IO.FileAccess;
using E3Tech.RecipeBuilding.Helpers;
using E3Tech.RecipeBuilding.Model;
using E3Tech.RecipeBuilding.Model.Blocks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Unity;

namespace E3Tech.RecipeBuilding.RecipeImport
{
    public class RecipeFileImporter : IRecipeImporter
    {
        private readonly IFileBrowser fileBrowser;
        private readonly IUnityContainer unityContainer;

        public RecipeFileImporter(IUnityContainer unityContainer, IFileBrowser fileBrowser)
        {
            this.unityContainer = unityContainer;
            this.fileBrowser = fileBrowser;
        }

        public IList<RecipeStep> Import()
        {
            string fileName = fileBrowser.OpenFile(".json");

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                return ImportJsonFile(fileName);
            }

            return null;
        }

        public Dictionary<SeqRecipeModel,IList<RecipeStep>> ImportRecipeFromSeqList(IList<SeqRecipeModel> seqRecipeModels)
        {
            Dictionary<SeqRecipeModel, IList<RecipeStep>> SeqRecipeListPair = new Dictionary<SeqRecipeModel, IList<RecipeStep>>();
            foreach(var seqRecipeModel in seqRecipeModels)
            {
                if (string.IsNullOrWhiteSpace(seqRecipeModel.FileLocation) == false)
                {
                    seqRecipeModel.IsExecuted = seqRecipeModel.IsExecuted;
                    seqRecipeModel.IsExecuting = seqRecipeModel.IsExecuting;
                    IList<RecipeStep> recipeList = ImportJsonFile(seqRecipeModel.FileLocation);
                    SeqRecipeListPair.Add(seqRecipeModel, recipeList);
                }
            }
            return SeqRecipeListPair;
        }

        private IList<RecipeStep> ImportJsonFile(string fileName)
        { 
            StreamReader sr = new StreamReader(fileName);
            string json = sr.ReadToEnd();
            var settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.Converters.Add(new BlockCreationConverter<ParameterizedRecipeBlock<Object>>(unityContainer));
            return JsonConvert.DeserializeObject<IList<RecipeStep>>(json, settings);
        }

        public IList<SeqRecipeModel> ImportSeqList()
        {
            string filePath = fileBrowser.OpenFile(".csv");
            IList<SeqRecipeModel> seqRecipeModels = null;
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                string seqRecipeData = File.ReadAllText(filePath);
                seqRecipeModels = new List<SeqRecipeModel>();
                //Execute a loop over the rows.
                foreach (string recipeData in seqRecipeData.Split('\n'))
                {
                    if (!string.IsNullOrEmpty(recipeData))
                    {
                        SeqRecipeModel seqRecipeModel = new SeqRecipeModel();
                        string[] data = recipeData.Split(',');
                        if (data.Length == 3)
                        {
                            seqRecipeModel.RecipeName = data[0];
                            seqRecipeModel.RecipeGuidId = Guid.Parse(data[1]);
                            seqRecipeModel.FileLocation = data[2].Replace(".json\r", ".json");
                            seqRecipeModels.Add(seqRecipeModel);
                        }
                    }
                }
            }

            return seqRecipeModels;
        }

        

        public List<SeqRecipeModel> ReloadSeqRecipes()
        {
            List<SeqRecipeModel> seqRecipeModels = null;
            string filePath = fileBrowser.GetDefaultReceipeFileDirectiory() + "ExecutingRecipeList.csv";
            if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
            {
                string seqRecipeData = File.ReadAllText(filePath);
                seqRecipeModels = new List<SeqRecipeModel>();
                //Execute a loop over the rows.
                foreach (string recipeData in seqRecipeData.Split('\n'))
                {
                    if (!string.IsNullOrEmpty(recipeData))
                    {
                        SeqRecipeModel seqRecipeModel = new SeqRecipeModel();
                        string[] data = recipeData.Split(',');
                        if (data.Length == 5)
                        {
                            seqRecipeModel.RecipeName = data[0];
                            seqRecipeModel.RecipeGuidId = Guid.Parse(data[1]);
                            seqRecipeModel.FileLocation = data[2].Replace(".json\r", ".json");
                            seqRecipeModel.IsExecuting = bool.Parse(data[3]);
                            seqRecipeModel.IsExecuted = bool.Parse(data[4]);

                            seqRecipeModels.Add(seqRecipeModel);
                        }
                    }
                }
            }
            return seqRecipeModels;
        }

        public string ImportWithFileName(out string fileName)
        {
            fileName = fileBrowser.OpenFile(".json");
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                string[] stringlist = fileName.Split('\\');
                string appdataFileName = fileBrowser.GetDefaultReceipeFileDirectiory() + stringlist[stringlist.Length - 1];
                if (File.Exists(appdataFileName) == false)
                {
                    File.Copy(fileName, appdataFileName);
                }

                return stringlist[stringlist.Length - 1].Replace(".json", string.Empty);
            }
            return null;
        }
    }
}
