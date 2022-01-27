using System;
using Prism.Mvvm;

namespace E3Tech.RecipeBuilding.Model.Blocks
{
    public class AoodPumpBlockParameters : BindableBase, ICloneable
    {
        public string Name
        {
            get
            {
                return this.GetType().ToString();
            }
        }

        private string _started;
        public string Started
        {
            get => !string.IsNullOrWhiteSpace(_started) ? _started : "00:00";
            set
            {
                _started = value;
                RaisePropertyChanged();
            }
        }

        private string _ended;
        public string Ended
        {
            get => !string.IsNullOrWhiteSpace(_ended) ? _ended : "00:00";
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

        private string _modifyPump1;
        public string ModifyPump1
        {
            get { return _modifyPump1; }
            set
            {
                _modifyPump1 = value;
                RaisePropertyChanged();
            }
        }
        private string _pump1Status;
        public string Pump1Status
        {
            get { return _pump1Status; }
            set
            {
                _pump1Status = value;
                RaisePropertyChanged();
            }
        }

        private string _modifyPump2;
        public string ModifyPump2
        {
            get { return _modifyPump2; }
            set
            {
                _modifyPump2 = value;
                RaisePropertyChanged();
            }
        }
        private string _pump2Status;
        public string Pump2Status
        {
            get { return _pump2Status; }
            set
            {
                _pump2Status = value;
                RaisePropertyChanged();
            }
        }

        public object Clone()
        {
            return new AoodPumpBlockParameters()
            {
                Started = this.Started?.Clone().ToString(),
                StartedTime = this.StartedTime?.Clone().ToString(),
                Ended = this.Ended?.Clone().ToString(),
                EndedTime = this.EndedTime?.Clone().ToString(),
                Enabled = this.Enabled?.Clone().ToString(),

                ModifyPump1 = this.ModifyPump1?.Clone().ToString(),
                ModifyPump2 = this.ModifyPump2?.Clone().ToString(),
                Pump1Status = this.Pump1Status?.Clone().ToString(),
                Pump2Status = this.Pump2Status?.Clone().ToString(),
            };
        }
    }
}
