using E3Tech.RecipeBuilding.Helpers;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;

namespace E3Tech.RecipeBuilding.Model.Blocks
{
    public class BaseBlockParameters : BindableBase
    {
        public event EventHandler<SourceChangesEventErgs> OnSourceChanged;
        private string _started;
        public string Started
        {
            get => _started ?? bool.FalseString;
            set
            {
                _started = value;
                RaisePropertyChanged();
            }
        }

        private string _ended;
        public string Ended
        {
            get => _ended ?? bool.FalseString;
            set
            {
                _ended = value;
                RaisePropertyChanged();
            }
        }

        private string _startedTime;
        public string StartedTime
        {
            get => !string.IsNullOrWhiteSpace(_startedTime) ? _startedTime : "00:00:00";
            set
            {
                _startedTime = value;
                RaisePropertyChanged();
            }
        }

        private string _endedTime;
        public string EndedTime
        {
            get => !string.IsNullOrWhiteSpace(_endedTime) ? _endedTime : "00:00:00";
            set
            {
                _endedTime = value;
                RaisePropertyChanged();
            }
        }
        private string _remainingTime;
        public string RemainingTime
        {
            get => !string.IsNullOrWhiteSpace(_remainingTime) ? _remainingTime : "00:00:00";
            set
            {
                _remainingTime = value;
                RaisePropertyChanged();
            }
        }

        private string _enabled;
        public string Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                RaisePropertyChanged();
            }
        }

        private string source;
        public string Source
        {
            get => source;
            set
            {
                source = value;
                RaisePropertyChanged();
                if (source != null)
                {
                    OnSourceChanged?.Invoke(this, new SourceChangesEventErgs() { Source = source });
                }
            }
        }

        private string destination;
        public virtual string Destination
        {
            get => destination;
            set
            {
                destination = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> filterDestination;
        public ObservableCollection<string> FilterDestination
        {
            get
            {
                return filterDestination;
            }
            set
            {
                filterDestination = value;
                OnPropertyChanged();
            }
        }

        private string timeInterval;
        public string TimeInterval
        {
            get { return timeInterval; }
            set { SetProperty(ref timeInterval, value); }
        }

        private string intervalType;
        public string IntervalType
        {
            get { return intervalType; }
            set { SetProperty(ref intervalType, value); }
        }
    }
}
