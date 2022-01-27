using Prism.Mvvm;
using System;

namespace E3Tech.RecipeBuilding.Model.Blocks
{
    public class StartBlockParameters : BindableBase, ICloneable
    {
        public string Name
        {
            get => "Start";
        }

        private string _uiLabel;
        public string UiLabel
        {
            get => _uiLabel ?? "Start";
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

        private string _heatCoolDeltaTemp;
        public string HeatCoolDeltaTemp
        {
            get { return _heatCoolDeltaTemp; }
            set
            {
                _heatCoolDeltaTemp = value;
                RaisePropertyChanged();
            }
        }

        private string _heatCoolModeSelection;
        public string HeatCoolModeSelection
        {
            get => _heatCoolModeSelection ?? bool.FalseString;
            set
            {
                _heatCoolModeSelection = value;
                RaisePropertyChanged();
            }
        }

        public object Clone()
        {
            return new StartBlockParameters()
            {
                UiLabel = this.UiLabel?.Clone().ToString(),
                Started = this.Started?.Clone().ToString(),
                StartedTime = this.StartedTime?.Clone().ToString(),
                Ended = this.Ended?.Clone().ToString(),
                EndedTime = this.EndedTime?.Clone().ToString(),
                HeatCoolDeltaTemp = this.HeatCoolDeltaTemp?.Clone().ToString(),
                HeatCoolModeSelection = this.HeatCoolModeSelection?.Clone().ToString(),
            };
        }
    }
}
