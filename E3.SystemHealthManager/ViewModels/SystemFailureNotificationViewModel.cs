using E3.SystemHealthManager.Models;
using E3.SystemHealthManager.Services;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace E3.SystemHealthManager.ViewModels
{
    public class SystemFailureNotificationViewModel : BindableBase
    {
        private readonly ISystemFailuresManager systemFailuresManager;

        public SystemFailureNotificationViewModel(ISystemFailuresManager systemFailuresManager)
        {
            this.systemFailuresManager = systemFailuresManager;
            this.systemFailuresManager.RefreshSystemFailures += SystemFailuresManager_RefreshSystemFailures;
        }

        private void SystemFailuresManager_RefreshSystemFailures(IEnumerable<SystemFailure> systemFailures)
        {
            if (systemFailures.Any(f => f.Id == SystemFailure.Id))
            {
                //Update System Failure object here
                SystemFailure = systemFailures.First(f => f.Id == SystemFailure.Id);
                RaisePropertyChanged(nameof(SystemFailure));
            }
        }

        public void AcknowledgeSystemFailure(object id)
        {
            systemFailuresManager.Acknowledge((int)id);
            if (CurrentWindow == null)
            {
                throw new NullReferenceException("Current Window is null");
            }
            else
            {
                CurrentWindow.Close();
            }
        }

        public void SetSystemFailureObject(SystemFailure systemFailure)
        {
            SystemFailure = systemFailure;
        }

        public void Loaded(UserControl userControl)
        {
            CurrentWindow = Window.GetWindow(userControl);
        }

        #region Commands
        private ICommand _acknowledgeSystemFailureCommand;
        public ICommand AcknowledgeSystemFailureCommand
        {
            get => _acknowledgeSystemFailureCommand ?? (_acknowledgeSystemFailureCommand = new DelegateCommand<object>(AcknowledgeSystemFailure));
            set => SetProperty(ref _acknowledgeSystemFailureCommand, value);
        }

        private ICommand _loadedCommand;
        public ICommand LoadedCommand
        {
            get => _loadedCommand ?? (_loadedCommand = new DelegateCommand<UserControl>(Loaded));
            set => SetProperty(ref _loadedCommand, value);
        }
        #endregion

        #region Properties
        private SystemFailure _systemFailure;
        public SystemFailure SystemFailure
        {
            get => _systemFailure ?? (_systemFailure = new SystemFailure());
            set => SetProperty(ref _systemFailure, value);
        }

        private Window _currentWindow;
        public Window CurrentWindow
        {
            get => _currentWindow;
            set => SetProperty(ref _currentWindow, value);
        }
        #endregion
    }
}
