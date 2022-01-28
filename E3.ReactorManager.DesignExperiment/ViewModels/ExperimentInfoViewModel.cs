using E3.Mediator.Models;
using E3.Mediator.Services;
using E3.ReactorManager.DesignExperiment.Model;
using E3.ReactorManager.DesignExperiment.Model.Data;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;
using E3.UserManager.Model.Data;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Unity;

namespace E3.ReactorManager.DesignExperiment.ViewModels
{
    public class ExperimentInfoViewModel : BindableBase
    {
        private readonly IRegionManager regionManager;
        private readonly MediatorService mediatorService;
        private readonly IExperimentInfoProvider experimentInfoProvider;
        private readonly IDesignExperiment designExperiment;
        private readonly TaskScheduler taskScheduler;
        private string deviceId = string.Empty;
        private User userDetails = new User();

        public ExperimentInfoViewModel(IUnityContainer containerProvider, IRegionManager regionManager, MediatorService mediatorService, IExperimentInfoProvider experimentInfoProvider)
        {
            taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            this.designExperiment = containerProvider.Resolve<IDesignExperiment>();
            this.regionManager = regionManager;
            this.mediatorService = mediatorService;
            this.experimentInfoProvider = experimentInfoProvider;
            this.mediatorService.Register(InMemoryMediatorMessageContainer.UpdateSelectedDeviceId, (d) => {
                deviceId = (d as Device).Id;
                GetBatchRunningInDevice((d as Device).Id);
                LoadUserDetails();
            });
        }

        private void LoadUserDetails()
        {
            userDetails = (User)Application.Current.Resources["LoggedInUser"];
        }

        #region GetCurrentBatchDetails
        public void GetBatchRunningInDevice(string deviceId)
        {
            this.deviceId = deviceId;
            Task.Factory.StartNew(new Func<object, Batch>(GetExperimentInfo), deviceId)
                    .ContinueWith(UpdateBatchDetailsInUI);
        }

        private void UpdateBatchDetailsInUI(Task<Batch> task)
        {
            if (task.IsCompleted)
            {
                CurrentBatchDetails = task.Result;
            }
        }

        private Batch GetExperimentInfo(object deviceId)
        {
            return experimentInfoProvider.GetBatchRunningInDevice((string)deviceId);
        }
        #endregion

        #region End Batch
        public void EndBatch()
        {
            Task.Factory.StartNew(new Action(TerminateBatch))
                .ContinueWith(new Action<Task>(NavigateToDashboard), taskScheduler);
        }

        public bool CanEndBatch()
        {
            return !string.IsNullOrWhiteSpace(AdminCredential.Username)
                && !string.IsNullOrWhiteSpace(AdminCredential.PasswordHash)
                && !string.IsNullOrWhiteSpace(CleaningSolvent)
                && !string.IsNullOrWhiteSpace(CurrentBatchDetails.Identifier);
        }

        private void NavigateToDashboard(Task task)
        {
            if (task.IsCompleted)
            {
                NavigationParameters parameters = new NavigationParameters()
                {
                    { "UserDetails", userDetails }
                };
                regionManager.RequestNavigate("SelectedViewPane", "Dashboard", parameters);
            }
        }

        private void TerminateBatch()
        {
            designExperiment.EndBatch(AdminCredential, CurrentBatchDetails.Identifier, CurrentBatchDetails.Name, CurrentBatchDetails.FieldDeviceIdentifier, CleaningSolvent);
        }
        #endregion

        public void Navigate(string screenName)
        {
            NavigationParameters parameters = new NavigationParameters()
            {
                { "UserDetails", userDetails }
            };
            regionManager.RequestNavigate("SelectedViewPane", screenName, parameters);
        }

        private void InitializeDeviceParameters()
        {
            
        }

        #region Commands
        private ICommand _initializeDeviceParametersCommand;
        public ICommand InitializeDeviceParametersCommand
        {
            get => _initializeDeviceParametersCommand ?? (_initializeDeviceParametersCommand = new DelegateCommand(InitializeDeviceParameters));
            set => SetProperty(ref _initializeDeviceParametersCommand, value);
        }

        private ICommand _navigateCommand;
        public ICommand NavigateCommand
        {
            get => _navigateCommand ?? (_navigateCommand = new DelegateCommand<string>(Navigate));
            set => _navigateCommand = value;
        }

        private ICommand _endBatchCommand;
        public ICommand EndBatchCommand
        {
            get => _endBatchCommand ?? (_endBatchCommand = new DelegateCommand(EndBatch)
                .ObservesProperty(() => AdminCredential.Username)
                .ObservesProperty(() => AdminCredential.PasswordHash)
                .ObservesProperty(() => CleaningSolvent)
                .ObservesProperty(() => CurrentBatchDetails.Identifier));
            set => _endBatchCommand = value;
        }
        #endregion

        #region Properties
        
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

        private string _cleaningSolvent;
        public string CleaningSolvent
        {
            get => _cleaningSolvent;
            set
            {
                _cleaningSolvent = value;
                RaisePropertyChanged();
            }
        }

        #endregion
    }
}
