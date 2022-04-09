using E3.ReactorManager.DesignExperiment.Model;
using E3.ReactorManager.DesignExperiment.Model.Data;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer.Data;
using E3.UserManager.Model.Data;
using E3Tech.RecipeBuilding.Helpers;
using E3Tech.RecipeBuilding.Model;
using E3Tech.RecipeBuilding.Model.Blocks;
using E3Tech.RecipeBuilding.Model.RecipeExecutionInfoProvider;
using E3Tech.RecipeBuilding.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Unity;

namespace E3Tech.RecipeBuilding.ViewModels
{
    public class RecipeBuilderViewModel : BindableBase
    {
        private readonly IRecipeBuilder recipeBuilder;
        private readonly IDesignExperiment designExperiment;
        private readonly IRecipeReloader recipeReloader;
        private readonly IRecipeExecutor recipeExecutor;
        private readonly IUnityContainer containerProvider;
        private RecipeStepViewModel selectedStep;
        private readonly IFieldDevicesCommunicator fieldDevicesCommunicator;
        private readonly List<PropertyInfo> existingProperties = new List<PropertyInfo>(typeof(RecipeBuilderViewModel).GetProperties());
        private readonly TaskScheduler taskScheduler;
        private readonly IRecipeExecutionInfoProvider recipeExecutionInfoProvider;
        private Dictionary<SeqRecipeModel, IList<RecipeStep>> recipeSeqDetail;


        public RecipeBuilderViewModel(IUnityContainer containerProvider, IRecipeExecutor recipeExecutor, IFieldDevicesCommunicator fieldDevicesCommunicator, IRecipeBuilder recipeBuilder, IRecipeReloader recipeReloader, IDesignExperiment designExperiment)
        {
            taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            this.designExperiment = designExperiment;
            this.recipeExecutor = recipeExecutor;
            this.fieldDevicesCommunicator = fieldDevicesCommunicator;
            fieldDevicesCommunicator.FieldPointDataReceived -= OnLiveDataReceived;
            fieldDevicesCommunicator.FieldPointDataReceived += OnLiveDataReceived;
            recipeExecutionInfoProvider = containerProvider.Resolve<IRecipeExecutionInfoProvider>();
            this.containerProvider = containerProvider;
            this.recipeBuilder = recipeBuilder;
            this.recipeReloader = recipeReloader;
            LoadRegisteredBlocks(containerProvider);
            RecipeSteps = new ObservableCollection<RecipeStepViewModel>();
            LoadSteps();
        }

        #region Initialization of Recipe Steps and Registered Recipe Blocks
        private void LoadSteps()
        {
            int index = 1;
            foreach (RecipeStep step in recipeBuilder.RecipeSteps)
            {
                step.BlockOne.Index = index++;
                RecipeSteps.Add(new RecipeStepViewModel(containerProvider) { RecipeStep = step });
            }
            if (IsSeqRecipeExecuting)
            {
                recipeSeqDetail = recipeBuilder.LoadSeqRecipeList();
                bool recipeStatus = recipeReloader.GetRecipeStatus(DeviceId);
                if (recipeStatus == false)
                {
                    UpdateNextSeqRecipeExecute();
                }
            }

        }
        private void LoadRegisteredBlocks(IUnityContainer containerProvider)
        {
            AvailableBlocks = new List<IRecipeBlock>();
            //Get all the recipe blocks from the container except Fill and Transfer blocks
            IEnumerable<IRecipeBlock> blocks = containerProvider.ResolveAll<IRecipeBlock>();

            if (blocks.Count() > 0)
            {
                /*
                 * Check if the recipe blocks are loaded in the UnityContainer
                 * and Update Available Blocks with the List returned from the UnityContainer
                 */
                AvailableBlocks = new List<IRecipeBlock>(blocks);
            }
        }
        #endregion

        #region Live Data Handlers
        private void UpdatePropertyValue(Task<LiveDataEventArgs> task)
        {
            var liveDataEventArgs = task.Result;

            if (liveDataEventArgs != null && liveDataEventArgs.PropertyInfo != null && liveDataEventArgs.LiveData != null)
            {
                liveDataEventArgs.PropertyInfo.SetValue(this, liveDataEventArgs.LiveData, null);
                if (isSeqRecipeExecuting && liveDataEventArgs.PropertyInfo.Name == "RecipeStatus" && liveDataEventArgs.LiveData == bool.FalseString)
                {
                    SelectedSeqRecipeModel = SeqRecipeModels.Where(x => x.IsExecuting == true).FirstOrDefault();
                    UpdateNextSeqRecipeExecute();
                }
                else
                {
                    if (SeqRecipeModels?.Any() == true && IsSeqRecipeExecuting == false)
                    {
                        foreach (var item in SeqRecipeModels)
                        {
                            if (item.IsExecuting == true)
                            {
                                item.IsExecuting = false;
                                item.IsExecuted = true;
                            }
                        }
                    }
                }
            }
        }

        private void UpdateNextSeqRecipeExecute()
        {
            var seqRecipeModel = SeqRecipeModels.Where(x => x.IsExecuting == true).FirstOrDefault();
            seqRecipeModel.IsExecuting = false;
            seqRecipeModel.IsExecuted = true;
            if (seqRecipeModel != null)
            {
                var index = SeqRecipeModels.IndexOf(seqRecipeModel);
                if (index < SeqRecipeModels.Count - 1 && index < EndSeq - 1)
                {
                    var keyvalueRecipeDetail = recipeSeqDetail.Where(x => x.Key.RecipeGuidId == SeqRecipeModels[index + 1].RecipeGuidId).FirstOrDefault();
                    MessageBoxResult result = MessageBox.Show(string.Format(string.Format("Do You want to execute : {0} ", keyvalueRecipeDetail.Key.RecipeName)), "Information", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        ClearRecipe();
                        var recipeSteps = keyvalueRecipeDetail.Value;
                        recipeBuilder.UpdateRecipeList(recipeSteps);

                        RecipeSteps.Clear();
                        int stepno = 1;
                        foreach (RecipeStep step in recipeSteps)
                        {
                            RecipeStepViewModel stepViewModel = containerProvider.Resolve<RecipeStepViewModel>();
                            stepViewModel.RecipeStep = step;
                            stepViewModel.RecipeStep.BlockOne.Index = stepno++;
                            RecipeSteps.Add(stepViewModel);
                        }
                        selectedSeqRecipeModel = keyvalueRecipeDetail.Key;
                        SelectedSeqRecipeModel = keyvalueRecipeDetail.Key;
                        if (CanStartBatch())
                        {
                            recipeExecutor.Execute(DeviceId, recipeBuilder.RecipeSteps);
                            keyvalueRecipeDetail.Key.IsExecuting = true;
                            recipeBuilder.SaveSeqRecipeWhileExecuting(recipeSeqDetail.Keys.ToList(), StartSeq, EndSeq);
                        }
                    }
                    else
                    {
                        IsSeqRecipeExecuting = false;
                    }
                }
                else
                {
                    IsSeqRecipeExecuting = false;
                }
            }
        }

