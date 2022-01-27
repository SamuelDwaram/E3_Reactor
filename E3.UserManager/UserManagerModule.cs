using E3.UserManager.Model.Interfaces;
using E3.UserManager.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace E3.UserManager
{
    public class UserManagerModule : IModule
    {
        private readonly IRegionManager regionManager;

        public UserManagerModule(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
        }
        
        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.Resolve<IUserManager>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IUserManager, Model.Implementations.UserManager>();
            containerRegistry.Register<IRoleManager, Model.Implementations.RoleManager>();

            containerRegistry.RegisterForNavigation(typeof(UserManagementView), "UserManagement");
            containerRegistry.RegisterForNavigation(typeof(LoginView), "Login");
            regionManager.RegisterViewWithRegion("RolesAndModulesConfiguration", typeof(RolesAndModulesConfigurationView));
            regionManager.RegisterViewWithRegion("LiveDateTime", typeof(LiveDateTimeView));
            regionManager.RegisterViewWithRegion("LoggedInUserDetails", typeof(LoggedInUserDetailsView));
            regionManager.RegisterViewWithRegion("UserManagement", typeof(UserManagementView));
        }
    }
}
