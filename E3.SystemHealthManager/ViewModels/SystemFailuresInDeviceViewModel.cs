using E3.SystemHealthManager.Models;
using Prism.Mvvm;
using System.Collections.Generic;
using E3.Mediator.Services;
using E3.Mediator.Models;
using E3.SystemHealthManager.Services;
using System.Linq;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;

namespace E3.SystemHealthManager.ViewModels
{
    public class SystemFailuresInDeviceViewModel : BindableBase
    {
        private readonly MediatorService mediatorService;
        private readonly ISystemFailuresManager systemFailuresManager;

        public SystemFailuresInDeviceViewModel(MediatorService mediatorService, ISystemFailuresManager systemFailuresManager)
        {
            this.mediatorService = mediatorService;
            this.systemFailuresManager = systemFailuresManager;
            this.systemFailuresManager.RefreshSystemFailures += SystemFailuresManager_RefreshSystemFailures;
            RegisterMediatorServices();
        }

        private void SystemFailuresManager_RefreshSystemFailures(IEnumerable<SystemFailure> systemFailures)
        {
            SystemFailures = systemFailures.Where(f => f.DeviceId == DeviceId);
        }

        private void RegisterMediatorServices()
        {
            mediatorService.Register(InMemoryMediatorMessageContainer.UpdateSelectedDeviceId, UpdateSystemFailuresInDevice);
        }

        private void UpdateSystemFailuresInDevice(object obj)
        {
            Device device = obj as Device;
            SystemFailures = systemFailuresManager.GetSystemFailuresForDevice(device.Id);
        }

        #region Properties
        private string _deviceId;
        public string DeviceId
        {
            get => _deviceId;
            set => SetProperty(ref _deviceId, value);
        }
        
        private IEnumerable<SystemFailure> _systemFailures;
        public IEnumerable<SystemFailure> SystemFailures
        {
            get => _systemFailures ?? (_systemFailures = new List<SystemFailure>());
            set => SetProperty(ref _systemFailures, value);
        }
        #endregion

    }
}
