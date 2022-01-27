using E3.Mediator.Services;
using Prism.Ioc;
using Prism.Modularity;

namespace E3.Mediator
{
    public class MediatorModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
 
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<MediatorService>();
        }
    }
}