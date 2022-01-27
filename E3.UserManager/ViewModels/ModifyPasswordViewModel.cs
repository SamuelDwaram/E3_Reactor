using E3.UserManager.Model.Data;
using E3.UserManager.Model.Interfaces;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace E3.UserManager.ViewModels
{
    public class ModifyPasswordViewModel : BindableBase
    {
        private readonly IUserManager userManager;

        public ModifyPasswordViewModel(User tobeModifiedUser, IUserManager userManager)
        {
            TobeModifiedUser = tobeModifiedUser;
            this.userManager = userManager;
        }

        #region Commands
        public ICommand UpdateUserCommand
        {
            get => new DelegateCommand<string>(updatedPassword => userManager.UpdateUser(TobeModifiedUser.UserID, TobeModifiedUser.Name, "Password", updatedPassword));
        }

        public ICommand LoadWindowCommand
        {
            get => new DelegateCommand<UserControl>(uc => CurrentWindow = Window.GetWindow(uc));
        }

        public ICommand CloseWindowCommand
        {
            get => new DelegateCommand(() => {
                if (CurrentWindow != null)
                {
                    CurrentWindow.Close();
                }
            });
        }
        #endregion

        #region Properties
        public Window CurrentWindow { get; set; } = new Window();

        private User _tobeModifiedUser;
        public User TobeModifiedUser
        {
            get { return _tobeModifiedUser; }
            set { SetProperty(ref _tobeModifiedUser, value); }
        }
        #endregion
    }
}
