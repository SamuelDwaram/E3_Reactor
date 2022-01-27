using Unity;

namespace E3.ReactorManager.HardwareAbstractionLayer
{
    public class FieldDevicesCommunicatorInstanceProvider
    {
        static IUnityContainer unityContainer;

        public FieldDevicesCommunicatorInstanceProvider(IUnityContainer containerProvider)
        {
            unityContainer = containerProvider;
        }

        public static T GetInstance<T>()
        {
            return unityContainer.Resolve<T>();
        }
    }
}
