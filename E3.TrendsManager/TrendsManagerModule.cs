using E3.TrendsManager.Services;
using E3.TrendsManager.ViewModels;
using E3.TrendsManager.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace E3.TrendsManager
{
    public class TrendsManagerModule : IModule
    {
        private readonly IRegionManager regionManager;

        public TrendsManagerModule(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
 
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<LiveTrendsViewModel>();
            containerRegistry.RegisterSingleton<ITrendsManager, DefaultTrendsManager>();
            containerRegistry.RegisterSingleton<ILiveTrendManager, LiveTrendManager>();
            regionManager.RegisterViewWithRegion("Trends", typeof(TrendsView));
            regionManager.RegisterViewWithRegion("LiveTrends", typeof(LiveTrendsView));

            containerRegistry.RegisterForNavigation(typeof(NavigableTrendsView), "Trends");
        }
    }
}