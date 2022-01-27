using Prism.Mvvm;

namespace E3Tech.RecipeBuilding.Model.Blocks
{
    public class BaseDrainBlockParameters : BindableBase
    {
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
            get => !string.IsNullOrWhiteSpace(_startedTime) ? _startedTime : "00:00";
            set
            {
                _startedTime = value;
                RaisePropertyChanged();
            }
        }

        private string _endedTime;
        public string EndedTime
        {
            get => !string.IsNullOrWhiteSpace(_endedTime) ? _endedTime : "00:00";
            set
            {
                _endedTime = value;
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
