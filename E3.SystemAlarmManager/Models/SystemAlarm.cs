using System;
using System.Collections.Generic;
using System.Linq;

namespace E3.SystemAlarmManager.Models
{
    public class SystemAlarm
    {
        public int Id { get; set; }

        public int SystemAlarmPolicyId { get; set; } = 0;

        public string SystemAlarmParameterName { get; set; } = string.Empty;

        public int SystemFailureId { get; set; } = 0;

        public string DeviceId { get; set; }

        public string DeviceLabel { get; set; }

        public string FieldPointLabel { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public AlarmState State { get; set; }

        public IEnumerable<AlarmState> AlarmStateValues
        {
            get => Enum.GetValues(typeof(AlarmState)).Cast<AlarmState>();
        }

        public IEnumerable<AlarmType> AlarmTypeValues
        {
            get => Enum.GetValues(typeof(AlarmType)).Cast<AlarmType>();
        }

        public AlarmType Type { get; set; }

        public DateTime TimeStamp { get; set; }

        public DateTime RaisedTimeStamp { get; set; }

        public DateTime AcknowledgedTimeStamp { get; set; }
    }

    public enum AlarmState
    {
        Raised,
        Acknowledged,
        Resolved
    }

    public enum AlarmType
    {
        Process,
        System
    }
}
