using E3.ReactorManager.DesignExperiment.Model;
using E3.ReactorManager.DesignExperiment.Model.Data;
using E3.ReactorManager.Interfaces.DataAbstractionLayer;
using E3.ReactorManager.Interfaces.DataAbstractionLayer.Data;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;
using E3.UserManager.Model.Data;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace E3.ReactorManager.DesignExperiment.ViewModels
{
    public class DesignExperimentViewModel : BindableBase, IRegionMemberLifetime
    {
        private readonly IRegionManager regionManager;
        private readonly IDesignExperiment designExperiment;
        private readonly IDatabaseReader databaseReader;
        private readonly IFieldDevicesCommunicator fieldDevicesCommunicator;
        private readonly TaskScheduler taskScheduler;

        public DesignExperimentViewModel(IRegionManager regionManager, IDatabaseReader databaseReader, IDesignExperiment designExperiment, IFieldDevicesCommunicator fieldDevicesCommunicator)
        {
            taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            this.regionManager = regionManager;
            this.databaseReader = databaseReader;
            this.designExperiment = designExperiment;
            this.fieldDevicesCommunicator = fieldDevicesCommunicator;
        }

        #region Start Batch
        public void StartBatch()
        {
            Task.Factory.StartNew(() => designExperiment.StartBatch(NewBatchDetails))
                .ContinueWith(new Action<Task<bool>>((t) => {
                    if (t.IsCompleted)
                    {
                        if (t.Result)
                        {
                            Navigate("Dashboard");
                        }
                        else
                        {
                            MessageBox.Show("Batch Already Exists, Please Re-enter the Batch Details", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }), taskScheduler);
        }

        private bool CanStartBatch()
        {
            return !string.IsNullOrWhiteSpace(NewBatchDetails.DosingPumpUsage)
                && !string.IsNullOrWhiteSpace(NewBatchDetails.Name)
                && !string.IsNullOrWhiteSpace(NewBatchDetails.Number)
                && !string.IsNullOrWhiteSpace(NewBatchDetails.Stage)
                && !string.IsNullOrWhiteSpace(NewBatchDetails.ScientistName)
                && !string.IsNullOrWhiteSpace(NewBatchDetails.Comments);
        }
        #endregion

        public void Navigate(string screenName)
        {
            regionManager.RequestNavigate("SelectedViewPane", screenName, NavigationParameters);
        }

        public void GetAvailableFieldDevices()
        {
            Task.Factory.StartNew(new Func<Dictionary<string, string>>(() => {
                return (from DataRow row in databaseReader.ExecuteReadCommand("select * from dbo.FieldDevices where Type='Reactor'", CommandType.Text).AsEnumerable()
                        select new KeyValuePair<string, string>(row["Identifier"].ToString(), row["Label"].ToString())).ToDictionary(keyValuePair => keyValuePair.Key, keyValuePair => keyValuePair.Value);
            })).ContinueWith(t => {
                AvailableFieldDevices.Clear();
                foreach (KeyValuePair<string, string> keyValuePair in t.Result)
                {
                    if (fieldDevicesCommunicator.ReadFieldPointValue<bool>(keyValuePair.Key, "RunningBatchStatus"))
                    {
                        //Field Device is being used. Skip
                    }
                    else
                    {
                        AvailableFieldDevices.Add(keyValuePair.Key, keyValuePair.Value);
                    }
                }
                RaisePropertyChanged(nameof(AvailableFieldDevices));
            });
        }

        #region Update Connected Equipments for Selected Field Device
        public void UpdateSelectedFieldDeviceLabel(string deviceId)
        {
            if (AvailableFieldDevices.ContainsKey(deviceId))
            {
                NewBatchDetails.FieldDeviceLabel = AvailableFieldDevices[deviceId];
            }
        }

        public void UpdateConnectedHcForSelectedFieldDevice(string deviceId)
        {
            Task.Factory.StartNew(new Func<object, DataTable>(GetConnectedEquipments), deviceId)
                .ContinueWith(new Action<Task<DataTable>>((t) => {
                    if (t.IsCompleted)
                    {
                        foreach (DataRow row in t.Result.AsEnumerable())
                        {
                            if (row["EquipmentType"].ToString() == "HC")
                            {
                                NewBatchDetails.HCIdentifier = row["EquipmentName"].ToString();
                            }
                        }
                    }}));
        }

        public void UpdateConnectedStirrerForSelectedFieldDevice(string deviceId)
        {
            Task.Factory.StartNew(new Func<object, DataTable>(GetConnectedEquipments), deviceId)
                .ContinueWith(new Action<Task<DataTable>>((t) => {
                    if (t.IsCompleted)
                    {
                        foreach (DataRow row in t.Result.AsEnumerable())
                        {
                            if (row["EquipmentType"].ToString() == "Stirrer")
                            {
                                NewBatchDetails.StirrerIdentifier = row["EquipmentModel"].ToString();
                            }
                        }
                    }
                }));
        }

        private DataTable GetConnectedEquipments(object deviceId)
        {
            IList<DbParameterInfo> parameters = new List<DbParameterInfo>
            {
                new DbParameterInfo("@FieldDeviceIdentifier", (string)deviceId, DbType.String)
            };
            return databaseReader.ExecuteReadCommand("GetAllConnectedEquipments", CommandType.StoredProcedure, parameters);
        }
        #endregion

        public void OpenChemicalDatabase()
        {

        }

        public void UploadReactionImage()
        {

        }

        public void SelectDosingPumpUsage(string usageStatus)
        {
            NewBatchDetails.DosingPumpUsage = usageStatus.ToLower() == "yes" ? bool.TrueString : bool.FalseString;
        }

        #region Commands
        public ICommand LoadUserDetailsCommand
        {
            get => new DelegateCommand(() => {
                UserDetails = (User)Application.Current.Resources["LoggedInUser"];
                NewBatchDetails.ScientistName = UserDetails.Name;
            });
        }
        private ICommand _selectDosingPumpUsageCommand;
        public ICommand SelectDosingPumpUsageCommand
        {
            get => _selectDosingPumpUsageCommand ?? (_selectDosingPumpUsageCommand = new DelegateCommand<string>(SelectDosingPumpUsage));
            set => _selectDosingPumpUsageCommand = value;
        }

        private ICommand _openChemicalDatabaseCommand;
        public ICommand OpenChemicalDatabaseCommand
        {
            get => _openChemicalDatabaseCommand ?? (_openChemicalDatabaseCommand = new DelegateCommand(OpenChemicalDatabase));
            set => _openChemicalDatabaseCommand = value;
        }

        private ICommand _startBatchCommand;
        public ICommand StartBatchCommand
        {
            get => _startBatchCommand ?? (_startBatchCommand = new DelegateCommand(StartBatch, CanStartBatch)
                .ObservesProperty(() => NewBatchDetails.DosingPumpUsage)
                .ObservesProperty(() => NewBatchDetails.Name)
                .ObservesProperty(() => NewBatchDetails.Number)
                .ObservesProperty(() => NewBatchDetails.Stage)
                .ObservesProperty(() => NewBatchDetails.ScientistName)
                .ObservesProperty(() => NewBatchDetails.Comments));
            set => _startBatchCommand = value;
        }

        private ICommand _navigateCommand;
        public ICommand NavigateCommand
        {
            get => _navigateCommand ?? (_navigateCommand = new DelegateCommand<string>(Navigate));
            set => _navigateCommand = value;
        }

        private ICommand _getAvailableFieldDevicesCommand;
        public ICommand GetAvailableFieldDevicesCommand
        {
            get => _getAvailableFieldDevicesCommand ?? (_getAvailableFieldDevicesCommand = new DelegateCommand(GetAvailableFieldDevices));
            set => _getAvailableFieldDevicesCommand = value;
        }

        private ICommand _updateConnectedHcForSelectedFieldDeviceCommand;
        public ICommand UpdateConnectedHcForSelectedFieldDeviceCommand
        {
            get => _updateConnectedHcForSelectedFieldDeviceCommand ?? (_updateConnectedHcForSelectedFieldDeviceCommand = new DelegateCommand<string>(UpdateConnectedHcForSelectedFieldDevice));
            set => _updateConnectedHcForSelectedFieldDeviceCommand = value;
        }

        private ICommand _updateConnectedStirrerForSelectedFieldDeviceCommand;
        public ICommand UpdateConnectedStirrerForSelectedFieldDeviceCommand
        {
            get => _updateConnectedStirrerForSelectedFieldDeviceCommand ?? (_updateConnectedStirrerForSelectedFieldDeviceCommand = new DelegateCommand<string>(UpdateConnectedStirrerForSelectedFieldDevice));
            set => _updateConnectedStirrerForSelectedFieldDeviceCommand = value;
        }

        private ICommand _uploadReactionImageCommand;
        public ICommand UploadReactionImageCommand
        {
            get => _uploadReactionImageCommand ?? (_uploadReactionImageCommand = new DelegateCommand(UploadReactionImage));
            set => _uploadReactionImageCommand = value;
        }

        private ICommand _updateSelectedFieldDeviceLabelCommand;
        public ICommand UpdateSelectedFieldDeviceLabelCommand
        {
            get => _updateSelectedFieldDeviceLabelCommand ?? (_updateSelectedFieldDeviceLabelCommand = new DelegateCommand<string>(UpdateSelectedFieldDeviceLabel));
            set => _updateSelectedFieldDeviceLabelCommand = value;
        }
        #endregion

        #region Properties
        public bool KeepAlive
        {
            get => false;
        }

        private User _userDetails;
        public User UserDetails
        {
            get => _userDetails ?? (_userDetails = new User());
            set => SetProperty(ref _userDetails, value);
        }

        private Batch _newBatchDetails;
        public Batch NewBatchDetails
        {
            get => _newBatchDetails ?? (_newBatchDetails = new Batch());
            set => SetProperty(ref _newBatchDetails, value);
        }

        private NavigationParameters _navigationParameters;
        public NavigationParameters NavigationParameters
        {
            get => _navigationParameters ?? (_navigationParameters = new NavigationParameters());
            set => SetProperty(ref _navigationParameters, value);
        }

        private Dictionary<string, string> _availableFieldDevices;
        public Dictionary<string, string> AvailableFieldDevices
        {
            get => _availableFieldDevices ?? (_availableFieldDevices = new Dictionary<string, string>());
            set => SetProperty(ref _availableFieldDevices, value);
        }

        #endregion

    }
}
