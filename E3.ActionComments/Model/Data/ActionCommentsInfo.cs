using Prism.Mvvm;
using System;

namespace E3.ActionComments.Model.Data
{
    public class ActionCommentsInfo : BindableBase
    {
        private string _fieldDeviceLabel;
        public string FieldDeviceLabel
        {
            get { return _fieldDeviceLabel; }
            set
            {
                _fieldDeviceLabel = value;
                RaisePropertyChanged();
            }
        }

        private string _comments;
        public string Comments
        {
            get { return _comments; }
            set
            {
                _comments = value;
                RaisePropertyChanged();
            }
        }

        private string _nameOfUser;
        public string NameOfUser
        {
            get { return _nameOfUser; }
            set
            {
                _nameOfUser = value;
                RaisePropertyChanged();
            }
        }

        private DateTime _timeStamp;
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set
            {
                _timeStamp = value;
                RaisePropertyChanged();
            }
        }
    }
}
