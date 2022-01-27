using E3.DialogServices.DialogDataContexts;
using E3.DialogServices.Model;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confirmation = E3.DialogServices.DialogDataContexts.Confirmation;

namespace E3.DialogServices.ViewModels
{
    public class DialogServiceViewModel : BindableBase
    {
        IDialogServiceProvider dialogServiceProvider;
        TaskScheduler taskScheduler;

        public DialogServiceViewModel(IDialogServiceProvider dialogServiceProvider)
        {
            taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            this.dialogServiceProvider = dialogServiceProvider;
            dialogServiceProvider.ShowAlertDialog += OnShowAlertDialog;
            dialogServiceProvider.ShowConfirmationDialog += OnShowConfirmationDialog;
            dialogServiceProvider.ShowDynamicDialog += OnShowDynamicDialog;
        }

        private void OnShowDynamicDialog(object sender, DynamicDialogDataContext dynamicDialogArgs)
        {
            Task.Factory.StartNew(new Action<object>(ShowDialog<DynamicDialogDataContext>), dynamicDialogArgs, new CancellationToken(), TaskCreationOptions.None, taskScheduler);
        }

        private void OnShowConfirmationDialog(object sender, Confirmation confirmationDialogArgs)
        {
            Task.Factory.StartNew(new Action<object>(ShowDialog<Confirmation>), confirmationDialogArgs, new CancellationToken(), TaskCreationOptions.None, taskScheduler);
        }

        private void OnShowAlertDialog(object sender, Alert alertDialogArgs)
        {
            Task.Factory.StartNew(new Action<object>(ShowDialog<Alert>), alertDialogArgs, new CancellationToken(), TaskCreationOptions.None , taskScheduler);
        }

        void ShowDialog<T>(object args)
        {
            switch ((T)args)
            {
                case Alert alert:
                    if (UpdateNotificationList<Alert>((Alert)args))
                    {
                        _alertInteractionRequest.Raise(alert, OnAlertDialogClosed);
                    }
                    break;
                case Confirmation confirmation:
                    _confirmationInteractionRequest.Raise(confirmation);
                    break;
                case DynamicDialogDataContext dynamicDialogDataContext:
                    if (UpdateNotificationList<DynamicDialogDataContext>((DynamicDialogDataContext)args))
                    {
                        _dynamicDialogInteractionRequest.Raise(dynamicDialogDataContext, OnDynamicDialogClosed);
                    }
                    break;
                default:
                    break;
            }
        }

        #region Callback Executers
        private void OnDynamicDialogClosed(DynamicDialogDataContext dynamicDialogDataContext)
        {
            if (dynamicDialogDataContext.CallBack != null)
            {
                dynamicDialogDataContext.CallBack.BeginInvoke(null, null);
            }
            NotificationList.Remove(dynamicDialogDataContext);
        }

        private void OnAlertDialogClosed(Alert alert)
        {
            if (alert.CallBack != null)
            {
                alert.CallBack.BeginInvoke(null, null);
            }
            NotificationList.Remove(alert);
        }
        #endregion

        #region Notification List handlers
        private bool UpdateNotificationList<T>(T data)
        {
            if (CheckIfNotificationExistsAlready<T>(data))
            {
                return false;
            }

            NotificationList.Add(data);
            return true;
        }

        private bool CheckIfNotificationExistsAlready<T>(T data)
        {
            return NotificationList.Any(item => item.GetType() == typeof(T) && Convert.ChangeType(item, typeof(T)) == Convert.ChangeType(data, typeof(T)));
        }
        #endregion

        #region Interaction Requests

        private InteractionRequest<DynamicDialogDataContext> _dynamicDialogInteractionRequest;
        public IInteractionRequest DynamicDialogInteractionRequest
        {
            get => _dynamicDialogInteractionRequest ?? (_dynamicDialogInteractionRequest = new InteractionRequest<DynamicDialogDataContext>());
        }

        private InteractionRequest<Confirmation> _confirmationInteractionRequest;
        public IInteractionRequest ConfirmationInteractionRequest
        {
            get => _confirmationInteractionRequest ?? (_confirmationInteractionRequest = new InteractionRequest<Confirmation>());
        }

        private InteractionRequest<Alert> _alertInteractionRequest;
        public IInteractionRequest AlertInteractionRequest
        {
            get => _alertInteractionRequest ?? (_alertInteractionRequest = new InteractionRequest<Alert>());
        }
        #endregion

        private IList<object> _notificationsList;
        public IList<object> NotificationList
        {
            get => _notificationsList ?? (_notificationsList = new List<object>());
            set
            {
                _notificationsList = value;
                RaisePropertyChanged();
            }
        }
    }
}
