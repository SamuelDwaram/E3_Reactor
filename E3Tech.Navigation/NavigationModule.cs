using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Prism.Unity.Ioc;

namespace E3Tech.Navigation
{
    public class NavigationModule : IModule
    {
        private IRegionManager regionManager;

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            UnityContainerExtension unityContainerExtention = ((UnityContainerExtension)containerRegistry);
            unityContainerExtention.RegisterSingleton<IViewManager, ViewManager>();
            regionManager = unityContainerExtention.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("ViewsPane", typeof(ViewsPane));
        }
    }
}