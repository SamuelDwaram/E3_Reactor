using E3.AuditTrailManager.Model;
using E3.AuditTrailManager.Model.Enums;
using E3.DialogServices.Model;
using E3.UserManager.Model.Data;
using E3.UserManager.Model.Interfaces;
using E3.UserManager.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Unity;

namespace E3.UserManager.ViewModels
{
    public class LoginViewModel : BindableBase, IRegionMemberLifetime, INavigationAware
    {
        private readonly IRegionManager regionManager;
        private readonly TaskScheduler taskScheduler;
        private readonly IAuditTrailManager auditTrailManager;
        private readonly IDialogServiceProvider dialogServiceProvider;
        private readonly IUserManager userManager;

        public LoginViewModel(IRegionManager regionManager, IUserManager userManager, IAuditTrailManager auditTrailManager, IDialogServiceProvider dialogServiceProvider)
        {
            this.regionManager = regionManager;
            this.userManager = userManager;
            this.auditTrailManager = auditTrailManager;
            this.dialogServiceProvider = dialogServiceProvider;
            taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            UserNameError = false;
            PasswordError = false;
        }

        #region Login
        public void Login(PasswordBox passwordBox)
        {
            /* Update password */
            Password = passwordBox.Password;

            if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
            {
                AuthenticationInProgress = true;

                Task.Factory.StartNew(new Func<User>(ValidateUserCredentials))
                    .ContinueWith(new Action<Task<User>>(UpdateUserDetailsAndValidationStatus), taskScheduler);
            }
            else
            {
                UsernameErrorMessage = string.IsNullOrWhiteSpace(Username) ? "Please Enter Valid Username" : string.Empty;
                PasswordErrorMessage = string.IsNullOrWhiteSpace(Password) ? "Please Enter Valid Password" : string.Empty;
                UserNameError = string.IsNullOrWhiteSpace(Username);
                PasswordError = string.IsNullOrWhiteSpace(Password);
            }
        }

        private void UpdateUserDetailsAndValidationStatus(Task<User> task)
        {
            AuthenticationInProgress = false;

            if (task.IsCompleted)
            {
                User user = task.Result;

                if (user == null)
                {
                    auditTrailManager.RecordEventAsync($"Unauthorized login with {Username}", string.Empty, EventTypeEnum.UserManagement);
                    UsernameErrorMessage = "Please Enter Valid Username";
                    PasswordErrorMessage = "Please Enter Valid Password";
                    UserNameError = true;
                    PasswordError = true;
                }
                else
                {
                    if (userManager.GetUserLoginStatus(user.UserID))
                    {
                        //Skip. don't allow user to login since he is already logged in
                        UsernameErrorMessage = "User already logged in from another system";
                        PasswordErrorMessage = string.Empty;
                        UserNameError = true;
                        PasswordError = false;
                    }
                    else
                    {
                        //check if user is Admin. If yes login immediately
                        if (user.Roles.Any(r => r.Name == "Admin"))
                        {
                            //userManager.UpdateUserLoginStatus(user.UserID, true);
                            CheckForPasswordExpiryAndLogin(user);
                        }
                        //check for password expiry
                        else if (user.PasswordExpired)
                        {
                            PasswordErrorMessage = "Password Expired. Please contact Admin";
                            PasswordError = true;
                        }
                        //check for User Active status
                        else if (CheckIfUserIsActive(user.CurrentStatus))
                        {
                            //userManager.UpdateUserLoginStatus(user.UserID, true);
                            CheckForPasswordExpiryAndLogin(user);
                        }
                        else
                        {
                            // User not active
                            UsernameErrorMessage = "User not active. Please contact Admin";
                            UserNameError = true;
                            return;
                        }
                    }
                }
            }
        }

        public void CheckForPasswordExpiryAndLogin(User user)
        {
            if (user.DaysRemainingInPasswordExpiry <= 7)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show($"Your password is expiring in {user.DaysRemainingInPasswordExpiry} days. Would you like to change?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    ModifyPasswordViewModel modifyPasswordViewModel = new ModifyPasswordViewModel(user, userManager);
                    dialogServiceProvider.ShowDynamicDialogWindow(string.Empty, default, new ModifyPasswordView(modifyPasswordViewModel), new Action(()=> {
                        Task.Factory.StartNew(() => LoginToSystem(userManager.GetAllUsers().First(u => u.UserID == user.UserID)), CancellationToken.None, TaskCreationOptions.None, taskScheduler);
                    }));
                    return;
                }
            }

            LoginToSystem(user);
        }

        public void LoginToSystem(User user)
        {
            //Add User details to the application resources
            Application.Current.Resources["LoggedInUser"] = user;

            //Log Audit Trail as User Logged in
            auditTrailManager.RecordEventAsync(user.Name + " logged in", user.Name, EventTypeEnum.UserManagement);

            //Navigate to Dashboard
            regionManager.RequestNavigate("SelectedViewPane", "Dashboard");
        }

        private User ValidateUserCredentials()
        {
            return userManager.AuthenticateCredential(new Credential { Username = this.Username, PasswordHash = this.Password });
        }

        /// <summary>
        /// Checks whether the user's login status in Active or Inactive
        /// </summary>
        /// <param name="userCurrentStatus"></param>
        /// <returns></returns>
        public bool CheckIfUserIsActive(UserStatus userCurrentStatus)
        {
            switch (userCurrentStatus)
            {
                case UserStatus.Active:
                    return true;
            }
            return false;
        }
        #endregion

        #region Navigation Aware Handlers
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (Application.Current.Resources.Contains("LoggedInUser"))
            {
                User loggedInUser = (User)Application.Current.Resources["LoggedInUser"];
                userManager.UpdateUserLoginStatus(loggedInUser.UserID, false);
                Application.Current.Resources.Remove("LoggedInUser");
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            
        }
        #endregion

        public ICommand LoginCommand
        {
            get => new DelegateCommand<PasswordBox>(Login);
        }

        #region Properties
        public bool KeepAlive
        {
            get => false;
        }

        private bool _authenticationInProgress;
        public bool AuthenticationInProgress
        {
            get { return _authenticationInProgress; }
            set { SetProperty(ref _authenticationInProgress, value); }
        }

        /// <summary>
        /// Describes if there is Username Error  
        /// </summary>
        private bool _userNameError;
        public bool UserNameError
        {
            get { return _userNameError; }
            set
            {
                _userNameError = value;
                RaisePropertyChanged();
            }
        }

        private string _usernameErrorMessage;
        public string UsernameErrorMessage
        {
            get { return _usernameErrorMessage; }
            set { SetProperty(ref _usernameErrorMessage, value); }
        }

        /// <summary>
        /// Describes if there is Password Error
        /// </summary>
        private bool _passwordError;
        public bool PasswordError
        {
            get { return _passwordError; }
            set
            {
                _passwordError = value;
                RaisePropertyChanged();
            }
        }

        private string _passwordErrorMessage;
        public string PasswordErrorMessage
        {
            get { return _passwordErrorMessage; }
            set { SetProperty(ref _passwordErrorMessage, value); }
        }

        /// <summary>
        /// Contains the username entered in GUI
        /// </summary>
        private string _username;
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                RaisePropertyChanged();
                ResetErrors();
            }
        }

        /// <summary>
        /// Contains the password entered in GUI
        /// </summary>
        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                RaisePropertyChanged();
                ResetErrors();
            }
        }

        private void ResetErrors()
        {
            UserNameError = false;
            PasswordError = false;
        }
        #endregion
    }
}
