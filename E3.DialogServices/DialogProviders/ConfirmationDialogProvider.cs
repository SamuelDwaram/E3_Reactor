using E3.DialogServices.DialogTypes;
using Prism.Interactivity.InteractionRequest;
using System;
using System.Windows;
using System.Windows.Interactivity;
using Confirmation = E3.DialogServices.DialogDataContexts.Confirmation;

namespace E3.DialogServices.DialogProviders
{
    public class ConfirmationDialogProvider : TriggerAction<FrameworkElement>
    {
        protected override void Invoke(object parameter)
        {
            InteractionRequestedEventArgs args = parameter as InteractionRequestedEventArgs;
            if (args != null)
            {
                Confirmation confirmation = args.Context as Confirmation;
                if (confirmation != null)
                {
                    ConfirmationWindow window = new ConfirmationWindow(confirmation);
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
