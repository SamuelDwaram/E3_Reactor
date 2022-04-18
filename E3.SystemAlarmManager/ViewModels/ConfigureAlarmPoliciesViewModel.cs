using E3.ReactorManager.Interfaces.DataAbstractionLayer;
using E3.ReactorManager.Interfaces.DataAbstractionLayer.Data;
using E3.SystemAlarmManager.Models;
using E3.SystemAlarmManager.Services;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using E3.Mediator.Services;
using E3.Mediator.Models;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;

namespace E3.SystemAlarmManager.ViewModels
{
    public class ConfigureAlarmPoliciesViewModel : BindableBase
    {
        private readonly MediatorService mediatorService;
        private readonly IDatabaseReader databaseReader;
        private readonly ISystemAlarmPoliciesManager systemAlarmPoliciesManager;

        public ConfigureAlarmPoliciesViewModel(MediatorService mediatorService, IDatabaseReader databaseReader, ISystemAlarmPoliciesManager systemAlarmPoliciesManager)
        {
            this.mediatorService = mediatorService;
            this.databaseReader = databaseReader;
            this.systemAlarmPoliciesManager = systemAlarmPoliciesManager;
            this.systemAlarmPoliciesManager.RefreshSystemAlarmPolicies += SystemAlarmPoliciesManager_RefreshSystemAlarmPolicies;

            Task.Factory.StartNew(new Func<IList<Device>>(LoadAvailableDevices))
                .ContinueWith((t) => AvailableDevices = t.Result);
        }

        private void SystemAlarmPoliciesManager_RefreshSystemAlarmPolicies(IList<SystemAlarmPolicy> systemAlarmPolicies, string deviceId = null)
        {
            DeviceAlarmPolicies = systemAlarmPolicies.Where(p => p.DeviceId == SelectedDevice.Id).ToList();
        }

        public IList<Device> LoadAvailableDevices()
        {
            return (from DataRow row in databaseReader.ExecuteReadCommand("select * from dbo.FieldDevices", CommandType.Text).AsEnumerable()
                    select new Device
                    {
                        Id = row["Identifier"].ToString(),
                        Label = row["Label"].ToString()
                    }).ToList();
        }

        public IList<SystemAlarmPolicy> GetDeviceAlarmPoliciesForIndividualParameters(object device)
        {
            Device selectedDevice = (Device)device;
            IList<DbParameterInfo> dbParameters = new List<DbParameterInfo>
                {
                    new DbParameterInfo("FieldDeviceIdentifier", selectedDevice.Id, DbType.String)
                };
            IList<string> deviceParameters
                = (from DataRow row in databaseReader.ExecuteReadCommand($"select Label from dbo.FieldPoints where FieldDeviceIdentifier='{selectedDevice.Id}'",
                                            CommandType.Text, dbParameters).AsEnumerable()
                   select Convert.ToString(row["Label"])).ToList();
            return (from parameter in deviceParameters
                    select new SystemAlarmPolicy
                    {
                        DeviceId = selectedDevice.Id,
                        DeviceLabel = selectedDevice.Label,
                        PolicyType = PolicyType.Individual,
                        Parameters = new SystemAlarmParameters
                        {
                            Name = parameter
                        }
                    }).ToList();
        }

        public IList<SystemAlarmPolicy> GetDeviceAlarmPoliciesForGroupParameters(object device)
        {
            Device selectedDevice = (Device)device;
            IList<DbParameterInfo> dbParameters = new List<DbParameterInfo>
                {
                    new DbParameterInfo("FieldDeviceIdentifier", selectedDevice.Id, DbType.String)
                };
            IList<string> parameterGroups
                = (from DataRow row in databaseReader.ExecuteReadCommand($"select DataUnit from dbo.SensorsDataSet where FieldDeviceIdentifier='{selectedDevice.Id}' and DataUnit is not null",
                                            CommandType.Text, dbParameters).AsEnumerable()
                   select Convert.ToString(row["DataUnit"])).ToList();
            return (from parameter in parameterGroups
                    select new SystemAlarmPolicy
                    {
                        DeviceId = selectedDevice.Id,
                        DeviceLabel = selectedDevice.Label,
                        PolicyType = PolicyType.Group,
                        Parameters = new SystemAlarmParameters
                        {
                            Name = parameter
                        }
                    }).ToList();
        }

