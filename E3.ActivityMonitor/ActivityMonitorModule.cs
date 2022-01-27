using E3.ActivityMonitor.Services;
using Prism.Ioc;
using Prism.Modularity;

namespace E3.ActivityMonitor
{
    public class ActivityMonitorModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.Resolve<IActivityMonitor>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IActivityMonitor, DefaultActivityMonitor>();
        }
    }
}