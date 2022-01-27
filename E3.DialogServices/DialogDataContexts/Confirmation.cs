using Prism.Interactivity.InteractionRequest;
using System.Windows.Input;

namespace E3.DialogServices.DialogDataContexts
{
    public class Confirmation : INotification
    {
        public ICommand ConfirmCommand { get; set; }

        public ICommand CancelCommand { get; set; }

        public bool Confirmed { get; set; }

        public string Title { get; set; }

        public object Content { get; set; }
    }
}
