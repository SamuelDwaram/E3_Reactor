using E3.ReactorManager.Interfaces.Framework.Logging;
using Unity;

namespace E3.ReactorManager.Framework
{
    public class FrameworkModuleInstanceProvider
    {
        static IUnityContainer unityContainer;

        public FrameworkModuleInstanceProvider(IUnityContainer containerProvider)
        {
            unityContainer = containerProvider;
        }

        public static ILogger GetInstance()
        {
            return unityContainer.Resolve<ILogger>();
        }
    }
}
