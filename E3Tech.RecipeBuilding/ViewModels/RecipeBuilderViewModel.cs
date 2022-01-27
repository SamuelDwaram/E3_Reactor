using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer.Data;
using E3Tech.RecipeBuilding.Helpers;
using E3Tech.RecipeBuilding.Model;
using E3Tech.RecipeBuilding.Model.Blocks;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Unity;
using E3Tech.RecipeBuilding.Model.RecipeExecutionInfoProvider;
using Prism.Regions;

namespace E3Tech.RecipeBuilding.ViewModels
{
    public class RecipeBuilderViewModel : BindableBase
    {
        private readonly IRecipeBuilder recipeBuilder;
        private readonly IRecipeReloader recipeReloader;
        private readonly IRecipeExecutor recipeExecutor;
        private readonly IUnityContainer containerProvider;
        private RecipeStepViewModel selectedStep;
        private readonly IFieldDevicesCommunicator fieldDevicesCommunicator;
        private readonly List<PropertyInfo> existingProperties = new List<PropertyInfo>(typeof(RecipeBuilderViewModel).GetProperties());
        private readonly TaskScheduler taskScheduler;
        private readonly IRecipeExecutionInfoProvider recipeExecutionInfoProvider;
        private Dictionary<SeqRecipeModel, IList<RecipeStep>> recipeSeqDetail;


