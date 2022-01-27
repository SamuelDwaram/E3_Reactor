using System;
using System.Windows.Input;
using E3.DialogServices.DialogDataContexts;

namespace E3.DialogServices.Model
{
    public class DialogServiceProvider : IDialogServiceProvider
    {
        public void ShowAlert(string title, object content, ICommand acknowledgeCommand = null, Action callBack = null)
        {
            Alert showAlertEventArgs = new Alert
            {
                Title = title,
                Content = content,
                AcknowledgeCommand = acknowledgeCommand,
                CallBack = callBack
            };
            ShowAlertDialog?.BeginInvoke(this, showAlertEventArgs, null, null);
        }

        public void ShowConfirmation(string title, object content, ICommand confirmCommand = null, ICommand cancelCommand = null)
        {
            Confirmation showConfirmationEventArgs = new Confirmation
            {
                Title = title,
                Content = content,
                ConfirmCommand = confirmCommand,
                CancelCommand = cancelCommand
            };
            ShowConfirmationDialog?.BeginInvoke(this, showConfirmationEventArgs, null, null);
        }

        public void ShowDynamicDialogWindow(string title, object content, object dialogContentUI, Action callBack = null)
        {
            DynamicDialogDataContext showDynamicDialogEventArgs = new DynamicDialogDataContext
            {
                Title = title,
                Content = content,
                DialogContentUI = dialogContentUI,
                CallBack = callBack
            };
            ShowDynamicDialog?.BeginInvoke(this, showDynamicDialogEventArgs, null, null);
        }

        public event ShowDialogEventHandler<Alert> ShowAlertDialog;
        public event ShowDialogEventHandler<Confirmation> ShowConfirmationDialog;
        public event ShowDialogEventHandler<DynamicDialogDataContext> ShowDynamicDialog;
    }
}
