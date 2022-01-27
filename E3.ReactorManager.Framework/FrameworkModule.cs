using E3.ReactorManager.Interfaces.Framework.Logging;
using Prism.Ioc;
using Prism.Modularity;
using Unity;

namespace E3.ReactorManager.Framework
{
    public class FrameworkModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<ILogger, Logger>();
        }
    }
}
