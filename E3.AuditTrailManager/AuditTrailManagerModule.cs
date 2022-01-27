using E3.AuditTrailManager.Model;
using E3.AuditTrailManager.Views;
using Prism.Ioc;
using Prism.Modularity;

namespace E3.AuditTrailManager
{
    public class AuditTrailManagerModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.Resolve<Model.AuditTrailManager>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IAuditTrailManager, Model.AuditTrailManager>();
            containerRegistry.RegisterForNavigation(typeof(AuditTrailView), "AuditTrail");
        }
    }
}
