using E3.DialogServices.DialogDataContexts;
using E3.DialogServices.DialogTypes;
using Prism.Interactivity.InteractionRequest;
using System;
using System.Windows;
using System.Windows.Interactivity;

namespace E3.DialogServices.DialogProviders
{
    public class AlertDialogProvider : TriggerAction<FrameworkElement>
    {
        protected override void Invoke(object parameter)
        {
            InteractionRequestedEventArgs args = parameter as InteractionRequestedEventArgs;
            if (args != null)
            {
                Alert alert = args.Context as Alert;
                if (alert != null)
                {
                    AlertWindow window = new AlertWindow(alert);
                    EventHandler closeHandler = null;
                    closeHandler = (sender, e) =>
                    {
                        window.Closed -= closeHandler;
                        args.Callback();
                    };
                    window.Closed += closeHandler;
                    window.ShowDialog();
                }
            }

        }
    }
}
