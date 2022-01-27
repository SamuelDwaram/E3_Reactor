using E3.ActivityMonitor.Services;
using E3.AuditTrailManager.Model;
using E3.AuditTrailManager.Model.Enums;
using E3.ReactorManager.Interfaces.Framework.Logging;
using E3.UserManager.Model.Data;
using E3.UserManager.Model.Interfaces;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Unity;

namespace Anathem.Shell.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IUnityContainer unityContainer;
        private readonly TaskScheduler taskScheduler;
        private readonly IRegionManager regionManager;
        private ILogger logger;
        private IAuditTrailManager auditTrailManager;

        public MainWindowViewModel(IUnityContainer unityContainer, IRegionManager regionManager)
        {
            this.unityContainer = unityContainer;
            taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            this.regionManager = regionManager;
        }

        private void Loaded()
        {
            Task.Factory.StartNew(new Action(UpdateActiveView));
            auditTrailManager = unityContainer.Resolve<IAuditTrailManager>();
            logger = unityContainer.Resolve<ILogger>();
            unityContainer.Resolve<IActivityMonitor>().ApplicationIsIdle += MainWindowViewModel_ApplicationIsIdle;
            NavigateCommand.Execute("Initialize");
        }

        private void MainWindowViewModel_ApplicationIsIdle(object sender, EventArgs e)
        {
            Task.Factory.StartNew(new Action(() => NavigateToOtherScreen("Login")), CancellationToken.None, TaskCreationOptions.None, taskScheduler);
        }

        private void UpdateActiveView()
        {
            IRegionNavigationJournalEntry currentEntry = regionManager.Regions["SelectedViewPane"].NavigationService.Journal.CurrentEntry;
            if (currentEntry == null)
            {
                ActiveView = "";
            }
            else
            {
                ActiveView = currentEntry.Uri.ToString();
            }

            Thread.Sleep(1000);
            Task.Factory.StartNew(UpdateActiveView);
        }

        public async void CloseApplication()
        {
            if (logger != null)
            {
                logger.Log(LogType.Information, "Shutting down the application");
            }
            if (auditTrailManager != null)
            {
                await auditTrailManager.RecordEventSync("SHUT DOWN", GetLoggedInUserName(), EventTypeEnum.UserManagement);
            }
            /*
             * Kill the application after closing all the connections and logging audit
             */
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        public string GetLoggedInUserName()
        {
            if (Application.Current.Resources.Contains("LoggedInUser"))
            {
                return Application.Current.Resources["LoggedInUser"].GetType().GetProperty("Name").GetValue(Application.Current.Resources["LoggedInUser"]).ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        public void NavigateToOtherScreen(string screenIdentifier)
        {
            regionManager.RequestNavigate("SelectedViewPane", screenIdentifier);
        }

        public async void LogOut()
        {
            await auditTrailManager.RecordEventSync("User Logged Out", GetLoggedInUserName(), EventTypeEnum.UserManagement);
            NavigateToOtherScreen("Login");
        }

        #region Properties
        private string _activeView;
        public string ActiveView
        {
            get { return _activeView; }
            set { SetProperty(ref _activeView, value); }
        }
        #endregion

        #region Commands
        public ICommand LoadedCommand => new DelegateCommand(Loaded);
       // public ICommand NavigateCommand => new DelegateCommand<string>(page => regionManager.RequestNavigate("SelectedViewPane", page));
        public ICommand NavigateCommand => new DelegateCommand(() => regionManager.RequestNavigate("SelectedViewPane", "Initialize"));

        public ICommand LogOutCommand => new DelegateCommand(LogOut);
        public ICommand ClosedCommand
        {
            get => new DelegateCommand(() => Task.Factory.StartNew(() => {
                User loggedInUser = (User)Application.Current.Resources["LoggedInUser"];
                if (loggedInUser != null)
                {
                    unityContainer.Resolve<IUserManager>().UpdateUserLoginStatus(loggedInUser.UserID, false);
                }
            }).ContinueWith(t => CloseApplication()));
        }
        #endregion
    }
}
