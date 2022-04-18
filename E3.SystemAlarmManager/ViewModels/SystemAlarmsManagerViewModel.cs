using Prism.Commands;
using Prism.Regions;
using System.Diagnostics;
using System.Windows.Input;

namespace E3.SystemAlarmManager.ViewModels
{
    public class SystemAlarmsManagerViewModel : IRegionMemberLifetime, INavigationAware
    {
        private readonly IRegionManager regionManager;

        public SystemAlarmsManagerViewModel(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
        }

        public ICommand NavigateCommand
        {
            get => new DelegateCommand<string>(page => regionManager.RequestNavigate("SelectedViewPane", page));
        }
        public bool KeepAlive => false;

        #region Navigation Aware
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            //Removing the sub region "ConfigureFailures" to avoid Argument Exception in PRISM framework
            //when navigating to same page some time later
            navigationContext.NavigationService.Region.RegionManager.Regions.Remove("ConfigureFailures");
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            
        }
        #endregion
    }
}
