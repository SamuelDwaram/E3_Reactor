using Prism.Mvvm;
using System;

namespace E3Tech.RecipeBuilding.Model.Blocks
{
    public class TransferBlockParameters : BindableBase, ICloneable
    {
        public string Name
        {
            get => "Transfer";
        }

        private string _uiLabel;
        public string UiLabel
        {
            get => _uiLabel ?? "Transfer";
            set
            {
                _uiLabel = value;
                RaisePropertyChanged();
            }
        }

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

        private string _volume;
        public string Volume
        {
            get => _volume;
            set
            {
                _volume = value;
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

        private string destination;
        public string Destination
        {
            get => destination;
            set
            {
                destination = value;
                RaisePropertyChanged();
            }
        }

        private string transferMode;
        public string TransferMode
        {
            get => transferMode ?? bool.FalseString;
            set { SetProperty(ref transferMode, value); }
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

        public object Clone()
        {
            return new TransferBlockParameters()
            {
                UiLabel = this.UiLabel?.Clone().ToString(),
                Started = this.Started?.Clone().ToString(),
                StartedTime = this.StartedTime?.Clone().ToString(),
                Ended = this.Ended?.Clone().ToString(),
                EndedTime = this.EndedTime?.Clone().ToString(),
                Enabled = this.Enabled?.Clone().ToString(),

                Source = this.Source?.Clone().ToString(),
                Destination = this.Destination?.Clone().ToString(),
                Volume = this.Volume?.Clone().ToString(),
                TransferMode = this.TransferMode?.Clone().ToString(),
                TimeInterval = this.TimeInterval?.Clone().ToString(),
                IntervalType = this.IntervalType?.Clone().ToString(),
            };
        }
    }
}
