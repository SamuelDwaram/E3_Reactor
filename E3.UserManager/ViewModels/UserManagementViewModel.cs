using E3.DialogServices.Model;
using E3.ReactorManager.Interfaces.Framework.Logging;
using E3.UserManager.Model.Interfaces;
using E3.UserManager.Model.Data;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Unity;
using E3.UserManager.Views;
using System.Windows;

namespace E3.UserManager.ViewModels
{
    public class UserManagementViewModel : BindableBase
    {
        private readonly IUserManager userManager;
        private readonly IRoleManager roleManager;
        private readonly ILogger logger;
        private readonly IRegionManager regionManager;
        private readonly IDialogServiceProvider dialogServiceProvider;
        private readonly IUnityContainer unityContainer;

        public UserManagementViewModel(IRegionManager regionManager, IUnityContainer containerProvider, IUserManager userManager, IRoleManager roleManager, ILogger logger, IDialogServiceProvider dialogServiceProvider)
        {
            this.regionManager = regionManager;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.logger = logger;
            this.dialogServiceProvider = dialogServiceProvider;
            unityContainer = containerProvider;

            this.userManager.UpdateUserInfoInUIEvent += UserManager_UpdateUserInfoInUIEvent;
        }

        private void UserManager_UpdateUserInfoInUIEvent(User user, string updatedProperty)
        {
            if (updatedProperty.Contains("AccessLevel"))
            {
                int index = ExistingUsers.IndexOf(ExistingUsers.First(u => u.UserID == user.UserID));
                ExistingUsers[index].Roles.Clear();
                ExistingUsers[index].Roles = user.Roles;
            }
        }

        #region Get All Users Functions
        public void GetAllUsers()
        {
            Task.Factory.StartNew(new Func<IList<User>>(userManager.GetAllUsers))
                .ContinueWith(new Action<Task<IList<User>>>(UpdateExistingUsersCollection));
        }

        private void UpdateExistingUsersCollection(Task<IList<User>> task)
        {
            if (task.IsCompleted)
            {
                ExistingUsers = task.Result;
            }
        }
        #endregion

        #region Get All Roles
        public void GetAllRoles()
        {
            Task.Factory.StartNew(new Func<IList<Role>>(roleManager.GetAllRoles))
                .ContinueWith(new Action<Task<IList<Role>>>(UpdateAvailableRolesCollection));
        }

        private void UpdateAvailableRolesCollection(Task<IList<Role>> task)
        {
            if (task.IsCompleted)
            {
                AvailableRoles = task.Result;
            }
        }
        #endregion

        #region Add or Remove Selected Roles
        public void AddSelectedRole(string givenRoleName)
        {
            if (!NewUser.Roles.Any(role => role.Name == givenRoleName))
            {
                NewUser.Roles.Add(new Role(givenRoleName));
            }
        }

        public void RemoveSelectedRole(string givenRoleName)
        {
            if (NewUser.Roles.Any(role => role.Name == givenRoleName))
            {
                NewUser.Roles.ToList().RemoveAll(role => role.Name == givenRoleName);
            }
        }
        #endregion

        #region Create New User
        private bool CanCreateNewUser()
        {
            return !string.IsNullOrWhiteSpace(NewUser.Name)
                && !string.IsNullOrWhiteSpace(NewCredentials.Username)
                && !string.IsNullOrWhiteSpace(NewCredentials.PasswordHash)
                && NewUser.Roles.Count > 0
                && NewCredentials.PasswordHash == ConfirmPasswordText
                && userManager.IsValidPasswordFormat(NewCredentials.PasswordHash)
                && roleManager.CanAccessModule(UserDetails.Roles, "UserManagement");
        }

        public void CreateNewUser()
        {
            Task.Factory.StartNew(new Action(AddUserToTheSystem))
                .ContinueWith(new Action<Task>(RefreshUsersListInUI))
                .ContinueWith(new Action<Task>(ResetNewUserDetailsInUI));
        }

        private void ResetNewUserDetailsInUI(Task task)
        {
            if (task.Exception == null)
            {
                // Reset previously entered User details in UI
                NewUser = new User();
                NewCredentials = new Credential();
                ConfirmPasswordText = string.Empty;
            }
        }

        private void RefreshUsersListInUI(Task task)
        {
            if (task.Exception == null)
            {
                logger.Log(LogType.Information, "Addition of user : " + NewUser.Name + " successful");
                GetAllUsers();
            }
            else
            {
                logger.Log(LogType.Exception, "Error occured while adding user : " + NewUser.Name, task.Exception);
            }
        }

        private void AddUserToTheSystem()
        {
            userManager.AddUser(NewUser, NewCredentials);
        }
        #endregion

        public void Navigate(string screenName)
        {
            NavigationParameters parameters = new NavigationParameters
            {
                { "UserDetails", UserDetails }
            };
            regionManager.RequestNavigate("SelectedViewPane", screenName, parameters);
        }

        #region Modify User
        private bool CanModifyUser(User selectedUser)
        {
            //A user can modify only his details
            return selectedUser.UserID == UserDetails.UserID;
        }

        public void ModifyUser(User selectedUser)
        {
            ModifyUserViewModel modifyUserDataContext = unityContainer.Resolve<ModifyUserViewModel>();
            modifyUserDataContext.UpdateParameters(selectedUser, UserDetails);
            dialogServiceProvider.ShowDynamicDialogWindow("Modify user", default, new ModifyUserView(modifyUserDataContext));
        }
        #endregion

