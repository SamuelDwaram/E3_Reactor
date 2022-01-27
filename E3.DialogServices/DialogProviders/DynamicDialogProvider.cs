using E3.DialogServices.DialogDataContexts;
using E3.DialogServices.DialogTypes;
using Prism.Interactivity.InteractionRequest;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace E3.DialogServices.DialogProviders
{
    public class DynamicDialogProvider : TriggerAction<FrameworkElement>
    {
        protected override void Invoke(object parameter)
        {
            InteractionRequestedEventArgs args = parameter as InteractionRequestedEventArgs;
            if (args != null)
            {
                DynamicDialogDataContext dynamicDialogDataContext = args.Context as DynamicDialogDataContext;
                if (dynamicDialogDataContext != null)
                {
                    DynamicDialogWindow window = new DynamicDialogWindow(dynamicDialogDataContext);
                    ContentControl dialogContent = dynamicDialogDataContext.DialogContentUI as ContentControl;
                    window.Height = dialogContent.Height;
                    window.Width = dialogContent.Width;
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
