using E3.AuditTrailManager.Model.Enums;
using Prism.Mvvm;
using System;

namespace E3.AuditTrailManager.Model.Data
{
    public class AuditEvent : BindableBase
    {
        private string _nameOfUser;
        public string NameOfUser
        {
            get => _nameOfUser;
            set
            {
                _nameOfUser = value;
                RaisePropertyChanged();
            }
        }

        private DateTime _timeStamp;
        public DateTime TimeStamp
        {
            get => _timeStamp;
            set
            {
                _timeStamp = value;
                RaisePropertyChanged();
            }
        }

        private EventTypeEnum _eventCategory;
        public EventTypeEnum EventCategory
        {
            get => _eventCategory;
            set
            {
                _eventCategory = value;
                RaisePropertyChanged();
            }
        }

        private string _message;
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                RaisePropertyChanged();
            }
        }
    }
}
