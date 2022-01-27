using E3.DialogServices.Model;
using E3.DialogServices.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace E3.DialogServices
{
    public class DialogServicesModule : IModule
    {
        private readonly IRegionManager regionManager;

        public DialogServicesModule(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IDialogServiceProvider, DialogServiceProvider>();
            regionManager.RegisterViewWithRegion("DialogService", typeof(DialogServiceView));
        }
    }
}
