using E3.ReactorManager.Interfaces.DataAbstractionLayer;
using Prism.Ioc;
using Prism.Modularity;
using Unity;

namespace E3.ReactorManager.DataAbstractionLayer
{
    public class DataAbstractionLayerModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.Resolve<IDatabaseReader>();
            containerProvider.Resolve<IDatabaseWriter>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IDatabaseReader, DatabaseReader>();
            containerRegistry.Register<IDatabaseWriter, DatabaseWriter>();
        }
    }
}
