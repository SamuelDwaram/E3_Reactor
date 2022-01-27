using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Windows.Input;

namespace Anathem.Ui.ViewModels
{
    public class DashboardViewModel : BindableBase
    {
        private readonly IRegionManager regionManager;

        public DashboardViewModel(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
        }

        public ICommand navigateCommand;
        public ICommand NavigateCommand
        {
            get
            {
                return new DelegateCommand<string>(page => Assign(page));
            }
        }

        private void Assign(string page)
        {
            regionManager.RequestNavigate("SelectedViewPane", page);
            
        }
    }
}
