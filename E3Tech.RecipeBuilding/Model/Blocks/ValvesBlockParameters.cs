using System;
using Prism.Mvvm;

namespace E3Tech.RecipeBuilding.Model.Blocks
{
    public class ValvesBlockParameters : BindableBase, ICloneable
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

        private string _modifyValveA;
        public string ModifyValveA
        {
            get { return _modifyValveA; }
            set
            {
                _modifyValveA = value;
                RaisePropertyChanged();
            }
        }
        private string _valveAStatus;
        public string ValveAStatus
        {
            get { return _valveAStatus; }
            set
            {
                _valveAStatus = value;
                RaisePropertyChanged();
            }
        }

        private string _modifyValveB;
        public string ModifyValveB
        {
            get { return _modifyValveB; }
            set
            {
                _modifyValveB = value;
                RaisePropertyChanged();
            }
        }
        private string _valveBStatus;
        public string ValveBStatus
        {
            get { return _valveBStatus; }
            set
            {
                _valveBStatus = value;
                RaisePropertyChanged();
            }
        }

        private string _modifyValveC;
        public string ModifyValveC
        {
            get { return _modifyValveC; }
            set
            {
                _modifyValveC = value;
                RaisePropertyChanged();
            }
        }
        private string _valveCStatus;
        public string ValveCStatus
        {
            get { return _valveCStatus; }
            set
            {
                _valveCStatus = value;
                RaisePropertyChanged();
            }
        }
        public object Clone()
        {
            return new ValvesBlockParameters()
            {
                Started = this.Started?.Clone().ToString(),
                StartedTime = this.StartedTime?.Clone().ToString(),
                Ended = this.Ended?.Clone().ToString(),
                EndedTime = this.EndedTime?.Clone().ToString(),
                Enabled = this.Enabled?.Clone().ToString(),

                ModifyValveA = this.ModifyValveA?.Clone().ToString(),
                ModifyValveB = this.ModifyValveB?.Clone().ToString(),
                ModifyValveC = this.ModifyValveC?.Clone().ToString(),
                ValveAStatus = this.ValveAStatus?.Clone().ToString(),
                ValveBStatus = this.ValveBStatus?.Clone().ToString(),
                ValveCStatus = this.ValveCStatus?.Clone().ToString(),
            };
        }
    }
}
