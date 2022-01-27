using Prism.Interactivity.InteractionRequest;
using System;

namespace E3.DialogServices.DialogDataContexts
{
    public class DynamicDialogDataContext : INotification
    {
        public string Title { get; set; }

        public object Content { get; set; }

        public object DialogContentUI { get; set; }

        public Action CallBack { get; set; }
    }
}
