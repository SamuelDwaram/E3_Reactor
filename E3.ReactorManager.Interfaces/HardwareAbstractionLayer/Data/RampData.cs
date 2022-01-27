using BaseWpf.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace E3.ReactorManager.Interfaces.HardwareAbstractionLayer.Data
{
    public class RampData : ViewModelBase
    {
        #region Properties
        /// <summary>
        /// Describes the index of current ramp data 
        /// </summary>
        private string _rampDataIndex;
        public string RampDataIndex
        {
            get { return _rampDataIndex; }
            set
            {
                _rampDataIndex = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Describes whether process is heat/cool/hold
        /// </summary>
        private string _processType;
        public string ProcessType
        {
            get { return _processType; }
            set
            {
                _processType = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Describes at what time from the start of experiment
        /// the above mentioned process should start
        /// </summary>
        private string _timeToMaintain;
        public string TimeToMaintain
        {
            get { return _timeToMaintain; }
            set
            {
                _timeToMaintain = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Set Point to be given after the above mentioned time 
        /// from the start of the experiment
        /// </summary>
        private string _toBeMaintainedSetPoint;
        public string ToBeMaintainedSetPoint
        {
            get { return _toBeMaintainedSetPoint; }
            set
            {
                //set only integer value to the setpoint
                _toBeMaintainedSetPoint = int.Parse(value).ToString(); ;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// status of this particular rampData (Started or Ended)
        /// </summary>
        private string _rampDataStatus;
        public string RampDataStatus
        {
            get { return _rampDataStatus; }
            set
            {
                _rampDataStatus = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// time when this particular RampData (timeToMaintain, toBeMaintainedSetPoint) started
        /// </summary>
        private string _timeStarted;
        public string TimeStarted
        {
            get { return _timeStarted; }
            set
            {
                _timeStarted = value;
                OnPropertyChanged();
            }
        }
        private string _startedHoldingTime;
        public string StartedHoldingTime
        {
            get { return _startedHoldingTime; }
            set
            {
                _startedHoldingTime = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// time when this particular RampData (timeToMaintain, toBeMaintainedSetPoint) ended
        /// after time of timetoMaintain is completed
        /// </summary>
        private string _timeEnded;
        public string TimeEnded
        {
            get { return _timeEnded; }
            set
            {
                _timeEnded = value;
                OnPropertyChanged();
            }
        }
        #endregion
    }
}
