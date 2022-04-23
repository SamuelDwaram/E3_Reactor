using E3.UserManager.Model.Data;
using E3.UserManager.Model.Interfaces;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Windows;
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

        public ICommand LoadedCommand => new DelegateCommand(Loaded);

        private void Loaded()
        {
            UserDetails = (User)Application.Current.Resources["LoggedInUser"];
        }

        public ICommand NavigateCommand
        {
            get
            {
                return new DelegateCommand<string>(page => regionManager.RequestNavigate("SelectedViewPane", page));
            }
        }

        private User _userDetails;
        public User UserDetails
        {
            get => _userDetails ?? (_userDetails = new User());
            set
            {
                _userDetails = value;
                RaisePropertyChanged();
            }
        }

    }
}
