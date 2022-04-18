using E3.SystemAlarmManager.Models;
using E3.SystemAlarmManager.Services;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace E3.SystemAlarmManager.ViewModels
{
    public class SystemAlarmsViewModel : BindableBase
    {
        private readonly ISystemAlarmsManager systemAlarmsManager;
        private readonly TaskScheduler taskScheduler;

        public SystemAlarmsViewModel(ISystemAlarmsManager systemAlarmsManager)
        {
            taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            this.systemAlarmsManager = systemAlarmsManager;
            this.systemAlarmsManager.RefreshSystemAlarms += SystemAlarmsManager_RefreshSystemAlarms;
        }

        private void SystemAlarmsManager_RefreshSystemAlarms(IEnumerable<SystemAlarm> systemAlarms)
        {
            foreach (SystemAlarm alarm in systemAlarms.OrderByDescending(al => al.TimeStamp))
            {
                //check if this alarm exists in the SystemAlarms
                // if exists update the state and other properties of alarm
                SystemAlarm existingAlarm = SystemAlarms.FirstOrDefault(al => al.Id == alarm.Id);
                if (existingAlarm == null)
                {
                    //Alarm does not exists in the list
                    //Append the new alarm to the list
                    SystemAlarms.ToList().Insert(0, alarm);
                }
                else
                {
                    //Alarm exists update all the properties of the alarm
                    existingAlarm.State = alarm.State;
                    existingAlarm.TimeStamp = alarm.TimeStamp;
                    existingAlarm.RaisedTimeStamp = alarm.RaisedTimeStamp;
                    existingAlarm.AcknowledgedTimeStamp = alarm.AcknowledgedTimeStamp;
                }
            }

            //Issue property changed notification to Ui
            RaisePropertyChanged(nameof(SystemAlarms));
        }

        public void LoadSystemAlarms()
        {
            Task.Factory.StartNew(new Func<IEnumerable<SystemAlarm>>(systemAlarmsManager.GetAll))
                .ContinueWith(new Action<Task<IEnumerable<SystemAlarm>>>((t) => SystemAlarms = t.Result.OrderByDescending(al => al.TimeStamp).ToList()));
        }

        public void FilterAlarms(object alarmsFilterType)
        {
            AlarmsFilterType selectedFilter = (AlarmsFilterType)alarmsFilterType;
            switch (selectedFilter)
            {
                case AlarmsFilterType.None:
                    // Skip. Don't filter SystemAlarms.
                    break;
                case AlarmsFilterType.Type:
                    Task.Factory.StartNew(new Func<IEnumerable<SystemAlarm>>(() => SystemAlarms.Where(al => al.Type == SelectedAlarmType)))
                        .ContinueWith(new Action<Task<IEnumerable<SystemAlarm>>>((t) => UpdateFilteredAlarms(t.Result)));
                    break;
                case AlarmsFilterType.Device:
                    if (string.IsNullOrWhiteSpace(SelectedDevice))
                    {
                        // Skip. Selected Device is empty
                        // Update Devices
                        FieldDevices = SystemAlarms.GroupBy(al => al.DeviceId).ToDictionary(al => al.Key, al => al.First().DeviceLabel);
                    }
                    else
                    {
                        Task.Factory.StartNew(new Func<IEnumerable<SystemAlarm>>(() => SystemAlarms.Where(al => al.DeviceId == SelectedDevice)))
                            .ContinueWith(new Action<Task<IEnumerable<SystemAlarm>>>((t) => UpdateFilteredAlarms(t.Result)));
                    }
                    break;
                case AlarmsFilterType.State:
                    Task.Factory.StartNew(new Func<object, IEnumerable<SystemAlarm>>(FilterAlarmsByState), SystemAlarms)
                        .ContinueWith(new Action<Task<IEnumerable<SystemAlarm>>>((t) => UpdateFilteredAlarms(t.Result) ));
                    break;
                case AlarmsFilterType.TimeStamp_Ascend:
                    Task.Factory.StartNew(new Func<IEnumerable<SystemAlarm>>(() => SystemAlarms.OrderBy(al => al.TimeStamp)))
                        .ContinueWith(new Action<Task<IEnumerable<SystemAlarm>>>((t) => UpdateFilteredAlarms(t.Result)));
                    break;
                case AlarmsFilterType.TimeStamp_Descend:
                    Task.Factory.StartNew(new Func<IEnumerable<SystemAlarm>>(() => SystemAlarms.OrderByDescending(al => al.TimeStamp)))
                        .ContinueWith(new Action<Task<IEnumerable<SystemAlarm>>>((t) => UpdateFilteredAlarms(t.Result)));
                    break;
                default:
                    break;
            }
        }

        public void UpdateFilteredAlarms(IEnumerable<SystemAlarm> systemAlarms)
        {
            Task.Factory.StartNew(() => FilteredAlarms = systemAlarms, CancellationToken.None, TaskCreationOptions.None, taskScheduler);
        }

        private IEnumerable<SystemAlarm> FilterAlarmsByState(object sysAlarms)
        {
            switch (SelectedAlarmState)
            {
                case AlarmState.Raised:
                    return ((IList<SystemAlarm>)sysAlarms).OrderByDescending(al => al.RaisedTimeStamp);
                case AlarmState.Acknowledged:
                    return ((IList<SystemAlarm>)sysAlarms).OrderByDescending(al => al.AcknowledgedTimeStamp);
                case AlarmState.Resolved:
                default:
                    return ((IList<SystemAlarm>)sysAlarms).OrderByDescending(al => al.TimeStamp);
            }
        }

        #region Commands
        private ICommand _loadSystemAlarmsCommand;
        public ICommand LoadSystemAlarmsCommand
        {
            get => _loadSystemAlarmsCommand ?? (_loadSystemAlarmsCommand = new DelegateCommand(LoadSystemAlarms));
            set => SetProperty(ref _loadSystemAlarmsCommand, value);
        }

        private ICommand _filterAlarmsCommand;
        public ICommand FilterAlarmsCommand
        {
            get => _filterAlarmsCommand ?? (_filterAlarmsCommand = new DelegateCommand<object>(FilterAlarms));
            set => SetProperty(ref _filterAlarmsCommand, value);
        }
        #endregion

        #region Properties
        public IEnumerable<SystemAlarm> SystemAlarms { get; set; } = new List<SystemAlarm>();
        public IEnumerable<SystemAlarm> FilteredAlarms { get; set; } = new List<SystemAlarm>();
        
        public IEnumerable<AlarmsFilterType> AlarmsFilterTypeValues
        {
            get => Enum.GetValues(typeof(AlarmsFilterType)).Cast<AlarmsFilterType>();
        }

        public IEnumerable<AlarmState> AlarmStateValues
        {
            get => new SystemAlarm().AlarmStateValues;
        }

        private AlarmState _selectedAlarmState = AlarmState.Acknowledged;
        public AlarmState SelectedAlarmState
        {
            get => _selectedAlarmState;
            set => SetProperty(ref _selectedAlarmState, value);
        }

        public IEnumerable<AlarmType> AlarmTypeValues
        {
            get => new SystemAlarm().AlarmTypeValues;
        }

        private AlarmType _selectedAlarmType = AlarmType.Process;
        public AlarmType SelectedAlarmType
        {
            get => _selectedAlarmType;
            set => SetProperty(ref _selectedAlarmType, value);
        }

        private Dictionary<string, string> _fieldDevices;
        public Dictionary<string, string> FieldDevices
        {
            get => _fieldDevices ?? (_fieldDevices = new Dictionary<string, string>());
            set => SetProperty(ref _fieldDevices, value);
        }

        private string _selectedDevice;
        public string SelectedDevice
        {
            get => _selectedDevice;
            set => SetProperty(ref _selectedDevice, value);
        }
        #endregion
    }

    public enum AlarmsFilterType
    {
        None,
        Type,
        Device,
        State,
        TimeStamp_Ascend,
        TimeStamp_Descend
    }
}
