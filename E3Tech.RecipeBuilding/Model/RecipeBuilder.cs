using E3Tech.RecipeBuilding.Model.Blocks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace E3Tech.RecipeBuilding.Model
{
    internal class RecipeBuilder : IRecipeBuilder, INotifyPropertyChanged
    {
        private IRecipeExporter recipeExporter;
        private IRecipeImporter recipeImporter;
        private readonly IRecipeRulesValidator recipeRulesValidator;
        private readonly IRecipeRefresher recipeRefresher;
        private readonly IRecipeReloader recipeReloader;
        private List<RecipeStep> recipeSteps;
        private List<SeqRecipeModel> SeqRecipeList;

        public RecipeBuilder(IRecipeRulesValidator recipeRulesValidator, IRecipeExporter recipeExporter, IRecipeImporter recipeImporter, IRecipeRefresher recipeRefresher, IRecipeReloader recipeReloader)
        {
            this.recipeExporter = recipeExporter;
            this.recipeImporter = recipeImporter;
            this.recipeRulesValidator = recipeRulesValidator;
            this.recipeRefresher = recipeRefresher;
            this.recipeReloader = recipeReloader;
            this.recipeRefresher.RefreshBlockRecieved += RecipeRefresher_RefreshBlock;
            recipeSteps = new List<RecipeStep>();
            SeqRecipeList = new List<SeqRecipeModel>();
        }

        private void UpdateRecipeSteps(Task<IList<RecipeStep>> task)
        {
            if (task.IsCompleted)
            {
                recipeSteps = new List<RecipeStep>(task.Result);
            }
            else
            {
                if (task.IsFaulted)
                {
                    // display error to user
                }
            }
        }

        #region Recipe Refreshers
        private void RecipeRefresher_RefreshBlock(object sender, RefreshBlockEventArgs e)
        {
            if (e.Id == DeviceId)
            {
                RefreshBlock(e.StepIndex, e.BlockName, e.ParameterName, e.ParameterValue);
            }
        }

        public void RefreshBlock(int stepIndex, string blockName, string parameterName, string paremeterValue)
        {
            if (recipeSteps.Count < 1 || recipeSteps.Count <= stepIndex)
            {
                return;
            }

            IRecipeBlock block = null;
            block = GetBlockByName(blockName, recipeSteps[stepIndex], block);

            if (block != null)
            {
                block.UpdateParameterValue(parameterName, paremeterValue);
            }
        }

        private static IRecipeBlock GetBlockByName(string blockName, RecipeStep updateStep, IRecipeBlock block)
        {
            if (updateStep.BlockOne != null && updateStep.BlockOne.Name == blockName)
            {
                block = updateStep.BlockOne;
            }

            return block;
        }

        #endregion

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool UpdateStep(RecipeStep recipeStep, int blockIndex, IRecipeBlock block)
        {
            if (recipeRulesValidator.ValidateAddingNewBlockToStep(recipeStep, block, blockIndex))
            {
                switch (blockIndex)
                {
                    case 0:
                    default:
                        recipeStep.BlockOne = block;
                        break;
                }
                return true;
            }
            return false;
        }

        public void Export()
        {
            recipeExporter.Export(this.RecipeSteps);
        }

        public bool ValidateSeqRecipe()
        {
            IList<string> fileNotFound;
            bool result = recipeRulesValidator.CheckAllSequenceRecipeExist(SeqRecipeList, out fileNotFound);
            if (result == false)
            {
                MessageBox.Show(string.Format("Following recipe Not found: {0}", string.Join(",", fileNotFound)), "File not Found", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return result;
        }

        public void UpdateRecipeList(IList<RecipeStep> recipeSteps)
        {
            this.recipeSteps.Clear();
            this.recipeSteps.AddRange(recipeSteps);
        }

        public Dictionary<SeqRecipeModel, IList<RecipeStep>> LoadSeqRecipeList()
        {
            return recipeImporter.ImportRecipeFromSeqList(SeqRecipeList);
        }

        public List<SeqRecipeModel> ReloadSeqRecipes()
        {
            List<SeqRecipeModel> seqRecipeModels = recipeImporter.ReloadSeqRecipes();
            if (seqRecipeModels != null)
            {
                SeqRecipeList = seqRecipeModels;
            }
            return seqRecipeModels;
        }


        public bool AddNewStep(RecipeStep step, IRecipeBlock recipeBlock)
        {
            if (recipeRulesValidator.ValidateAddingNewStep(recipeSteps.ToArray(), recipeBlock, 0))
            {
                step.BlockOne = recipeBlock;
                step.Name = "Step " + recipeSteps.Count;
                recipeSteps.Add(step);
                return true;
            }
            return false;
        }

        public void AddNewBlankStep(RecipeStep step, int index)
        {
            step.BlockOne = null;
            step.Name = "Step " + recipeSteps.Count;
            recipeSteps.Insert(index, step);
            UpdateRecipeStepNames();
        }

        public bool AddNewStepWhileRunningRecipe(RecipeStep currentRecipeStep, RecipeStep newRecipeStep, IRecipeBlock block, int toBeAddedStepIndex)
        {
            if (recipeRulesValidator.ValidateAddingNewStepWhileRunningRecipe(recipeSteps.ToArray(), currentRecipeStep, newRecipeStep, block, recipeSteps.Count, toBeAddedStepIndex))
            {
                newRecipeStep.BlockOne = block;
                newRecipeStep.Name = "Step " + toBeAddedStepIndex;
                recipeSteps.Insert(toBeAddedStepIndex, newRecipeStep);
                UpdateRecipeStepNames();
                return true;
            }

            return false;
        }

        private void UpdateRecipeStepNames()
        {
            int stepIndex = 0;
            foreach (RecipeStep step in recipeSteps)
            {
                step.Name = "Step " + stepIndex;
                stepIndex += 1;
            }
        }

        public bool RemoveStep(RecipeStep step)
        {
            if (recipeRulesValidator.ValidateDelete(recipeSteps, step))
            {
                if (recipeSteps.Remove(step))
                {
                    RaisePropertyChanged("RecipeSteps");
                    return true;
                }
            }
            return false;
        }

        public void RemoveBlockFromStep(RecipeStep step, IRecipeBlock block)
        {

            if (recipeRulesValidator.ValidateDeleteBlock(step, block))
            {
                var deleteStep = recipeSteps.Where(x => x.BlockOne?.GuidID == step.BlockOne?.GuidID).FirstOrDefault();
                if (deleteStep?.BlockOne != null)
                {
                    recipeSteps.Remove(deleteStep);
                    step.BlockOne = null;
                }
            }
        }

        public bool CheckEndBlockInRecipe(IList<RecipeStep> recipeSteps)
        {
            return recipeRulesValidator.CheckEndBlockInRecipe(recipeSteps);
        }

        public void Clear()
        {
            recipeSteps.Clear();
        }

        public RecipeStep[] Import()
        {
            IList<RecipeStep> importedRecipeSteps = recipeImporter.Import();

            if (importedRecipeSteps != null)
            {
                recipeSteps = importedRecipeSteps.ToList();
                return recipeSteps.ToArray();
            }
            return null;
        }

        public SeqRecipeModel ImportWithFile()
        {
            string filename;
            string receipeName = recipeImporter.ImportWithFileName(out filename);
            if (string.IsNullOrWhiteSpace(receipeName) == false)
            {
                SeqRecipeModel seqRecipeModel = GetSeqRecipeModel(receipeName, filename);
                SeqRecipeList.Add(seqRecipeModel);
                return seqRecipeModel;
            }
            return null;
        }

        public void SaveSeqRecipeWhileExecuting(IList<SeqRecipeModel> SeqRecipeList)
        {
            recipeExporter.SaveSeqRecipeWhileExecuting(SeqRecipeList);
        }

        public void ClearSeqRecipe()
        {
            SeqRecipeList.Clear();
        }
        public List<SeqRecipeModel> ImportSeqReciepe()
        {
            var seqRecipeList = recipeImporter.ImportSeqList();
            if (seqRecipeList?.Count > 0)
            {
                SeqRecipeList.Clear();
                SeqRecipeList.AddRange(seqRecipeList);
            }
            return SeqRecipeList;
        }

        public SeqRecipeModel InsertRecipeInSeq(int index)
        {
            if (index < SeqRecipeList.Count)
            {
                string filename;
                string receipeName = recipeImporter.ImportWithFileName(out filename);
                if (string.IsNullOrWhiteSpace(receipeName) == false)
                {
                    var seqRecipeModel = GetSeqRecipeModel(receipeName, filename);
                    SeqRecipeList.Insert(index, seqRecipeModel);
                    return seqRecipeModel;
                }
            }
            return null;
        }

        private SeqRecipeModel GetSeqRecipeModel(string receipeName, string filename)
        {
            SeqRecipeModel seqRecipeModel = new SeqRecipeModel();
            seqRecipeModel.RecipeName = receipeName;
            seqRecipeModel.FileLocation = filename;
            seqRecipeModel.RecipeGuidId = Guid.NewGuid();
            return seqRecipeModel;
        }

        public bool DeleteRecipeFromSeq(SeqRecipeModel seqRecipeModel)
        {
            if (seqRecipeModel != null)
            {
                return SeqRecipeList.Remove(seqRecipeModel);
            }
            return false;
        }

        public void DeleteSeqRecipe()
        {
            recipeExporter.DeleteSeqRecipe();
        }

        public void SaveSeqRecipe()
        {
            if (SeqRecipeList?.Count > 0)
            {
                recipeExporter.SaveSeqRecipe(this.SeqRecipeList);
            }
        }

        public IList<RecipeStep> ReturnRecipeSteps()
        {
            return RecipeSteps;
        }

        public void ReloadRecipeSteps(Action<Task> action, TaskScheduler taskScheduler)
        {
            Task.Factory.StartNew(new Func<object, IList<RecipeStep>>(recipeReloader.ReloadRecipe), DeviceId)
                .ContinueWith(new Action<Task<IList<RecipeStep>>>(UpdateRecipeSteps))
                .ContinueWith(new Action<Task>(action), taskScheduler);
        }

        public bool CheckIfRecipeStepContainsAnyExecutingBlock(RecipeStep recipeStep)
        {
            if (recipeStep.BlockOne != null)
            {
                if (!GetBlockEndedStatus(recipeStep.BlockOne))
                {
                    return true;
                }
            }
            return false;
        }

        public bool GetBlockEndedStatus(IRecipeBlock block)
        {
            string blockEndedStatus;

            switch (block.Name)
            {
                case "Start":
                    blockEndedStatus = (block as ParameterizedRecipeBlock<StartBlockParameters>).Parameters.Ended;
                    break;
                case "HeatCool":
                    blockEndedStatus = (block as ParameterizedRecipeBlock<HeatCoolBlockParameters>).Parameters.Ended;
                    break;
                case "Stirrer":
                    blockEndedStatus = (block as ParameterizedRecipeBlock<StirrerBlockParameters>).Parameters.Ended;
                    break;
                case "Wait":
                    blockEndedStatus = (block as ParameterizedRecipeBlock<WaitBlockParameters>).Parameters.Ended;
                    break;
                case "Transfer":
                    blockEndedStatus = (block as ParameterizedRecipeBlock<TransferBlockParameters>).Parameters.Ended;
                    break;
                default:
                    blockEndedStatus = string.Empty;
                    break;
            }

            if (string.IsNullOrWhiteSpace(blockEndedStatus))
            {
                return false;
            }
            else
            {
                return bool.Parse(blockEndedStatus);
            }
        }



        public RecipeStep[] RecipeSteps => recipeSteps.ToArray();

        public string Id { get; set; }

        public string DeviceId { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
