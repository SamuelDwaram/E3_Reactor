using Prism.Interactivity.InteractionRequest;
using System;
using System.Windows.Input;

namespace E3.DialogServices.DialogDataContexts
{
    public class Alert : INotification
    {
        public Action CallBack { get; set; }

        public ICommand AcknowledgeCommand { get; set; }

        public string Title { get; set; }

        public object Content { get; set; }
    }
}
