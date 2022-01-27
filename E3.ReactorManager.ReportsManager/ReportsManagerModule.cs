using E3.ReactorManager.ReportsManager.Model.Implementations;
using E3.ReactorManager.ReportsManager.Model.Interfaces;
using E3.ReactorManager.ReportsManager.ViewModels;
using E3.ReactorManager.ReportsManager.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace E3.ReactorManager.ReportsManager
{
    public class ReportsManagerModule : IModule
    {
        private readonly IRegionManager regionManager;

        public ReportsManagerModule(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.Resolve<ReportPreviewViewModel>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<ReportPreviewViewModel>();
            containerRegistry.RegisterSingleton<IReportPrinter, ReportsPrinter>();
            containerRegistry.RegisterSingleton<ICsvReportPrinter, CsvReportPrinter>();
            containerRegistry.Register<IDocumentHandler, DocumentHandler>();
            regionManager.RegisterViewWithRegion("ReportPreview", typeof(ReportPreview));
        }
    }
}