        public void UpdateDeviceAlarmPolicies(Device device)
        {
            // Notify other colleagues
            mediatorService.NotifyColleagues(InMemoryMediatorMessageContainer.UpdateDeviceIdForAlarmConfiguration, device);
            Task.Factory.StartNew(new Func<IEnumerable<SystemAlarmPolicy>>(() => systemAlarmPoliciesManager.GetPoliciesForDevice(device.Id)))
                .ContinueWith(systemAlarmPolicies => {
                    if (systemAlarmPolicies.Result.Count() > 0)
                    {
                        DeviceAlarmPolicies = systemAlarmPolicies.Result.ToList();
                    }
                    else
                    {
                        Task.Factory.StartNew(new Func<string>(() => {
                            IList<DbParameterInfo> dbParameters = new List<DbParameterInfo>
                            {
                                new DbParameterInfo("@DeviceId", device.Id, DbType.String),
                            };
                            return databaseReader.ExecuteReadCommand("GetMaxDevicePoliciesType", CommandType.StoredProcedure, dbParameters).AsEnumerable()
                                .First().Field<string>(0);
                        })).ContinueWith(policyType => {
                            DeviceAlarmPoliciesSortOrder = (PolicyType)Enum.Parse(typeof(PolicyType), policyType.Result);
                            if (policyType.Result == PolicyType.Group.ToString())
                            {
                                Task.Factory.StartNew(new Func<object, IList<SystemAlarmPolicy>>(GetDeviceAlarmPoliciesForGroupParameters), device)
                                    .ContinueWith((t) => DeviceAlarmPolicies = t.Result);
                            }
                            else
                            {
                                Task.Factory.StartNew(new Func<object, IList<SystemAlarmPolicy>>(GetDeviceAlarmPoliciesForIndividualParameters), device)
                                    .ContinueWith((t) => DeviceAlarmPolicies = t.Result);
                            }
                        });
                    }
                });
        }

        public void ReorderPolicies(string sortOrder)
        {
            DeviceAlarmPoliciesSortOrder = (PolicyType)Enum.Parse(typeof(PolicyType), sortOrder);
            if (SelectedDevice != new Device())
            {
                UpdateDeviceAlarmPolicies(SelectedDevice);
            }
        }

        public void SaveChangesInAlarmPolicies()
        {
            Task.Factory.StartNew(() =>
            {
                foreach (SystemAlarmPolicy systemAlarmPolicy in DeviceAlarmPolicies)
                {
                    //configure properties for SystemAlarmPolicy
                    if (string.IsNullOrWhiteSpace(systemAlarmPolicy.Title))
                    {
                        if (systemAlarmPolicy.Parameters.VariationType == AlarmParametersVariationType.Above)
                        {
                            systemAlarmPolicy.Title = $"Over {systemAlarmPolicy.Parameters.Name}";
                        }
                        else if (systemAlarmPolicy.Parameters.VariationType == AlarmParametersVariationType.Below)
                        {
                            systemAlarmPolicy.Title = $"Under {systemAlarmPolicy.Parameters.Name}";
                        }
                        else if (systemAlarmPolicy.Parameters.VariationType == AlarmParametersVariationType.Both)
                        {
                            systemAlarmPolicy.Title = $"Over&Under& {systemAlarmPolicy.Parameters.Name}";
                        }
                    }
                    systemAlarmPoliciesManager.AddSystemAlarmPolicy(systemAlarmPolicy);
                }
            });
        }

        public void EnableAlarmPolicy(object policyId)
        {
            if (policyId == null)
            {
                return;
            }
            systemAlarmPoliciesManager.ModifyAlarmPolicyStatus((int)policyId, true);
        }

        public void DisableAlarmPolicy(object policyId)
        {
            if (policyId == null)
            {
                return;
            }
            systemAlarmPoliciesManager.ModifyAlarmPolicyStatus((int)policyId, false);
        }

        public void ToggleAdminPopupView()
        {
            if (IsAdminParametersViewOpen)
            {
                IsAdminParametersViewOpen = false;    
            }
            else
            {
                IsAdminPopupStatusOpen = true;
            }
        }

        public void ValidateAdminCredentials()
        {
            if (AdminCredentials.Username == "e3techllp" && AdminCredentials.Password == "e3techllp")
            {
                IsAdminParametersViewOpen = true;
            }
            else
            {
                IsAdminParametersViewOpen = false;
            }

            //Close Popup
            IsAdminPopupStatusOpen = false;

            //Clear Username and Pssword
            AdminCredentials.Username = "";
            AdminCredentials.Password = "";

            RaisePropertyChanged(nameof(AdminCredentials));
        }