        public RecipeBuilderViewModel(IUnityContainer containerProvider, IRecipeExecutor recipeExecutor, IFieldDevicesCommunicator fieldDevicesCommunicator, IRecipeBuilder recipeBuilder, IRecipeReloader recipeReloader)
        {
            taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
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
            /*
             * If DeviceId has not started any Recipe and application was running fine
             * then Load recipe from Recipe Builder
             */
            foreach (RecipeStep step in recipeBuilder.RecipeSteps)
            {
                RecipeSteps.Add(new RecipeStepViewModel(containerProvider) { RecipeStep = step });
            }
            if (IsSeqRecipeExecuting)
            {
                recipeSeqDetail = recipeBuilder.LoadSeqRecipeList();
                bool recipeStatus = recipeReloader.GetRecipeStatus(DeviceId);
                bool recipeEndedStatus = recipeReloader.GetRecipeEndedStatus(DeviceId);
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
                if (index < SeqRecipeModels.Count - 1)
                {
                    var keyvalueRecipeDetail = recipeSeqDetail.Where(x => x.Key.RecipeGuidId == SeqRecipeModels[index + 1].RecipeGuidId).FirstOrDefault();
                    MessageBoxResult result = MessageBox.Show(string.Format(string.Format("Do You want to execute : {0} ", keyvalueRecipeDetail.Key.RecipeName)), "Information", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        var recipeSteps = keyvalueRecipeDetail.Value;
                        recipeBuilder.UpdateRecipeList(recipeSteps);

                        RecipeSteps.Clear();
                        foreach (RecipeStep step in recipeSteps)
                        {
                            RecipeStepViewModel stepViewModel = containerProvider.Resolve<RecipeStepViewModel>();
                            stepViewModel.RecipeStep = step;
                            RecipeSteps.Add(stepViewModel);
                        }
                        selectedSeqRecipeModel = keyvalueRecipeDetail.Key;
                        SelectedSeqRecipeModel = keyvalueRecipeDetail.Key;

                        var isExecuting = ExecuteRecipe();
                        if (isExecuting)
                        {
                            keyvalueRecipeDetail.Key.IsExecuting = true;
                            recipeBuilder.SaveSeqRecipeWhileExecuting(recipeSeqDetail.Keys.ToList());
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
            RecipeEnded = fieldDevicesCommunicator.ReadFieldPointValue<bool>(DeviceId, "RecipeEnded").ToString();
            RecipePaused = fieldDevicesCommunicator.ReadFieldPointValue<bool>(DeviceId, "PauseRecipe").ToString();
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
                foreach (RecipeStep step in recipeSteps)
                {
                    RecipeStepViewModel stepViewModel = containerProvider.Resolve<RecipeStepViewModel>();
                    stepViewModel.RecipeStep = step;
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
            if (recipeBuilder.CheckEndBlockInRecipe(recipeBuilder.RecipeSteps))
            {
                recipeExecutor.Execute(DeviceId, recipeBuilder.RecipeSteps);
                return true;
            }
            else
            {
                MessageBox.Show("Please Add End Block in the Recipe",
                                "Recipe Execution Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            return false;
        }

        private void PauseRecipe()
        {
            fieldDevicesCommunicator
               .SendCommandToDevice(DeviceId,
                                          "PauseRecipe",
                                          "bool",
                                          Boolean.TrueString);
        }

        private bool CanStartRecipe()
        {
            return recipeExecutor != null;
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
            StartRecipe();
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
            }
        }

        private void ClearSeqReceipeExecute()
        {
            ClearRecipe();
            recipeBuilder.ClearSeqRecipe();
            SeqRecipeModels.Clear();
            recipeBuilder.DeleteSeqRecipe();
        }

        private void StartSeqReceipeCommandExecute()
        {
            if (SeqRecipeModels?.Count > 0 && recipeBuilder.ValidateSeqRecipe())
            {
                IsSeqRecipeExecuting = true;
                ClearRecipe();
                foreach (var item in SeqRecipeModels)
                {
                    item.IsExecuted = false;
                    item.IsExecuting = false;
                }
                recipeSeqDetail = recipeBuilder.LoadSeqRecipeList();
                recipeBuilder.UpdateRecipeList(recipeSeqDetail.First().Value);
                selectedSeqRecipeModel = recipeSeqDetail.First().Key;
                SelectedSeqRecipeModel = recipeSeqDetail.First().Key;
                foreach (var step in recipeSeqDetail.First().Value.ToList())
                {
                    RecipeStepViewModel stepViewModel = containerProvider.Resolve<RecipeStepViewModel>();
                    stepViewModel.RecipeStep = step;
                    RecipeSteps.Add(stepViewModel);
                }

                var result = ExecuteRecipe();

                if (result == true)
                {
                    recipeSeqDetail.First().Key.IsExecuting = true;
                    recipeBuilder.SaveSeqRecipeWhileExecuting(recipeSeqDetail.Keys.ToList());
                }
            }
        }



        private void MouseDoubleClickExecute()
        {
            SeqRecipeModel seqRecipeModel = recipeBuilder.ImportWithFile();
            if (seqRecipeModel != null)
            {
                SeqRecipeModels.Add(seqRecipeModel);
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

                var seqRecipeList = recipeBuilder.ReloadSeqRecipes();
                if (seqRecipeList?.Count > 0)
                {
                    SeqRecipeModels = new ObservableCollection<SeqRecipeModel>(seqRecipeList);
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

            IRecipeBlock block = GetBlockInstance(parameters.DataObject);
            int recipeStepIndex = RecipeSteps.IndexOf(parameters.DataContext as RecipeStepViewModel);
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

                if (isBlockValid)
                {
                    block.Configure(containerProvider);
                    UpdateRecipeToExecuter(block, recipeStepIndex);
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
                if (!firstStep)
                {
                    RecipeSteps.Add(stepViewModel);
                }

                block.Configure(containerProvider);
                SelectedStep = stepViewModel;
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
                RecipeSteps.Insert(toBeAddedRecipeStepIndex, stepViewModel);
                block.Configure(containerProvider);
                SelectedStep = stepViewModel;

                //if (bool.Parse(RecipeStatus))
                //{
                //    // If Recipe is running start it again to update all the steps to the plc
                //    StartRecipe();
                //}
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

        private ICommand startRecipeCommand;
        public ICommand StartRecipeCommand
        {
            get => startRecipeCommand ?? (startRecipeCommand = new DelegateCommand(new Action(StartRecipe), new Func<bool>(CanStartRecipe)));
            set => SetProperty(ref startRecipeCommand, value);
        }
        private ICommand pauseRecipeCommand;
        public ICommand PauseRecipeCommand
        {
            get => pauseRecipeCommand ?? (pauseRecipeCommand = new DelegateCommand(new Action(PauseRecipe)));
            set => SetProperty(ref pauseRecipeCommand, value);
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
            IsSeqRecipeExecuting = false;
            selectedSeqRecipeModel.IsExecuting = false;
            AbortRecipeExecution();
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
    }

    public enum RecipeBlockStatus
    {
        NotConfigured,   // Null Recipe Block
        NotStarted,
        Started,
        Ended,
    }
}