        private void OnLiveDataReceived(object sender, FieldPointDataReceivedArgs fieldPointDataChangedArgs)
        {
            if (fieldPointDataChangedArgs.FieldDeviceIdentifier == DeviceId)
            {
                var liveDataEventArgs = new LiveDataEventArgs
                {
                    PropertyInfoIdentifier = fieldPointDataChangedArgs.FieldPointDescription,
                    LiveData = fieldPointDataChangedArgs.NewFieldPointData,
                };

                Task.Factory.StartNew(new Func<object, LiveDataEventArgs>(ValidateLiveDataReceived), liveDataEventArgs)
                    .ContinueWith(new Action<Task<LiveDataEventArgs>>(UpdatePropertyValue), taskScheduler);
            }
        }

        private LiveDataEventArgs ValidateLiveDataReceived(object liveData)
        {
            if (liveData != null)
            {
                var liveDataEventArgs = (LiveDataEventArgs)liveData;

                liveDataEventArgs.PropertyInfo
                    = existingProperties.FirstOrDefault(property => property.Name == liveDataEventArgs.PropertyInfoIdentifier);

                return liveDataEventArgs;
            }

            return null;
        }
        #endregion

        public void UpdateRecipeParameters()
        {
            RecipeStatus = fieldDevicesCommunicator.ReadFieldPointValue<bool>(DeviceId, "RecipeStatus").ToString();
            IsRecipeRunning = fieldDevicesCommunicator.ReadFieldPointValue<bool>(DeviceId, "RecipeStatus").ToString();
            RecipeEnded = fieldDevicesCommunicator.ReadFieldPointValue<bool>(DeviceId, "RecipeEnded").ToString();
            RecipePaused = fieldDevicesCommunicator.ReadFieldPointValue<bool>(DeviceId, "PauseRecipe").ToString();
            DrainStatus = fieldDevicesCommunicator.ReadFieldPointValue<bool>(DeviceId, "DrainStatus").ToString();
        }

        public void UpdateNavigationParameters(NavigationParameters NavigationParameters)
        {
            this.NavigationParameters = NavigationParameters;
        }

        #region Export & Import Recipe
        private void ExportRecipe()
        {
            recipeBuilder.Export();
        }