        #region Commands
        private ICommand _reorderPoliciesCommand;
        public ICommand ReorderPoliciesCommand
        {
            get => _reorderPoliciesCommand ?? (_reorderPoliciesCommand = new DelegateCommand<string>(ReorderPolicies));
            set => SetProperty(ref _reorderPoliciesCommand, value);
        }

        private ICommand _enableAlarmPolicyCommand;
        public ICommand EnableAlarmPolicyCommand
        {
            get => _enableAlarmPolicyCommand ?? (_enableAlarmPolicyCommand = new DelegateCommand<object>(EnableAlarmPolicy));
            set => SetProperty(ref _enableAlarmPolicyCommand, value);
        }

        private ICommand _disableAlarmPolicyCommand;
        public ICommand DisableAlarmPolicyCommand
        {
            get => _disableAlarmPolicyCommand ?? (_disableAlarmPolicyCommand = new DelegateCommand<object>(DisableAlarmPolicy));
            set => SetProperty(ref _disableAlarmPolicyCommand, value);
        }

        private ICommand _updateDeviceAlarmPoliciesCommand;
        public ICommand UpdateDeviceAlarmPoliciesCommand
        {
            get => _updateDeviceAlarmPoliciesCommand ?? (_updateDeviceAlarmPoliciesCommand = new DelegateCommand<Device>(UpdateDeviceAlarmPolicies));
            set => SetProperty(ref _updateDeviceAlarmPoliciesCommand, value);
        }

        private ICommand _saveChangesInAlarmPoliciesCommand;
        public ICommand SaveChangesInAlarmPoliciesCommand
        {
            get => _saveChangesInAlarmPoliciesCommand ?? (_saveChangesInAlarmPoliciesCommand = new DelegateCommand(SaveChangesInAlarmPolicies));
            set => SetProperty(ref _saveChangesInAlarmPoliciesCommand, value);
        }

        private ICommand _toggleAdminPopupCommand;
        public ICommand ToggleAdminPopupCommand
        {
            get => _toggleAdminPopupCommand ?? (_toggleAdminPopupCommand = new DelegateCommand(ToggleAdminPopupView));
            set => SetProperty(ref _toggleAdminPopupCommand, value);
        }

        private ICommand _validateAdminCredentialsCommand;
        public ICommand ValidateAdminCredentialsCommand
        {
            get => _validateAdminCredentialsCommand ?? (_validateAdminCredentialsCommand = new DelegateCommand(ValidateAdminCredentials));
            set => SetProperty(ref _validateAdminCredentialsCommand, value);
        }
        #endregion

        #region Properties
        private IList<SystemAlarmPolicy> _deviceAlarmPolicies;
        public IList<SystemAlarmPolicy> DeviceAlarmPolicies
        {
            get => _deviceAlarmPolicies ?? (_deviceAlarmPolicies = new List<SystemAlarmPolicy>());
            set => SetProperty(ref _deviceAlarmPolicies, value);
        }

        private PolicyType _deviceAlarmPoliciesSortOrder = PolicyType.Group;
        public PolicyType DeviceAlarmPoliciesSortOrder
        {
            get => _deviceAlarmPoliciesSortOrder;
            set => SetProperty(ref _deviceAlarmPoliciesSortOrder, value);
        }

        private IList<Device> _availableDevices;
        public IList<Device> AvailableDevices
        {
            get => _availableDevices ?? (_availableDevices = new List<Device>());
            set => SetProperty(ref _availableDevices, value);
        }

        private Device _selectedDevice;
        public Device SelectedDevice
        {
            get => _selectedDevice ?? (_selectedDevice = new Device());
            set => SetProperty(ref _selectedDevice, value);
        }

        private bool _isAdminPopupStatusOpen;
        public bool IsAdminPopupStatusOpen
        {
            get => _isAdminPopupStatusOpen;
            set => SetProperty(ref _isAdminPopupStatusOpen, value);
        }

        private bool _isAdminParametersViewOpen;
        public bool IsAdminParametersViewOpen
        {
            get => _isAdminParametersViewOpen;
            set => SetProperty(ref _isAdminParametersViewOpen, value);
        }

        private AdminCredentials _adminCredentials;
        public AdminCredentials AdminCredentials
        {
            get => _adminCredentials ?? (_adminCredentials = new AdminCredentials());
            set => SetProperty(ref _adminCredentials, value);
        }
        #endregion
    }
}
