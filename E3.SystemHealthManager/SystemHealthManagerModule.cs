using E3.SystemHealthManager.Views;
using E3.SystemHealthManager.Services;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using E3.SystemHealthManager.ViewModels;

namespace E3.SystemHealthManager
{
    public class SystemHealthManagerModule : IModule
    {
        private readonly IRegionManager regionManager;

        public SystemHealthManagerModule(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.Resolve<ISystemFailurePoliciesManager>();
            containerProvider.Resolve<ISystemFailuresManager>();
            containerProvider.Resolve<SystemFailuresInDeviceViewModel>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<SystemFailuresInDeviceViewModel>();
            containerRegistry.RegisterSingleton<ISystemFailurePoliciesManager, SystemFailurePoliciesManager>();
            containerRegistry.RegisterSingleton<ISystemFailuresManager, SystemFailuresManager>();

            regionManager.RegisterViewWithRegion("ConfigureFailures", typeof(SystemFailuresConfigurationView));
            regionManager.RegisterViewWithRegion("SystemFailures", typeof(SystemFailuresView));
            regionManager.RegisterViewWithRegion("SystemFailuresInDevice", typeof(SystemFailuresInDeviceView));
        }
    }
}