        private void ImportRecipe()
        {
            RecipeStep[] recipeSteps = recipeBuilder.Import();
            if (recipeSteps != null)
            {
                RecipeSteps.Clear();
                int index = 1;
                foreach (RecipeStep step in recipeSteps)
                {
                    RecipeStepViewModel stepViewModel = containerProvider.Resolve<RecipeStepViewModel>();
                    stepViewModel.RecipeStep = step;
                    stepViewModel.RecipeStep.BlockOne.Index = index++;
                    RecipeSteps.Add(stepViewModel);
                }
            }
            else
            {
                MessageBox.Show("No data in the selected file", "Import Recipe Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Start Recipe
        private void StartRecipe()
        {
            ExecuteRecipe();
        }

        private bool ExecuteRecipe()
        {

            Task.Factory.StartNew(() => designExperiment.StartBatchCompact(NewBatchDetails))
                 .ContinueWith(new Action<Task<bool>>((t) =>
                 {
                     if (t.IsCompleted)
                     {
                         if (t.Result)
                         {
                             //Navigate("Dashboard");
                             IsBatchNameRequestPopUpOpen = false;
                         }
                         else
                         {
                             MessageBox.Show("Batch Already Exists, Please Re-enter the Batch Details", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                         }
                     }
                 }), taskScheduler);
            if (CanStartBatch())
            {
                recipeExecutor.Execute(DeviceId, recipeBuilder.RecipeSteps);
                if (IsSeqRecipeExecuting)
                {
                    var recipeSeqToExecute = recipeSeqDetail.ElementAt((int)StartSeq - 1);
                    recipeSeqToExecute.Key.IsExecuting = true;
                    recipeBuilder.SaveSeqRecipeWhileExecuting(recipeSeqDetail.Keys.ToList(), StartSeq, EndSeq);
                }
                return true;
            }
            else
            {
            }
            IsSeqRecipeExecuting = false;
            return false;
        }

        public void ValidateAndOpenPopup()
        {
            if (recipeBuilder.CheckEndBlockInRecipe(recipeBuilder.RecipeSteps))
            {
                NewBatchDetails.Name = "";
                NewBatchDetails.ScientistName = "";
                NewBatchDetails.Comments = "";
                IsBatchNameRequestPopUpOpen = true;
            }
            else
            {
                MessageBox.Show("Please Add End Block in the Recipe",
                                "Recipe Execution Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                IsBatchNameRequestPopUpOpen = false;
            }

        }

        private bool CanStartBatch()
        {
            return !string.IsNullOrWhiteSpace(NewBatchDetails.Name)
                && !string.IsNullOrWhiteSpace(NewBatchDetails.ScientistName)
                && !string.IsNullOrWhiteSpace(NewBatchDetails.Comments)
                && recipeBuilder.CheckEndBlockInRecipe(recipeBuilder.RecipeSteps);
        }

        private void CloseBatchNamePopup()
        {
            IsBatchNameRequestPopUpOpen = false;
            IsSeqRecipeExecuting = false;
        }

        private void OpenEndBatchPopup()
        {
            IsEndBatchPopUpOpen = true;
            AdminCredential.PasswordHash = "";
        }

        public void EndBatch()
        {
            UpdateBatchDetailsInUI();
            Task.Factory.StartNew(new Action(TerminateBatch))
                .ContinueWith(new Action<Task>(ClosePopup), taskScheduler);
        }
        public void ClosePopup(Task task)
        {
            if (task.IsCompleted)
            {
                IsEndBatchPopUpOpen = false;
            }
        }

        public void CloseBatchEndPopup()
        {
            IsEndBatchPopUpOpen = false;
        }

        public bool CanEndBatch()
        {
            return !string.IsNullOrWhiteSpace(AdminCredential.Username) && !string.IsNullOrWhiteSpace(AdminCredential.PasswordHash) && !string.IsNullOrWhiteSpace(CurrentBatchDetails.FieldDeviceIdentifier);
        }

        private void TerminateBatch()
        {
            if (designExperiment.EndBatchCompact(AdminCredential, CurrentBatchDetails.FieldDeviceIdentifier, CurrentBatchDetails.Name, CurrentBatchDetails.FieldDeviceIdentifier))
            {
                if (IsSeqRecipeExecuting)
                {
                    IsSeqRecipeExecuting = false;
                    if (selectedSeqRecipeModel != null)
                    {
                        selectedSeqRecipeModel.IsExecuting = false;
                    }

                    recipeBuilder.DeleteSeqRecipe();
                }
                AbortRecipeExecution();
            }
            else
            {
                IsEndBatchPopUpOpen = false;
                MessageBox.Show($"Unable to end the batch. Admin Credentials are not valid", "Authorization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            //Clear Admin Credentials, CleaningSolvent
            AdminCredential = new Credential();
        }

        private void PauseRecipe()
        {
            fieldDevicesCommunicator
               .SendCommandToDevice(DeviceId,
                                          "PauseRecipe",
                                          "bool",
                                          Boolean.TrueString);
        }
        private void SkipDrainExecution()
        {
            fieldDevicesCommunicator
               .SendCommandToDevice(DeviceId,
                                          "DrainSkipExecution",
                                          "bool",
                                          Boolean.TrueString);
        }
        private bool CanStartRecipe()
        {
            return recipeExecutor != null;
        }
        #endregion

        #region CurrentBatchDetails

        private void UpdateBatchDetailsInUI()
        {
            CurrentBatchDetails.FieldDeviceIdentifier = designExperiment.FetchRunningBatch();
        }

        #endregion

        #region Clear Recipe
        public void ClearRecipe()
        {
            recipeBuilder.Clear();
            RecipeSteps.Clear();
            recipeExecutor.ClearRecipe(DeviceId);
            fieldDevicesCommunicator
              .SendCommandToDevice(DeviceId,
                                         "ClearRecipe",
                                         "bool",
                                         Boolean.TrueString);
        }

        private bool CanClearRecipe()
        {
            return recipeExecutor != null;
        }
        #endregion

        private void UpdateBlockExecution(IRecipeBlock recipeBlock)
        {
            if (recipeBlock == null)
            {
                return;
            }

            recipeBlock.Configure(containerProvider);
        }

        private void HandleDeleteStep(RecipeStepViewModel stepViewModel)
        {
            if (stepViewModel != null)
            {
                if (recipeBuilder.RemoveStep(stepViewModel.RecipeStep))
                {
                    RecipeSteps.Remove(stepViewModel);
                }
                int index = 1;
                foreach (var item in RecipeSteps)
                {
                    item.RecipeStep.BlockOne.Index = index++;
                }
            }
        }

        private void HandleDeleteBlock(IRecipeBlock obj)
        {
            if (obj != null)
            {
                int index = 0;

                foreach (var recipeStepViewModel in RecipeSteps)
                {
                    if (recipeStepViewModel.RecipeStep?.BlockOne?.GuidID == obj.GuidID)
                    {
                        recipeBuilder.RemoveBlockFromStep(recipeStepViewModel.RecipeStep, obj);
                        if (recipeStepViewModel.RecipeStep?.BlockOne == null)
                        {
                            RecipeSteps.Remove(recipeStepViewModel);
                        }
                        break;
                        index++;
                    }
                }

                index = 1;
                foreach (var item in RecipeSteps)
                {
                    item.RecipeStep.BlockOne.Index = index++;
                }
            }
        }

        private void UpdateIsSaveChanges()
        {
            IsSaveChange = (RecipeStatus == "True" && IsEditEnabled == true) || IsSeqRecipeExecuting;
            IsEditEnabled = false;
        }

        private void UpdateIsEditEnable()
        {
            IsEditEnabled = RecipeStatus == "True" && IsSaveChange == false;
            IsSaveChange = RecipeStatus == "True" && IsSaveChange == true;
        }

        private void EditReceipeExecution()
        {
            IsSaveChange = true;
            UpdateIsSaveChanges();
            recipeExecutor.EditBlockExecution(DeviceId);
        }

        private void SaveChangesExecution()
        {
            IsSaveChange = false;
            recipeExecutor.Execute(DeviceId, recipeBuilder.RecipeSteps);
            UpdateIsEditEnable();
            recipeExecutor.SaveUpdatedBlockExecution(DeviceId);
        }

        #region Abort Recipe & Abort RecipeBlock Execution
        public void AddBlockExecution(IRecipeBlock block)
        {
            if (block != null)
            {
                int index = 0;

                foreach (var recipeStepViewModel in RecipeSteps)
                {
                    if (recipeStepViewModel.RecipeStep?.BlockOne?.GuidID == block.GuidID)
                    {
                        break;
                    }
                    index++;
                }
                var receipeStep = new RecipeStepViewModel(containerProvider) { RecipeStep = new RecipeStep() };
                recipeBuilder.AddNewBlankStep(receipeStep.RecipeStep, index);
                RecipeSteps.Insert(index, receipeStep);
            }
        }

        public void AbortRecipeExecution()
        {
            recipeExecutor.AbortRecipeExecution(DeviceId);
        }

        private bool CanAbortRecipeExecution()
        {
            return bool.Parse(RecipeStatus) || IsSeqRecipeExecuting;
        }
        #endregion

        #region SeqRecipe
        private void SaveSeqReceipeExecute()
        {
            if (SeqRecipeModels?.Count > 0)
            {
                recipeBuilder.SaveSeqRecipe();
            }
        }

        private void ImportSeqReceipeExecute()
        {
            var seqRecipeList = recipeBuilder.ImportSeqReciepe();
            if (seqRecipeList?.Count > 0)
            {
                SeqRecipeModels = new ObservableCollection<SeqRecipeModel>(seqRecipeList);
                EndSeq = (uint)SeqRecipeModels.Count();
                UpdateSeqRecipeCount();
            }
        }

        private void ClearSeqReceipeExecute()
        {
            ClearRecipe();
            recipeBuilder.ClearSeqRecipe();
            SeqRecipeModels.Clear();
            startSeq = 1;
            StartSeq = 1;
            endSeq = 1;
            EndSeq = 1;
            recipeBuilder.DeleteSeqRecipe();
        }

        private void StartSeqReceipeCommandExecute()
        {
            if (SeqRecipeModels?.Count > 0 && recipeBuilder.ValidateSeqRecipe())
            {
                foreach (var item in SeqRecipeModels)
                {
                    item.IsExecuted = false;
                    item.IsExecuting = false;
                }
                recipeSeqDetail = recipeBuilder.LoadSeqRecipeList();
                var recipeSeqToExecute = recipeSeqDetail.ElementAt((int)StartSeq - 1);
                ClearRecipe();

                selectedSeqRecipeModel = recipeSeqToExecute.Key;
                SelectedSeqRecipeModel = recipeSeqToExecute.Key;
                recipeBuilder.UpdateRecipeList(recipeSeqToExecute.Value);
                RecipeSteps.Clear();
                foreach (var step in recipeSeqToExecute.Value.ToList())
                {
                    RecipeStepViewModel stepViewModel = containerProvider.Resolve<RecipeStepViewModel>();
                    stepViewModel.RecipeStep = step;
                    RecipeSteps.Add(stepViewModel);
                }
                IsSeqRecipeExecuting = true;
                ValidateAndOpenPopup();
                //var result = ExecuteRecipe();

                //if (result == true)
                //{
                //    recipeSeqToExecute.Key.IsExecuting = true;
                //    recipeBuilder.SaveSeqRecipeWhileExecuting(recipeSeqDetail.Keys.ToList(), StartSeq, EndSeq);
                //}
            }
        }



        private void MouseDoubleClickExecute()
        {
            SeqRecipeModel seqRecipeModel = recipeBuilder.ImportWithFile();
            if (seqRecipeModel != null)
            {
                SeqRecipeModels.Add(seqRecipeModel);
                EndSeq = (uint)SeqRecipeModels.Count();
                UpdateSeqRecipeCount();
            }
        }

        private void UpdateSeqRecipeCount()
        {
            int index = 1;
            foreach (var seqRecipeModel in SeqRecipeModels)
            {
                seqRecipeModel.SeqCount = index++;
            }
        }

        #endregion

        public void SetDeviceId(string deviceId)
        {
            this.DeviceId = deviceId;
            //update Field Device Label
            FieldDeviceLabel = fieldDevicesCommunicator.GetFieldDeviceLabel(deviceId);

            if (RecipeSteps.Count == 0)
            {
                /*
                 * If RecipeSteps count was zero check RecipeStatus and RecipeEndedStatus
                 * and Reload Recipe Steps if required
                 */
                int startSeq;
                int endSeq;
                var seqRecipeList = recipeBuilder.ReloadSeqRecipes(out startSeq, out endSeq);
                if (seqRecipeList?.Count > 0)
                {
                    SeqRecipeModels = new ObservableCollection<SeqRecipeModel>(seqRecipeList);
                    this.startSeq = (uint)startSeq;
                    this.StartSeq = (uint)startSeq;
                    this.endSeq = (uint)endSeq;
                    this.EndSeq = (uint)endSeq;
                    UpdateSeqRecipeCount();
                    if (SeqRecipeModels.Where(x => x.IsExecuting == true).FirstOrDefault() != null)
                    {
                        IsSeqRecipeExecuting = true;
                    }
                }
                Task.Factory.StartNew(new Func<object, bool>(IsReloadRecipeActionRequired), deviceId)
                    .ContinueWith(new Action<Task<bool>>(ReloadRecipeSteps));
            }
        }

        private void ReloadRecipeSteps(Task<bool> task)
        {
            if (task.IsCompleted)
            {
                /* Update Recipe Status */
                this.RecipeStatus = task.Result.ToString();

                if (task.Result)
                {
                    recipeBuilder.ReloadRecipeSteps(new Action<Task>((t) => LoadSteps()), taskScheduler);
                }
            }
        }

        private bool IsReloadRecipeActionRequired(object arg)
        {
            bool recipeStatus = recipeReloader.GetRecipeStatus((string)arg);
            bool recipeEndedStatus = recipeReloader.GetRecipeEndedStatus((string)arg);

            return recipeStatus || recipeEndedStatus || IsSeqRecipeExecuting;
        }

        private bool IsRecipeEnded() => !bool.Parse(RecipeStatus);

        private void ShowErrorNotificationToUser()
        {
            MessageBox.Show("You are allowed to give only 'Clear Recipe' Command", "Invalid Command", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void SaveRecipeExecution()
        {
            Task.Factory.StartNew(() =>
            {
                recipeExecutionInfoProvider.SaveRecipeExecutionInfo(RecipeSteps.Select(step => step.RecipeStep).ToList(), DeviceId);
            });
        }

        #region Drag & Drop
        private IRecipeBlock GetBlockInstance(IDataObject dropData)
        {
            IRecipeBlock block = containerProvider.Resolve(dropData.GetData("PersistentObject").GetType()) as IRecipeBlock;
            block.Name = dropData.GetData("Text").ToString();
            block.GuidID = Guid.NewGuid().ToString();
            return block;
        }

        private void HandleDropOnCell(DataGridCellDropCommandParameters parameters)
        {
            var parameter = parameters.DataContext as RecipeStepViewModel;
            if (parameter.RecipeStep.BlockOne != null)
            {
                return;
            }
            IRecipeBlock block = GetBlockInstance(parameters.DataObject);

            int recipeStepIndex = RecipeSteps.IndexOf(parameter);
            if (IsDropBlockAllowed(block, recipeStepIndex))
            {
                bool isBlockValid = false;
                if (RecipeSteps.Count == 1 && block.Name == "Start")
                {
                    isBlockValid = recipeBuilder.AddNewStep(RecipeSteps.First().RecipeStep, block);
                }
                else
                {
                    isBlockValid = recipeBuilder.UpdateStep(RecipeSteps[recipeStepIndex].RecipeStep, parameters.Block, block);
                }

                if (isBlockValid && block.Configure(containerProvider))
                {
                    UpdateRecipeToExecuter(block, recipeStepIndex);
                }
                else
                {
                    RecipeSteps[recipeStepIndex].RecipeStep.BlockOne = null;
                }

                int index = 1;
                foreach (var item in RecipeSteps)
                {
                    if (item.RecipeStep.BlockOne != null)
                    {
                        item.RecipeStep.BlockOne.Index = index++;
                    }
                    else
                    {
                        index++;
                    }
                }
            }

        }

        private bool IsDropBlockAllowed(IRecipeBlock block, int recipeStepIndex)
        {
            return bool.Parse(RecipeStatus) ? GetRecipeStepEditPermission(recipeStepIndex) : true;
        }

        private void UpdateRecipeToExecuter(IRecipeBlock block, int recipeStepIndex)
        {
            recipeExecutor.UpdateBlock(recipeStepIndex, block, DeviceId);
        }

        private bool GetRecipeStepEditPermission(int newlyAddedBlockRecipeStepIndex)
        {
            int currentRunningRecipeStepIndex = 0;

            // Find the current Running step
            foreach (RecipeStepViewModel step in RecipeSteps)
            {
                if (step.RecipeStep.BlockOne.Name == "Start")
                {
                    // Skip. You don't have to check the Start Block status
                }
                else
                {
                    RecipeBlockStatus blockOneStatus = GetRecipeBlockStatus(step.RecipeStep.BlockOne);

                    if (blockOneStatus == RecipeBlockStatus.Started)
                    {
                        // Check if atleast one of the Recipe Blocks in that step is running
                        currentRunningRecipeStepIndex = RecipeSteps.IndexOf(step);
                        break;
                    }
                }
            }

            return currentRunningRecipeStepIndex <= newlyAddedBlockRecipeStepIndex ? true : false;
        }

        private RecipeBlockStatus GetRecipeBlockStatus(IRecipeBlock block)
        {
            return block == null ? RecipeBlockStatus.NotConfigured
                    : bool.Parse(block.GetParameterValue("Started")) ?
                        (bool.Parse(block.GetParameterValue("Ended")) ?
                            RecipeBlockStatus.Ended : RecipeBlockStatus.Started) : RecipeBlockStatus.NotStarted;
        }

        private void HandleDropOnGrid(DataGridDropCommandParameters parameters)
        {

            IRecipeBlock block = GetBlockInstance(parameters.DataObject);
            RecipeStep step;
            bool firstStep = false;
            if (RecipeSteps.Count == 1 && block.Name == "Start")
            {
                step = RecipeSteps.First().RecipeStep;
                firstStep = true;
            }
            else
            {
                step = new RecipeStep();
            }

            bool isBlockValid = recipeBuilder.AddNewStep(step, block);
            if (isBlockValid)
            {
                RecipeStepViewModel stepViewModel = containerProvider.Resolve<RecipeStepViewModel>();
                stepViewModel.RecipeStep = step;
                if (!firstStep && block.Configure(containerProvider))
                {
                    RecipeSteps.Add(stepViewModel);
                    SelectedStep = stepViewModel;
                }
            }
            int index = 1;
            foreach (var item in RecipeSteps)
            {
                if (item.RecipeStep.BlockOne != null)
                {
                    item.RecipeStep.BlockOne.Index = index++;
                }
                else
                {
                    index++;
                }
            }

        }

        public void HandleDropToAddNewStep(GridDropCommandParameters parameters)
        {

            IRecipeBlock block = GetBlockInstance(parameters.DataObject);
            int currentRecipeStepIndex = RecipeSteps.IndexOf(parameters.DataContext as RecipeStepViewModel);
            int toBeAddedRecipeStepIndex = currentRecipeStepIndex + 1;
            RecipeStep currentRecipeStep = RecipeSteps[currentRecipeStepIndex].RecipeStep;
            RecipeStep newRecipeStep = new RecipeStep();
            bool isBlockValid = recipeBuilder.AddNewStepWhileRunningRecipe(currentRecipeStep, newRecipeStep, block, toBeAddedRecipeStepIndex);
            if (isBlockValid)
            {
                RecipeStepViewModel stepViewModel = containerProvider.Resolve<RecipeStepViewModel>();
                stepViewModel.RecipeStep = newRecipeStep;

                bool result = block.Configure(containerProvider);
                if (result)
                {
                    RecipeSteps.Insert(toBeAddedRecipeStepIndex, stepViewModel);
                    SelectedStep = stepViewModel;
                }

            }
            int index = 1;
            foreach (var item in RecipeSteps)
            {
                if (item.RecipeStep.BlockOne != null)
                {
                    item.RecipeStep.BlockOne.Index = index++;
                }
                else
                {
                    index++;
                }
            }
        }

        private void HandleMouseButton(MouseButtonCommandParameters parameters)
        {
            DataObject dropData = new DataObject();
            dropData.SetData(DataFormats.Text, parameters.Data);
            dropData.SetData(DataFormats.Serializable, parameters.Data);
            // send the selected block type to be dynamically instantiated on drop.
            DragDrop.DoDragDrop(parameters.Sender, dropData, DragDropEffects.Copy);
        }
        #endregion

        #region SeqBlock

        private uint startSeq;
        public uint StartSeq
        {
            get
            {
                if (startSeq == 0)
                {
                    startSeq = 1;
                }
                return startSeq;
            }
            set
            {
                uint startseq;
                if (uint.TryParse(value.ToString(), out startseq) && startseq <= SeqRecipeModels.Count && startseq <= EndSeq && startseq >= 1)
                {
                    startSeq = startseq;
                }
                RaisePropertyChanged();
            }
        }

        private uint endSeq;
        public uint EndSeq
        {
            get
            {
                if (endSeq == 0)
                {
                    endSeq = 1;
                }
                return endSeq;
            }
            set
            {
                uint endseq;
                if (uint.TryParse(value.ToString(), out endseq) && endseq <= SeqRecipeModels.Count && endseq >= StartSeq)
                {
                    endSeq = endseq;
                }
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<SeqRecipeModel> seqRecipeModels;

        public ObservableCollection<SeqRecipeModel> SeqRecipeModels
        {
            get { return seqRecipeModels == null ? seqRecipeModels = new ObservableCollection<SeqRecipeModel>() : seqRecipeModels; }
            set
            {
                seqRecipeModels = value;
                RaisePropertyChanged();
            }
        }

        private SeqRecipeModel selectedSeqRecipeModel;
        public SeqRecipeModel SelectedSeqRecipeModel
        {
            get { return selectedSeqRecipeModel; }
            set
            {
                if (IsSeqRecipeExecuting == false)
                {
                    selectedSeqRecipeModel = value;
                }
                RaisePropertyChanged();
            }
        }

        private bool isSeqRecipeExecuting;
        public bool IsSeqRecipeExecuting
        {
            get { return isSeqRecipeExecuting; }
            set
            {
                isSeqRecipeExecuting = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Commands

        private ICommand dropCellCommand;
        public ICommand DropCellCommand
        {
            get => dropCellCommand ?? (dropCellCommand = new DelegateCommand<DataGridCellDropCommandParameters>(new Action<DataGridCellDropCommandParameters>(HandleDropOnCell)));
            set => SetProperty(ref dropCellCommand, value);
        }

        private ICommand dropCommand;
        public ICommand DropCommand
        {
            get => dropCommand ?? (dropCommand = new DelegateCommand<DataGridDropCommandParameters>(new Action<DataGridDropCommandParameters>(HandleDropOnGrid)));
            set => SetProperty(ref dropCommand, value);
        }

        private ICommand _dropCommandToAddNewStep;
        public ICommand DropCommandToAddNewStep
        {
            get => _dropCommandToAddNewStep ?? (_dropCommandToAddNewStep = new DelegateCommand<GridDropCommandParameters>(new Action<GridDropCommandParameters>(HandleDropToAddNewStep)));
            set => SetProperty(ref _dropCommandToAddNewStep, value);
        }

        private ICommand mouseButtonCommand;
        public ICommand MouseButtonCommand
        {
            get => mouseButtonCommand ?? (mouseButtonCommand = new DelegateCommand<MouseButtonCommandParameters>(new Action<MouseButtonCommandParameters>(HandleMouseButton)));
            set => SetProperty(ref mouseButtonCommand, value);
        }

        private ICommand exportRecipeCommand;
        public ICommand ExportRecipeCommand
        {
            get => exportRecipeCommand ?? (exportRecipeCommand = new DelegateCommand(new Action(ExportRecipe)));
            set => SetProperty(ref exportRecipeCommand, value);
        }

        //private ICommand startRecipeCommand;
        //public ICommand StartRecipeCommand
        //{
        //    get => startRecipeCommand ?? (startRecipeCommand = new DelegateCommand(new Action(StartRecipe), new Func<bool>(CanStartRecipe)));
        //    set => SetProperty(ref startRecipeCommand, value);
        //}

        private ICommand _openEndBatchPopupCommand;
        public ICommand OpenEndBatchPopupCommand
        {
            get => _openEndBatchPopupCommand ?? (_openEndBatchPopupCommand = new DelegateCommand(new Action(OpenEndBatchPopup)));
            set => SetProperty(ref _openEndBatchPopupCommand, value);
        }

        private ICommand _endBatchCommand;
        public ICommand EndBatchCommand
        {
            get => _endBatchCommand ?? (_endBatchCommand = new DelegateCommand(EndBatch)
                .ObservesProperty(() => AdminCredential.Username)
                .ObservesProperty(() => AdminCredential.PasswordHash)
                .ObservesProperty(() => CurrentBatchDetails.Identifier));
            set => _endBatchCommand = value;
        }

        private Batch _currentBatchDetails;
        public Batch CurrentBatchDetails
        {
            get => _currentBatchDetails ?? (_currentBatchDetails = new Batch());
            set
            {
                _currentBatchDetails = value;
                RaisePropertyChanged();
            }
        }

        private ICommand _validateAndOpenPopupCommand;
        public ICommand ValidateAndOpenPopupCommand
        {
            get => _validateAndOpenPopupCommand ?? (_validateAndOpenPopupCommand = new DelegateCommand(new Action(ValidateAndOpenPopup)));
            set => _startRecipeCommand = value;
        }

        private ICommand _closeBatchNamePopupCommand;
        public ICommand CloseBatchNamePopupCommand
        {
            get => _closeBatchNamePopupCommand ?? (_closeBatchNamePopupCommand = new DelegateCommand(new Action(CloseBatchNamePopup)));
            set => _closeBatchNamePopupCommand = value;
        }
        private ICommand _closeBatchEndPopupCommand;
        public ICommand CloseBatchEndPopupCommand
        {
            get => _closeBatchEndPopupCommand ?? (_closeBatchEndPopupCommand = new DelegateCommand(new Action(CloseBatchEndPopup)));
            set => _closeBatchEndPopupCommand = value;
        }
        private ICommand _startRecipeCommand;
        public ICommand StartRecipeCommand
        {
            get => _startRecipeCommand ?? (_startRecipeCommand = new DelegateCommand(new Action(StartRecipe))
                .ObservesProperty(() => NewBatchDetails.Name)
                .ObservesProperty(() => NewBatchDetails.ScientistName)
                .ObservesProperty(() => NewBatchDetails.Comments));
            set => _startRecipeCommand = value;
        }

        private ICommand pauseRecipeCommand;
        public ICommand PauseRecipeCommand
        {
            get => pauseRecipeCommand ?? (pauseRecipeCommand = new DelegateCommand(new Action(PauseRecipe)));
            set => SetProperty(ref pauseRecipeCommand, value);
        }
        private ICommand _skipDrainExecutionCommand;
        public ICommand SkipDrainExecutionCommand
        {
            get => _skipDrainExecutionCommand ?? (pauseRecipeCommand = new DelegateCommand(new Action(SkipDrainExecution)));
            set => SetProperty(ref _skipDrainExecutionCommand, value);
        }
        private ICommand clearRecipeCommand;
        public ICommand ClearRecipeCommand
        {
            get => clearRecipeCommand ?? (clearRecipeCommand = new DelegateCommand(new Action(ClearRecipe), new Func<bool>(CanClearRecipe)));
            set => SetProperty(ref clearRecipeCommand, value);
        }

        private ICommand deleteStepCommand;
        public ICommand DeleteStepCommand
        {
            get => deleteStepCommand ?? (deleteStepCommand = new DelegateCommand<RecipeStepViewModel>(new Action<RecipeStepViewModel>(HandleDeleteStep)));
            set => SetProperty(ref deleteStepCommand, value);
        }

        private ICommand deleteBlockCommand;
        public ICommand DeleteBlockCommand
        {
            get => deleteBlockCommand ?? (deleteBlockCommand = new DelegateCommand<IRecipeBlock>(new Action<IRecipeBlock>(HandleDeleteBlock)));
            set => SetProperty(ref deleteBlockCommand, value);
        }

        private ICommand updateBlockCommand;
        public ICommand UpdateBlockCommand
        {
            get => updateBlockCommand ?? (updateBlockCommand = new DelegateCommand<IRecipeBlock>(new Action<IRecipeBlock>(UpdateBlockExecution)));
            set => SetProperty(ref updateBlockCommand, value);
        }

        private ICommand addBlockExecutionCommand;
        public ICommand AddBlockExecutionCommand
        {
            get => addBlockExecutionCommand ?? (addBlockExecutionCommand = new DelegateCommand<IRecipeBlock>(new Action<IRecipeBlock>(AddBlockExecution)));
            set => SetProperty(ref addBlockExecutionCommand, value);
        }

        private ICommand abortRecipeExecutionCommand;
        public ICommand AbortRecipeExecutionCommand
        {
            get => abortRecipeExecutionCommand ?? (abortRecipeExecutionCommand = new DelegateCommand(new Action(AbortRecipeExecution), new Func<bool>(CanAbortRecipeExecution)));
            set => SetProperty(ref abortRecipeExecutionCommand, value);
        }
        private ICommand importRecipeCommand;
        public ICommand ImportRecipeCommand
        {
            get => importRecipeCommand ?? (importRecipeCommand = new DelegateCommand(new Action(ImportRecipe)));
            set => SetProperty(ref importRecipeCommand, value);
        }

        private ICommand _saveRecipeExecutionCommand;
        public ICommand SaveRecipeExecutionCommand
        {
            get => _saveRecipeExecutionCommand ?? (_saveRecipeExecutionCommand = new DelegateCommand(SaveRecipeExecution));
            set => SetProperty(ref _saveRecipeExecutionCommand, value);
        }

        private ICommand saveChangesRecipeCommand;
        public ICommand SaveChangesRecipeCommand
        {
            get => saveChangesRecipeCommand ?? (saveChangesRecipeCommand = new DelegateCommand(new Action(SaveChangesExecution)));
            set => SetProperty(ref saveChangesRecipeCommand, value);
        }

        private ICommand editRecipeCommand;
        public ICommand EditRecipeCommand
        {
            get => editRecipeCommand ?? (editRecipeCommand = new DelegateCommand(new Action(EditReceipeExecution)));
            set => SetProperty(ref editRecipeCommand, value);
        }

        #region SeqReceipeCommand

        private ICommand mouseDoubleClickCommand;
        public ICommand MouseDoubleClickCommand
        {
            get => mouseDoubleClickCommand ?? (mouseDoubleClickCommand = new DelegateCommand(new Action(MouseDoubleClickExecute)));
            set => SetProperty(ref mouseDoubleClickCommand, value);
        }

        private ICommand saveSeqReceipeCommand;
        public ICommand SaveSeqReceipeCommand
        {
            get => saveSeqReceipeCommand ?? (saveSeqReceipeCommand = new DelegateCommand(new Action(SaveSeqReceipeExecute)));
            set => SetProperty(ref saveSeqReceipeCommand, value);
        }

        private ICommand importSeqReceipeCommand;
        public ICommand ImportSeqReceipeCommand
        {
            get => importSeqReceipeCommand ?? (importSeqReceipeCommand = new DelegateCommand(new Action(ImportSeqReceipeExecute)));
            set => SetProperty(ref importSeqReceipeCommand, value);
        }

        private ICommand clearSeqReceipeCommand;
        public ICommand ClearSeqReceipeCommand
        {
            get => clearSeqReceipeCommand ?? (clearSeqReceipeCommand = new DelegateCommand(new Action(ClearSeqReceipeExecute)));
            set => SetProperty(ref clearSeqReceipeCommand, value);
        }

        private ICommand startSeqReceipeCommand;
        public ICommand StartSeqReceipeCommand
        {
            get => startSeqReceipeCommand ?? (startSeqReceipeCommand = new DelegateCommand(new Action(StartSeqReceipeCommandExecute)));
            set => SetProperty(ref startSeqReceipeCommand, value);
        }

        private ICommand previewSequenceCommand;
        public ICommand PreviewSequenceCommand
        {
            get => previewSequenceCommand ?? (previewSequenceCommand = new DelegateCommand<object>(new Action<object>(PreviewSequenceCommandExecute)));
            set => SetProperty(ref previewSequenceCommand, value);
        }

        private void PreviewSequenceCommandExecute(object obj)
        {
            if (obj is SeqRecipeModel)
            {
                var seqRecipeModel = obj as SeqRecipeModel;

                if (recipeSeqDetail == null || recipeSeqDetail.ContainsKey(seqRecipeModel) == false)
                {
                    recipeSeqDetail = recipeBuilder.LoadSeqRecipeList();
                }
                PreviewBuilderView view = new PreviewBuilderView();
                List<RecipeBlockPreviewModel> recipeBlockPreviewModels = new List<RecipeBlockPreviewModel>();
                int no = 1;
                foreach(var recipeStep in recipeSeqDetail[seqRecipeModel])
                {
                    RecipeBlockPreviewModel recipeBlockPreviewModel = new RecipeBlockPreviewModel();
                    recipeBlockPreviewModel.Name = recipeStep?.BlockOne.Name;
                    recipeBlockPreviewModel.BlockNo = no++.ToString() + ": ";
                    switch (recipeStep?.BlockOne.Name)
                    {
                       
                        case "Wait":
                            ParameterizedRecipeBlock<WaitBlockParameters> waitBlock = recipeStep.BlockOne as ParameterizedRecipeBlock<WaitBlockParameters>;
                            recipeBlockPreviewModel.PropertyThree = "Time Interval: " + waitBlock.Parameters.TimeInterval + " " + waitBlock.Parameters.IntervalType;
                            
                            break;
                        case "Stirrer":
                            ParameterizedRecipeBlock<StirrerBlockParameters> stirBlock = recipeStep.BlockOne as ParameterizedRecipeBlock<StirrerBlockParameters>;
                            recipeBlockPreviewModel.PropertyOne ="Set Point: " + stirBlock.Parameters.Destination;
                            recipeBlockPreviewModel.PropertyTwo = "Destination: " + stirBlock.Parameters.SetPoint;
                            break;
                        case "Transfer":
                            ParameterizedRecipeBlock<TransferBlockParameters> transferBlock = recipeStep.BlockOne as ParameterizedRecipeBlock<TransferBlockParameters>;
                            recipeBlockPreviewModel.PropertyOne = "Source: " + transferBlock.Parameters.Source;
                            recipeBlockPreviewModel.PropertyTwo = "Destination: " + transferBlock.Parameters.Destination;
                            recipeBlockPreviewModel.PropertyThree = "Time Interval: " + transferBlock.Parameters.TimeInterval + " " + transferBlock.Parameters.IntervalType;
                            recipeBlockPreviewModel.PropertyFour = "Transfer Mode: " + (transferBlock.Parameters.TransferMode.Equals(bool.TrueString) ? "Time" : "Level");
                            break;
           
                        case "Drain":
                            ParameterizedRecipeBlock<DrainBlockParameters> drainBlock = recipeStep.BlockOne as ParameterizedRecipeBlock<DrainBlockParameters>;
                            recipeBlockPreviewModel.PropertyOne = "Source: " + drainBlock.Parameters.Source;
                            recipeBlockPreviewModel.PropertyThree = "Time Interval: " + drainBlock.Parameters.TimeInterval + " " + drainBlock.Parameters.IntervalType;
                            break;
                        case "N2Purge":
                            ParameterizedRecipeBlock<N2PurgeBlockParameters> n2PurgeBlock = recipeStep.BlockOne as ParameterizedRecipeBlock<N2PurgeBlockParameters>;
                            recipeBlockPreviewModel.PropertyOne = "Source: " + n2PurgeBlock.Parameters.Source;
                            recipeBlockPreviewModel.PropertyThree ="Time Interval: " + n2PurgeBlock.Parameters.TimeInterval + " " + n2PurgeBlock.Parameters.IntervalType;
                            break;
                    }
                    recipeBlockPreviewModels.Add(recipeBlockPreviewModel);
                }
                view.PreviewListBox.ItemsSource = new ObservableCollection<RecipeBlockPreviewModel>(recipeBlockPreviewModels);
                bool result = view.ShowDialog().Value;

            }
        }

        private ICommand addAboveSeqReciepeCommand;
        public ICommand AddAboveSeqReciepeCommand
        {
            get => addAboveSeqReciepeCommand ?? (addAboveSeqReciepeCommand = new DelegateCommand<object>(new Action<object>(AddAboveSeqReceipeExecute)));
            set => SetProperty(ref addAboveSeqReciepeCommand, value);
        }

        private ICommand deleteReciepeFromSequenceCommand;
        public ICommand DeleteReciepeFromSequenceCommand
        {
            get => deleteReciepeFromSequenceCommand ?? (deleteReciepeFromSequenceCommand = new DelegateCommand<object>(new Action<object>(DeleteReciepeFromSequenceExecute)));
            set => SetProperty(ref deleteReciepeFromSequenceCommand, value);
        }

        private ICommand stopSeqReceipeCommand;
        public ICommand StopSeqReceipeCommand
        {
            get => stopSeqReceipeCommand ?? (stopSeqReceipeCommand = new DelegateCommand(new Action(StopSeqRecipeCommand)));
            set => SetProperty(ref stopSeqReceipeCommand, value);
        }

        private void StopSeqRecipeCommand()
        {
            OpenEndBatchPopup();
            //IsSeqRecipeExecuting = false;
            //selectedSeqRecipeModel.IsExecuting = false;
            //AbortRecipeExecution();
        }

        private void AddAboveSeqReceipeExecute(object obj)
        {
            if (obj is SeqRecipeModel)
            {
                var seqRecipeModel = obj as SeqRecipeModel;
                var index = SeqRecipeModels.IndexOf(seqRecipeModel);
                var seqRecipeBuilder = recipeBuilder.InsertRecipeInSeq(index);
                if (seqRecipeBuilder != null)
                {
                    SeqRecipeModels.Insert(index, seqRecipeBuilder);
                }
                UpdateSeqRecipeCount();

                EndSeq = (uint)SeqRecipeModels.Count;
            }
        }

        private void DeleteReciepeFromSequenceExecute(object obj)
        {
            if (obj is SeqRecipeModel)
            {
                var seqRecipeModel = obj as SeqRecipeModel;
                if (recipeBuilder.DeleteRecipeFromSeq(seqRecipeModel))
                {
                    SeqRecipeModels.Remove(seqRecipeModel);
                    UpdateSeqRecipeCount();
                    if (SeqRecipeModels.Count < EndSeq)
                    {
                        EndSeq = (uint)SeqRecipeModels.Count;
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Properties

        public IList<IRecipeBlock> AvailableBlocks { get; private set; }

        public ObservableCollection<RecipeStepViewModel> RecipeSteps { get; }

        public RecipeStepViewModel SelectedStep
        {
            get
            {
                return selectedStep;
            }
            set
            {
                selectedStep = value;
                RaisePropertyChanged(nameof(SelectedStep));
            }
        }

        public int selectedIndex;

        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }
            set
            {
                if (value != null && value != selectedIndex)
                {
                    selectedIndex = value;
                    RaisePropertyChanged(nameof(SelectedIndex));
                }
            }
        }

        public string DeviceId
        {
            get => recipeBuilder.DeviceId;
            private set
            {
                recipeBuilder.DeviceId = value;
                RaisePropertyChanged();
            }
        }

        private string _fieldDeviceLabel;
        public string FieldDeviceLabel
        {
            get => _fieldDeviceLabel;
            set
            {
                _fieldDeviceLabel = value;
                RaisePropertyChanged();
            }
        }

        private string _recipeStatus;
        public string RecipeStatus
        {
            get => _recipeStatus;
            set
            {
                _recipeStatus = value;
                RaisePropertyChanged();
                UpdateIsEditEnable();
            }
        }

        private string _drainStatus;
        public string DrainStatus
        {
            get => _drainStatus;
            set
            {
                _drainStatus = value;
                RaisePropertyChanged();
                //UpdateIsEditEnable();
            }
        }

        private string _recipeEnded;
        public string RecipeEnded
        {
            get => _recipeEnded;
            set
            {
                _recipeEnded = value;
                RaisePropertyChanged();
            }

        }

        private bool isEditEnabled;
        public bool IsEditEnabled
        {
            get { return isEditEnabled; }
            set
            {
                isEditEnabled = value;
                RaisePropertyChanged();

            }
        }

        private bool isSaveChange;
        public bool IsSaveChange
        {
            get { return isSaveChange; }
            set { isSaveChange = value; RaisePropertyChanged(); }
        }


        private string _recipePaused;
        public string RecipePaused
        {
            get => _recipePaused;
            set
            {
                _recipePaused = value;
                RaisePropertyChanged();
            }
        }

        private NavigationParameters _navigationParameters;
        public NavigationParameters NavigationParameters
        {
            get => _navigationParameters ?? (_navigationParameters = new NavigationParameters());
            set => SetProperty(ref _navigationParameters, value);
        }

        #endregion

        private string _isRecipeRunning;
        public string IsRecipeRunning
        {
            get => _isRecipeRunning;
            set
            {
                _isRecipeRunning = value;
                RaisePropertyChanged();
                UpdateIsEditEnable();
            }
        }

        private Batch _newBatchDetails;
        public Batch NewBatchDetails
        {
            get => _newBatchDetails ?? (_newBatchDetails = new Batch());
            set => SetProperty(ref _newBatchDetails, value);
        }

        private bool _isBatchNameRequestPopUpOpen;
        public bool IsBatchNameRequestPopUpOpen
        {
            get => _isBatchNameRequestPopUpOpen;
            set
            {
                _isBatchNameRequestPopUpOpen = value;
                RaisePropertyChanged();
            }
        }
        private bool _isEndBatchPopUpOpen;

        public bool IsEndBatchPopUpOpen

        {
            get => _isEndBatchPopUpOpen;
            set
            {
                _isEndBatchPopUpOpen = value;
                RaisePropertyChanged();
            }
        }
        private Credential _adminCredential;
        public Credential AdminCredential
        {
            get => _adminCredential ?? (_adminCredential = new Credential());
            set
            {
                _adminCredential = value;
                RaisePropertyChanged();
            }
        }
    }

    public enum RecipeBlockStatus
    {
        NotConfigured,   // Null Recipe Block
        NotStarted,
        Started,
        Ended,
    }
}
