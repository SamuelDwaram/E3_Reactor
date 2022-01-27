using Prism.Commands;
using Prism.Regions;
using System.Windows.Input;

namespace E3.TrendsManager.ViewModels
{
    public class NavigableTrendsViewModel : IRegionMemberLifetime
    {
        private readonly IRegionManager regionManager;

        public NavigableTrendsViewModel(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
        }

        public ICommand NavigateCommand
        {
            get => new DelegateCommand<string>(page => regionManager.RequestNavigate("SelectedViewPane", page));
        }
        public bool KeepAlive => false;
    }
}
