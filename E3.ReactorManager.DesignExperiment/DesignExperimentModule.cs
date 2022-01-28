using E3.ReactorManager.DesignExperiment.Model;
using E3.ReactorManager.DesignExperiment.ViewModels;
using E3.ReactorManager.DesignExperiment.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace E3.ReactorManager.DesignExperiment
{
    public class DesignExperimentModule : IModule
    {
        private readonly IRegionManager regionManager;

        public DesignExperimentModule(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.Resolve<IDesignExperiment>();
            containerProvider.Resolve<RunningExperimentTabViewModel>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IDesignExperiment, Model.DesignExperiment>();
            containerRegistry.Register<IExperimentInfoProvider, ExperimentInfoProvider>();

            containerRegistry.RegisterSingleton<RunningExperimentTabViewModel>();

            regionManager.RegisterViewWithRegion("ExperimentInfo", typeof(ExperimentInfoView));
            regionManager.RegisterViewWithRegion("RunningExperimentTab", typeof(RunningExperimentTabView));
            regionManager.RegisterViewWithRegion("DesignExperiment", typeof(DesignExperimentView));
            regionManager.RegisterViewWithRegion("RunningExperimentsList", typeof(RunningExperimentsListView));

            containerRegistry.RegisterForNavigation(typeof(DesignExperimentView), "DesignExperiment");
        }
    }
}