        #region Change User Status
        private bool CanChangeUserStatus(User selectedUser)
        {
            return selectedUser.UserID != UserDetails.UserID && roleManager.CanAccessModule(UserDetails.Roles, "UserManagement");
        }

        public void ChangeUserStatus(User selectedUser)
        {
            Task.Factory.StartNew(new Action<object>(UpdateUserStatus), selectedUser);
        }

        private void UpdateUserStatus(object userObject)
        {
            User user = (User)userObject;
            logger.Log(LogType.Information, "Updating Current Status of User : " + user.UserID);
            userManager.UpdateUser(user.UserID, user.Name, "CurrentStatus", (user.CurrentStatus == UserStatus.Active ? UserStatus.InActive : UserStatus.Active).ToString());
            GetAllUsers();
        }
        #endregion

        #region Commands
        public ICommand OpenRolesAndModulesConfigurationDialogCommand
        {
            get => new DelegateCommand(() => {
                dialogServiceProvider.ShowDynamicDialogWindow(default, default, new RolesAndModulesConfigurationView());
            });
        }
        public ICommand UpdateSelectedRoleCommand
        {
            get => new DelegateCommand<Role>(role => {
                NewUser.Roles.Clear();
                NewUser.Roles.Add(role);
                RaisePropertyChanged(nameof(RolesChanged));
            });
        }

        public ICommand LoadUserDetailsCommand
        {
            get => new DelegateCommand(() => UserDetails = (User)Application.Current.Resources["LoggedInUser"]);
        }

        private ICommand _getAllUsersCommand;
        public ICommand GetAllUsersCommand
        {
            get => _getAllUsersCommand ?? (_getAllUsersCommand = new DelegateCommand(GetAllUsers));
            set => _getAllUsersCommand = value;
        }

        private ICommand _getAllRolesCommand;
        public ICommand GetAllRolesCommand
        {
            get => _getAllRolesCommand ?? (_getAllRolesCommand = new DelegateCommand(GetAllRoles));
            set => _getAllRolesCommand = value;
        }

        private ICommand _addSelectedRoleCommand;
        public ICommand AddSelectedRoleCommand
        {
            get => _addSelectedRoleCommand ?? (_addSelectedRoleCommand = new DelegateCommand<string>(AddSelectedRole));
            set => _addSelectedRoleCommand = value;
        }

        private ICommand _removeSelectedRoleCommand;
        public ICommand RemoveSelectedRoleCommand
        {
            get => _removeSelectedRoleCommand ?? (_removeSelectedRoleCommand = new DelegateCommand<string>(RemoveSelectedRole));
            set => _removeSelectedRoleCommand = value;
        }

        private ICommand _createNewUserCommand;
        public ICommand CreateNewUserCommand
        {
            get => _createNewUserCommand 
                ?? (_createNewUserCommand = new DelegateCommand(CreateNewUser, CanCreateNewUser)
                        .ObservesProperty(() => NewUser.Name)
                        .ObservesProperty(() => NewUser.Designation)
                        .ObservesProperty(() => RolesChanged)
                        .ObservesProperty(() => NewCredentials.Username)
                        .ObservesProperty(() => NewCredentials.PasswordHash)
                        .ObservesProperty(() => ConfirmPasswordText));
            set => _createNewUserCommand = value;
        }

        private ICommand _navigateCommand;
        public ICommand NavigateCommand
        {
            get => _navigateCommand ?? (_navigateCommand = new DelegateCommand<string>(Navigate));
            set => _navigateCommand = value;
        }

        private ICommand _changeUserStatusCommand;
        public ICommand ChangeUserStatusCommand
        {
            get => _changeUserStatusCommand ?? (_changeUserStatusCommand = new DelegateCommand<User>(ChangeUserStatus, CanChangeUserStatus).ObservesProperty(() => UserDetails));
            set => _changeUserStatusCommand = value;
        }

        private ICommand _modifyUserCommand;
        public ICommand ModifyUserCommand
        {
            get => _modifyUserCommand ?? (_modifyUserCommand = new DelegateCommand<User>(ModifyUser, CanModifyUser).ObservesProperty(() => UserDetails));
            set => _modifyUserCommand = value;
        }
        #endregion

        #region Properties
        public bool RolesChanged
        {
            get;set;
        }
        private IList<User> _existingUsers;
        public IList<User> ExistingUsers
        {
            get => _existingUsers ?? (_existingUsers = new List<User>());
            set
            {
                _existingUsers = value;
                RaisePropertyChanged();
            }
        }

        private IList<Role> _availableRoles;
        public IList<Role> AvailableRoles
        {
            get => _availableRoles ?? (_availableRoles = new List<Role>());
            set
            {
                _availableRoles = value;
                RaisePropertyChanged();
            }
        }

        private User _selectedUser;
        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                RaisePropertyChanged();
            }
        }

        private string _confirmPasswordText;
        public string ConfirmPasswordText
        {
            get => _confirmPasswordText;
            set
            {
                _confirmPasswordText = value;
                RaisePropertyChanged();
            }
        }

        private Credential _newCredentials;
        public Credential NewCredentials
        {
            get => _newCredentials ?? (_newCredentials = new Credential());
            set
            {
                _newCredentials = value;
                RaisePropertyChanged();
            }
        }

        private User _newUser;
        public User NewUser
        {
            get => _newUser ?? (_newUser = new User());
            set
            {
                _newUser = value;
                RaisePropertyChanged();
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
        #endregion
    }
}
