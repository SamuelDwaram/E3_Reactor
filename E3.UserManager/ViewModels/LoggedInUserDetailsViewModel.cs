using E3.UserManager.Model.Data;
using Prism.Mvvm;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace E3.UserManager.ViewModels
{
    public class LoggedInUserDetailsViewModel : BindableBase
    {
        public LoggedInUserDetailsViewModel()
        {
            Task.Factory.StartNew(new Action(UpdateLoggedInUser));
        }

        private void UpdateLoggedInUser()
        {
            if (Application.Current.Resources.Contains("LoggedInUser"))
            {
                Username = ((User)Application.Current.Resources["LoggedInUser"]).Name;
            }
            else
            {
                Username = string.Empty;
            }

            Task.Factory.StartNew(new Action(() => Thread.Sleep(200)))
                .ContinueWith((t) => UpdateLoggedInUser());
        }

        #region Properties
        private string _username = string.Empty;
        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }
        #endregion
    }
}
