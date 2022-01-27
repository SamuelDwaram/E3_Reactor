using E3Tech.Navigation;
using E3Tech.RecipeBuilding.Model;
using E3Tech.RecipeBuilding.Model.Blocks;
using E3Tech.RecipeBuilding.RecipceExport;
using E3Tech.RecipeBuilding.RecipeImport;
using E3Tech.RecipeBuilding.ViewModels;
using E3Tech.RecipeBuilding.Views;
using E3Tech.RecipeBuilding.Model.RecipeExecutionInfoProvider;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Unity;

namespace E3Tech.RecipeBuilding
{
    public class RecipeBuildingModule : IModule
    {
        private readonly IUnityContainer container;
        private readonly IRegionManager regionManager;
        private readonly IViewManager viewManager;

        public RecipeBuildingModule(IUnityContainer container, IRegionManager manager)
        {
            this.container = container;
            this.regionManager = manager;
            viewManager = container.Resolve<IViewManager>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.Resolve<IRecipeExecutor>();
            container.Resolve<PLCRecipeRefresherAdapter>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<RecipeBuilderViewModel>();
            containerRegistry.Register<SequenceRecipeBuilderViewModel>();
            containerRegistry.Register<IRecipeBuilder, RecipeBuilder>();
            containerRegistry.RegisterSingleton<IMultiRecipeBuilder, MultiRecipeBuilder>();
            containerRegistry.RegisterSingleton<IRecipeRefresher, RecipeRefresher>();
            containerRegistry.Register<IRecipeRulesValidator, RecipeRulesValidator>();
            containerRegistry.Register<IRecipeExporter, RecipeFileExporter>();
            containerRegistry.Register<IRecipeImporter, RecipeFileImporter>();
            containerRegistry.Register<IRecipeExecutionInfoHandler, RecipeExecutionInfoHandler>();
            containerRegistry.Register<IRecipeExecutionInfoProvider, RecipeExecutionInfoProvider>();
            RegisterBlocks(containerRegistry);

            containerRegistry.RegisterSingleton<IRecipeExecutor, PLCRecipeExecutor>();
            containerRegistry.Register<IRecipeReloader, PLCRecipeReloader>();
            containerRegistry.RegisterInstance<IRecipesManager>(RecipesManager.Instance);

            regionManager.RegisterViewWithRegion("RecipeBuilder", typeof(MultiRecipeBuilderView));
            containerRegistry.RegisterForNavigation(typeof(RecipesView), "Recipes");
            viewManager.AddView("RecipeBuilderView");
            viewManager.AddView("SequenceReceipeBuilderView");
            containerRegistry.RegisterForNavigation(typeof(DesignExperimentView), "DesignExperiment");
            regionManager.RegisterViewWithRegion("SeqRecipes", typeof(SequenceRecipeBuilderView));
            containerRegistry.RegisterForNavigation(typeof(SequenceRecipeBuilderView), "SeqRecipes");

        }

        private static void RegisterBlocks(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IRecipeBlock, ParameterizedRecipeBlock<StartBlockParameters>>("StartBlock");
            containerRegistry.Register<IRecipeBlock, ParameterizedRecipeBlock<StirrerBlockParameters>>("StirrerBlock");
            containerRegistry.Register<IRecipeBlock, ParameterizedRecipeBlock<HeatCoolBlockParameters>>("Heat/Cool");
            containerRegistry.Register<IRecipeBlock, ParameterizedRecipeBlock<WaitBlockParameters>>("Wait");
            containerRegistry.Register<IRecipeBlock, ParameterizedRecipeBlock<TransferBlockParameters>>("Transfer");
            containerRegistry.Register<IRecipeBlock, ParameterizedRecipeBlock<DrainBlockParameters>>("Drain");
            containerRegistry.Register<IRecipeBlock, ParameterizedRecipeBlock<FlushBlockParameters>>("Flush");
            containerRegistry.Register<IRecipeBlock, ParameterizedRecipeBlock<N2PurgeBlockParameters>>("N2Purge");
            containerRegistry.Register<IRecipeBlock, ParameterizedRecipeBlock<EndBlockParameters>>("End");
            containerRegistry.Register<IRecipeBlock, RecipeBlock>();
        }
    }
}
