using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;
using E3.SystemAlarmManager.Models;
using E3.SystemAlarmManager.Services;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Unity.Builder.Operation;

namespace E3.SystemAlarmManager.ViewModels
{
    public class AlarmNotificationViewModel : BindableBase
    {
        private readonly ISystemAlarmsManager systemAlarmsManager;
        private readonly IFieldDevicesCommunicator fieldDevicesCommunicator;

        public AlarmNotificationViewModel(ISystemAlarmsManager systemAlarmsManager, IFieldDevicesCommunicator fieldDevicesCommunicator)
        {
            this.systemAlarmsManager = systemAlarmsManager;
            this.fieldDevicesCommunicator = fieldDevicesCommunicator;
        }

        public void AcknowledgeAlarm(object alarmId)
        {
            systemAlarmsManager.Acknowledge((int)alarmId);
            CloseCurrentWindow();
            fieldDevicesCommunicator.SendCommandToDevice(DeviceId, "AlarmRaised", "bool", Boolean.FalseString.ToString());
        }

        public void CloseCurrentWindow()
        {
            if (CurrentWindow == null)
            {
                throw new Exception("Current Window is null");
            }
            else
            {
                CurrentWindow.Close();
            }
        }

        public void SetCurrentWindow(UserControl userControl)
        {
            CurrentWindow = Window.GetWindow(userControl);
        }

        #region Commands
        private ICommand _acknowledgeAlarmCommand;
        public ICommand AcknowledgeAlarmCommand
        {
            get => _acknowledgeAlarmCommand ?? (_acknowledgeAlarmCommand = new DelegateCommand<object>(AcknowledgeAlarm));
            set => SetProperty(ref _acknowledgeAlarmCommand, value);
        }

        private ICommand _setCurrentWindowCommand;
        public ICommand SetCurrentWindowCommand
        {
            get => _setCurrentWindowCommand ?? (_setCurrentWindowCommand = new DelegateCommand<UserControl>(SetCurrentWindow));
            set => SetProperty(ref _setCurrentWindowCommand, value);
        }
        #endregion

        #region Properties
        private SystemAlarm _systemAlarm;
        public SystemAlarm SystemAlarm
        {
            get => _systemAlarm ?? (_systemAlarm = new SystemAlarm());
            set => SetProperty(ref _systemAlarm, value);
        }

        private Window _currentWindow;
        public Window CurrentWindow
        {
            get => _currentWindow ?? (_currentWindow = new Window());
            set => SetProperty(ref _currentWindow, value);
        }
        public string DeviceId => "Reactor_1";
        #endregion
    }
}
