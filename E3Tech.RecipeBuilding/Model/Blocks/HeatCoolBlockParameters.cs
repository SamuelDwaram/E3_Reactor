using Prism.Mvvm;
using System;

namespace E3Tech.RecipeBuilding.Model.Blocks
{
    public class HeatCoolBlockParameters : BindableBase, ICloneable
    {
        public string Name
        {
            get => "HeatCool";
        }

        private string _uiLabel;
        public string UiLabel
        {
            get => _uiLabel ?? "HeatCool";
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

        private string _setPoint;
        public string SetPoint
        {
            get { return _setPoint; }
            set
            {
                _setPoint = value;
                RaisePropertyChanged();
            }
        }

        private string _duration;
        public string Duration
        {
            get { return _duration; }
            set
            {
                _duration = value;
                RaisePropertyChanged();
            }
        }

        private string _operatingMode;
        public string OperatingMode
        {
            get { return _operatingMode; }
            set
            {
                _operatingMode = value;
                RaisePropertyChanged();
            }
        }

        public object Clone()
        {
            return new HeatCoolBlockParameters()
            {
                UiLabel = this.UiLabel?.Clone().ToString(),
                Started = this.Started?.Clone().ToString(),
                StartedTime = this.StartedTime?.Clone().ToString(),
                Ended = this.Ended?.Clone().ToString(),
                EndedTime = this.EndedTime?.Clone().ToString(),
                Enabled = this.Enabled?.Clone().ToString(),

                Duration = this.Duration?.Clone().ToString(),
                SetPoint = this.SetPoint?.Clone().ToString(),
                OperatingMode = this.OperatingMode?.Clone().ToString(),
            };
        }
    }
}
