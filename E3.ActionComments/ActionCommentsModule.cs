using E3.ActionComments.Model;
using E3.ActionComments.ViewModels;
using E3.ActionComments.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace E3.ActionComments
{
    public class ActionCommentsModule : IModule
    {
        IRegionManager regionManager;

        public ActionCommentsModule(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.Resolve<ActionCommentsViewModel>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<ActionCommentsViewModel>();
            containerRegistry.Register<IActionCommentsHandler, ActionCommentsHandler>();
            regionManager.RegisterViewWithRegion("ActionComments", typeof(ActionCommentsView));
        }
    }
}
