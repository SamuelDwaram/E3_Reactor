using E3.ReactorManager.Interfaces.DataAbstractionLayer;
using E3.SystemHealthManager.Models;
using E3.SystemHealthManager.Services;
using Prism.Mvvm;
using System;
using System.Windows.Input;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using E3.Mediator.Services;
using E3.Mediator.Models;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;

namespace E3.SystemHealthManager.ViewModels
{
    public class SystemFailuresConfigurationViewModel : BindableBase
    {
        public MediatorService mediatorService;
        private readonly IDatabaseReader databaseReader;
        private readonly ISystemFailurePoliciesManager systemFailurePoliciesManager;

        public SystemFailuresConfigurationViewModel(IDatabaseReader databaseReader, ISystemFailurePoliciesManager systemFailurePoliciesManager, MediatorService mediatorService)
        {
            this.mediatorService = mediatorService;
            this.mediatorService.Register(InMemoryMediatorMessageContainer.UpdateDeviceIdForAlarmConfiguration, UpdateConfigureFailuresView);

            this.databaseReader = databaseReader;
            this.systemFailurePoliciesManager = systemFailurePoliciesManager;
            this.systemFailurePoliciesManager.RefreshSystemFailurePolicies += SystemFailurePoliciesManager_RefreshSystemFailurePolicies;

            Task.Factory.StartNew(new Func<IList<Device>>(LoadAvailableDevices))
                .ContinueWith((t) => AvailableDevices = t.Result);
        }

        private void UpdateConfigureFailuresView(object device)
        {
            SelectedDevice = device as Device;
            UpdateDeviceFailurePoliciesCommand.Execute(SelectedDevice);
        }

        private void SystemFailurePoliciesManager_RefreshSystemFailurePolicies(IList<SystemFailurePolicy> systemFailurePolicies, string deviceId = null)
        {
            DeviceFailurePolicies = systemFailurePolicies.Where(p => p.DeviceId == SelectedDevice.Id).ToList();
        }

        public IList<Device> LoadAvailableDevices()
        {
            return (from DataRow row in databaseReader.ExecuteReadCommand("GetAvailableFieldDevices", CommandType.StoredProcedure).AsEnumerable()
                    select new Device
                    {
                        Id = row["Identifier"].ToString(),
                        Label = row["Label"].ToString()
                    }).ToList();
        }

        public IList<SystemFailurePolicy> GetDeviceFailurePolicies(object device)
        {
            Device selectedDevice = (Device)device;

            IEnumerable<SystemFailurePolicy> systemFailurePoliciesForDevice = systemFailurePoliciesManager.GetPoliciesForDevice(selectedDevice.Id);
            if (systemFailurePoliciesForDevice.Any())
            {
                return systemFailurePoliciesForDevice.ToList();
            }
            else
            {
                IList<string> deviceParameters
                    = (from DataRow row in databaseReader.ExecuteReadCommand($"select Label from dbo.FieldPoints where FieldDeviceIdentifier='{selectedDevice.Id}'",
                                                CommandType.Text).AsEnumerable()
                       select Convert.ToString(row["Label"])).ToList();
                return (from parameter in deviceParameters
                        select new SystemFailurePolicy
                        {
                            DeviceId = selectedDevice.Id,
                            DeviceLabel = selectedDevice.Label,
                            FailedResourceLabel = parameter,
                            TargetValue = "NC",
                            Title = $"{parameter} failure",
                            TroubleShootMessage = "Please check connections"
                        }).ToList();
            }
        }

        public void UpdateDeviceFailurePolicies(Device device)
        {
            Task.Factory.StartNew(new Func<object, IList<SystemFailurePolicy>>(GetDeviceFailurePolicies), device)
                .ContinueWith((t) => {
                    DeviceFailurePolicies.Clear();
                    DeviceFailurePolicies = t.Result;
                    RaisePropertyChanged(nameof(DeviceFailurePolicies));
                });
        }

        public void SaveChangesInFailurePolicies()
        {
            Task.Factory.StartNew(() =>
            {
                foreach (SystemFailurePolicy systemFailurePolicy in DeviceFailurePolicies)
                {
                    //configure properties for SystemFailurePolicy
                    if (string.IsNullOrWhiteSpace(systemFailurePolicy.Title))
                    {
                        if (systemFailurePolicy.FailureResourceType == FailureResourceType.Controller)
                        {
                            systemFailurePolicy.Title = "Slave Failure";
                        }
                        else
                        {
                            systemFailurePolicy.Title = "Parameter Failure";
                        }
                    }
                    systemFailurePoliciesManager.AddSystemFailurePolicy(systemFailurePolicy);
                }
            });
        }

        public void EnableFailurePolicy(object policyId)
        {
            if (policyId == null)
            {
                return;
            }
            systemFailurePoliciesManager.ModifyFailurePolicyStatus((int)policyId, true);
        }

        public void DisableFailurePolicy(object policyId)
        {
            if (policyId == null)
            {
                return;
            }
            systemFailurePoliciesManager.ModifyFailurePolicyStatus((int)policyId, false);
        }

        public void ToggleAdminPopupView()
        {
            if (IsAdminParametersViewOpen)
            {
                IsAdminParametersViewOpen = false;
            }
            else
            {
                IsAdminPopupOpen = true;
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
            IsAdminPopupOpen = false;

            //Clear Username and Pssword
            AdminCredentials.Username = "";
            AdminCredentials.Password = "";

            RaisePropertyChanged(nameof(AdminCredentials));
        }

        #region Commands
        private ICommand _enableFailurePolicyCommand;
        public ICommand EnableFailurePolicyCommand
        {
            get => _enableFailurePolicyCommand ?? (_enableFailurePolicyCommand = new DelegateCommand<object>(EnableFailurePolicy));
            set => SetProperty(ref _enableFailurePolicyCommand, value);
        }

        private ICommand _disableFailurePolicyCommand;
        public ICommand DisableFailurePolicyCommand
        {
            get => _disableFailurePolicyCommand ?? (_disableFailurePolicyCommand = new DelegateCommand<object>(DisableFailurePolicy));
            set => SetProperty(ref _disableFailurePolicyCommand, value);
        }

        private ICommand _updateDeviceFailurePoliciesCommand;
        public ICommand UpdateDeviceFailurePoliciesCommand
        {
            get => _updateDeviceFailurePoliciesCommand ?? (_updateDeviceFailurePoliciesCommand = new DelegateCommand<Device>(UpdateDeviceFailurePolicies));
            set => SetProperty(ref _updateDeviceFailurePoliciesCommand, value);
        }

        private ICommand _saveChangesInFailurePoliciesCommand;
        public ICommand SaveChangesInFailurePoliciesCommand
        {
            get => _saveChangesInFailurePoliciesCommand ?? (_saveChangesInFailurePoliciesCommand = new DelegateCommand(SaveChangesInFailurePolicies));
            set => SetProperty(ref _saveChangesInFailurePoliciesCommand, value);
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
        public IList<SystemFailurePolicy> DeviceFailurePolicies { get; set; } = new List<SystemFailurePolicy>();
        
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

        private bool _isAdminPopupOpen;
        public bool IsAdminPopupOpen
        {
            get => _isAdminPopupOpen;
            set => SetProperty(ref _isAdminPopupOpen, value);
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
