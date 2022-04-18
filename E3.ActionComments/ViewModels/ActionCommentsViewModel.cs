using E3.ActionComments.Model;
using E3.ActionComments.Model.Data;
using E3.Mediator.Models;
using E3.Mediator.Services;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace E3.ActionComments.ViewModels
{
    public class ActionCommentsViewModel : BindableBase
    {
        private readonly IActionCommentsHandler actionCommentsHandler;
        private readonly MediatorService mediatorService;
        string deviceId = string.Empty;
        string username = string.Empty;

        public ActionCommentsViewModel(MediatorService mediatorService, IActionCommentsHandler actionCommentsHandler)
        {
            this.mediatorService = mediatorService;
            this.actionCommentsHandler = actionCommentsHandler;
            RegisterMediatorServices();
        }

        private void RegisterMediatorServices()
        {
            mediatorService.Register(InMemoryMediatorMessageContainer.UpdateSelectedDeviceId, (obj) => {
                Device device = obj as Device;
                deviceId = device.Id;

                //Update ActionComments in UI
                UpdateActionCommentsForGivenDeviceId(deviceId);
            });
        }

        private void UpdateUsername(string username)
        {
            this.username = username;   
        }

        public void SaveComments()
        {
            Task.Factory.StartNew(() => {
                actionCommentsHandler.LogActionComments(deviceId, NewActionComments, username);
                NewActionComments = string.Empty;
                UpdateActionCommentsForGivenDeviceId(deviceId);
            });
        }

        public void InitializeParameters()
        {
            UpdateUsername(Application.Current.Resources["LoggedInUser"].GetType().GetProperty("Name").GetValue(Application.Current.Resources["LoggedInUser"]).ToString());
        }

        #region ActionComments updater in UI
        public void UpdateActionCommentsForGivenDeviceId(string deviceId)
        {
            this.deviceId = deviceId;
            Task.Factory.StartNew(new Func<object, IList<ActionCommentsInfo>>(GetActionCommentsFromHandler), deviceId)
                .ContinueWith(new Action<Task<IList<ActionCommentsInfo>>>(UpdateActionCommentsCollection));
        }

        private void UpdateActionCommentsCollection(Task<IList<ActionCommentsInfo>> task)
        {
            if (task.IsCompleted)
            {
                ActionCommentsCollection = task.Result;
            }
        }

        private IList<ActionCommentsInfo> GetActionCommentsFromHandler(object deviceId)
        {
            return actionCommentsHandler.GetActionComments((string)deviceId);
        }
        #endregion

        #region Commands
        private ICommand _initializeParametersCommand;
        public ICommand InitializeParametersCommand
        {
            get => _initializeParametersCommand ?? (_initializeParametersCommand = new DelegateCommand(InitializeParameters));
            set => SetProperty(ref _initializeParametersCommand, value);
        }

        private ICommand _saveCommentsCommand;
        public ICommand SaveCommentsCommand
        {
            get => _saveCommentsCommand ?? (_saveCommentsCommand = new DelegateCommand(SaveComments));
            set => SetProperty(ref _saveCommentsCommand, value);
        }
        #endregion

        #region Properties
        private IList<ActionCommentsInfo> _actionCommentsCollection;
        public IList<ActionCommentsInfo> ActionCommentsCollection
        {
            get => _actionCommentsCollection ?? (_actionCommentsCollection = new List<ActionCommentsInfo>());
            set
            {
                _actionCommentsCollection = value;
                RaisePropertyChanged();
            }
        }

        private string _newActionComments;
        public string NewActionComments
        {
            get => _newActionComments;
            set => SetProperty(ref _newActionComments, value);
        }
        #endregion
    }
}
