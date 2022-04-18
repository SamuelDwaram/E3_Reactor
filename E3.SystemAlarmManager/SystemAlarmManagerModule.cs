using E3.SystemAlarmManager.Views;
using E3.SystemAlarmManager.Services;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using E3Tech.Navigation;

namespace E3.SystemAlarmManager
{
    public class SystemAlarmManagerModule : IModule
    {
        private readonly IRegionManager regionManager;
        private readonly IViewManager viewManager;

        public SystemAlarmManagerModule(IRegionManager regionManager, IViewManager viewManager)
        {
            this.regionManager = regionManager;
            this.viewManager = viewManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.Resolve<ISystemAlarmPoliciesManager>();
            containerProvider.Resolve<ISystemAlarmsManager>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<ISystemAlarmPoliciesManager, SystemAlarmPoliciesManager>();
            containerRegistry.RegisterSingleton<ISystemAlarmsManager, SystemAlarmsManager>();

            regionManager.RegisterViewWithRegion("SystemAlarms", typeof(SystemAlarmsView));
            regionManager.RegisterViewWithRegion("ConfigureAlarmPolicies", typeof(ConfigureAlarmPoliciesView));

            containerRegistry.RegisterForNavigation(typeof(SystemAlarmsManagerView), "Alarms");
            viewManager.AddView("Alarms");
        }
    }
}