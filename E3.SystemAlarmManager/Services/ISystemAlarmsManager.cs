using E3.SystemAlarmManager.Models;
using System;
using System.Collections.Generic;

namespace E3.SystemAlarmManager.Services
{
    public interface ISystemAlarmsManager
    {
        void Acknowledge(int alarmId);

        IEnumerable<SystemAlarm> GetSystemAlarmsForDevice(string deviceId);

        IEnumerable<SystemAlarm> GetAll();

        IEnumerable<SystemAlarm> GetAll(string deviceId, DateTime startTime, DateTime endTime);

        event RefreshSystemAlarmsEventHandler RefreshSystemAlarms;
    }

    public delegate void RefreshSystemAlarmsEventHandler(IEnumerable<SystemAlarm> systemAlarms);
}
