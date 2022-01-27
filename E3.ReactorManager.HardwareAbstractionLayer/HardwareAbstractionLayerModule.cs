using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;
using Prism.Ioc;
using Prism.Modularity;

namespace E3.ReactorManager.HardwareAbstractionLayer
{
    public class HardwareAbstractionLayerModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.Resolve<IFieldDevicesCommunicator>();
            containerProvider.Resolve<FieldDevicesCommunicatorInstanceProvider>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IFieldDevicesCommunicator, FieldDevicesCommunicator>();
        }
    }
}
