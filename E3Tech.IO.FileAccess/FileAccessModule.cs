using Prism.Ioc;
using Prism.Modularity;

namespace E3Tech.IO.FileAccess
{
    public class FileAccessModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IFileBrowser, DefaultFileBrowser>();
        }
    }
}
