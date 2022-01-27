using E3.ReactorManager.Interfaces.Framework.Logging;
using E3.UserManager.Model.Data;
using E3.UserManager.Model.Interfaces;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace E3.UserManager.ViewModels
{
    public class ModifyUserViewModel : BindableBase, IRegionMemberLifetime
    {
        private readonly IUserManager userManager;
        private readonly ILogger logger;
        private readonly TaskScheduler taskScheduler;

        public ModifyUserViewModel(IUserManager userManager, ILogger logger, IRoleManager roleManager)
        {
            taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            this.logger = logger;
            this.userManager = userManager;

            Task.Factory.StartNew(new Func<IList<Role>>(roleManager.GetAllRoles))
                .ContinueWith(t => AvailableRoles = t.Result, taskScheduler);
        }

        #region Update User
        private bool CanUpdateUser()
        {
            return !string.IsNullOrWhiteSpace(SelectedUser.UserID) && !string.IsNullOrWhiteSpace(FieldToBeUpdated) && !string.IsNullOrWhiteSpace(UpdatedValue);
        }

        public void UpdateUser()
        {
            logger.Log(LogType.Information, "Updating User : " + SelectedUser.Name + " property " + FieldToBeUpdated + " to " + UpdatedValue);
            userManager.UpdateUser(SelectedUser.UserID, SelectedUser.Name, FieldToBeUpdated, UpdatedValue);
        }
        #endregion

        public void UpdateParameters(User selectedUser, User loggedInUser)
        {
            SelectedUser = selectedUser;
            LoggedInUser = loggedInUser;
        }

        public void PageLoaded(UserControl userControl)
        {
            CurrentWindow = Window.GetWindow(userControl);
        }

        public void CloseWindow(Window window)
        {
            if (window != null)
            {
                window.Close();
            }
        }

        #region Commands
        private ICommand _closeWindowCommand;
        public ICommand CloseWindowCommand
        {
            get => _closeWindowCommand ?? (_closeWindowCommand = new DelegateCommand<Window>(CloseWindow));
            set => _closeWindowCommand = value;
        }

        private ICommand _loadedCommand;
        public ICommand LoadedCommand
        {
            get => _loadedCommand ?? (_loadedCommand = new DelegateCommand<UserControl>(PageLoaded));
            set => _loadedCommand = value;
        }

        private ICommand _updateUserCommand;
        public ICommand UpdateUserCommand
        {
            get => _updateUserCommand
                ?? (_updateUserCommand = new DelegateCommand(UpdateUser, CanUpdateUser)
                .ObservesProperty(() => SelectedUser)
                .ObservesProperty(() => FieldToBeUpdated)
                .ObservesProperty(() => UpdatedValue));
            set => _updateUserCommand = value;
        }

        public ICommand SelectRoleCommand
        {
            get => new DelegateCommand<Role>(role => UpdatedValue = role.Name);
        }
        #endregion

        #region Properties
        private IList<Role> _availableRoles;
        public IList<Role> AvailableRoles
        {
            get { return _availableRoles; }
            set { SetProperty(ref _availableRoles, value); }
        }

        private Window _currentWindow;
        public Window CurrentWindow
        {
            get => _currentWindow;
            set
            {
                _currentWindow = value;
                RaisePropertyChanged();
            }
        }

        public bool KeepAlive
        {
            get => false;
        }

        private string _updatedValue;
        public string UpdatedValue
        {
            get => _updatedValue;
            set
            {
                _updatedValue = value;
                RaisePropertyChanged();
            }
        }

        private string _fieldToBeUpdated;
        public string FieldToBeUpdated
        {
            get => _fieldToBeUpdated;
            set
            {
                _fieldToBeUpdated = value;
                RaisePropertyChanged();
            }
        }

        private User _selectedUser;
        public User SelectedUser
        {
            get => _selectedUser ?? (_selectedUser = new User());
            set
            {
                _selectedUser = value;
                RaisePropertyChanged();
            }
        }

        private User _loggedInUser;
        public User LoggedInUser
        {
            get => _loggedInUser ?? (_loggedInUser = new User());
            set
            {
                _loggedInUser = value;
                RaisePropertyChanged();
            }
        }
        #endregion
    }
}
