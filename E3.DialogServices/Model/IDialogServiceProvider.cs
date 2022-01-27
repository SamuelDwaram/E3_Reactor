using E3.DialogServices.DialogDataContexts;
using System;
using System.Windows.Input;

namespace E3.DialogServices.Model
{
    public interface IDialogServiceProvider
    {
        event ShowDialogEventHandler<Alert> ShowAlertDialog;
        event ShowDialogEventHandler<Confirmation> ShowConfirmationDialog;
        event ShowDialogEventHandler<DynamicDialogDataContext> ShowDynamicDialog;

        void ShowAlert(string title, object content, ICommand acknowledgeCommand = null, Action callBack = null);

        void ShowConfirmation(string title, object content, ICommand confirmCommand = null, ICommand cancelCommand = null);

        void ShowDynamicDialogWindow(string title, object content, object dialogContentUI, Action callBack = null);
    }

    public delegate void ShowDialogEventHandler<T>(object sender, T args);
}
