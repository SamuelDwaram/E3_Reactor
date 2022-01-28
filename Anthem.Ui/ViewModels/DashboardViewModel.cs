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

        public ICommand NavigateCommand
        {
            get => new DelegateCommand<string>(page => regionManager.RequestNavigate("SelectedViewPane", page));
        }

       
    }
}
