using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer.Data;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Anathem.Ui.ViewModels
{
    public class ReactorControlViewModel : BindableBase, IRegionMemberLifetime
    {
        private readonly IFieldDevicesCommunicator fieldDevicesCommunicator;
        private readonly IRegionManager regionManager;
        private readonly TaskScheduler taskScheduler;

        public ReactorControlViewModel(IFieldDevicesCommunicator fieldDevicesCommunicator, IRegionManager regionManager)
        {
            taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            this.fieldDevicesCommunicator = fieldDevicesCommunicator;
            this.regionManager = regionManager;
            this.fieldDevicesCommunicator.FieldPointDataReceived += FieldDevicesCommunicator_FieldPointDataReceived;
            Task.Factory.StartNew(new Action(LoadDeviceParametersFromHardwareLayer))
                .ContinueWith(t => UpdateUi());
        }

        private void LoadDeviceParametersFromHardwareLayer()
        {
            foreach (SensorsDataSet sensorsDataSet in fieldDevicesCommunicator.GetFieldDeviceData(DeviceId).SensorsData)
            {
                foreach (FieldPoint fieldPoint in sensorsDataSet.SensorsFieldPoints)
                {
                    if (fieldPoint.TypeOfAddress == "FieldPoint")
                    {
                        if (ParameterDictionary.ContainsKey(fieldPoint.Label))
                        {
                            ParameterDictionary[fieldPoint.Label] = fieldPoint.Value;
                        }
                        else
                        {
                            ParameterDictionary.Add(fieldPoint.Label, fieldPoint.Value);
                        }
                        UpdateSetPoints(fieldPoint.Label, fieldPoint.Value);
                    }
                }
            }
        }

        private void FieldDevicesCommunicator_FieldPointDataReceived(object sender, FieldPointDataReceivedArgs liveData)
        {

            if (ParameterDictionary.ContainsKey(liveData.FieldPointIdentifier))
            {
                ParameterDictionary[liveData.FieldPointIdentifier] = liveData.NewFieldPointData;
                UpdateUi();
            }
            UpdateSetPoints(liveData.FieldPointIdentifier, liveData.NewFieldPointData);

        }
        private void UpdateSetPoints(string fpId, string data)
        {
            if (fpId.Contains(nameof(StirrerSetpoint_1)))
            {
                StirrerSetpoint_1 = data;
            }
            else if (fpId.Contains(nameof(StirrerSetpoint_2)))
            {
                StirrerSetpoint_2 = data;
            }
            else if (fpId.Contains(nameof(StirrerSetpoint_3)))
            {
                StirrerSetpoint_3 = data;
            }
        }

        private void UpdateUi()
        {
            StirrerFeedback_1 = Convert.ToString(Convert.ToSingle(ParameterDictionary["StirrerFeedback_1"]));
            StirrerFeedback_2 = Convert.ToString(Convert.ToSingle(ParameterDictionary["StirrerFeedback_2"]));
            StirrerFeedback_3 = Convert.ToString(Convert.ToSingle(ParameterDictionary["StirrerFeedback_3"]));
            Task.Factory.StartNew(new Action(() => RaisePropertyChanged(nameof(ParameterDictionary))), CancellationToken.None, TaskCreationOptions.None, taskScheduler);
        }
        private void ChangeStirrer1SetPoint()
        {
            //first convert the user entered stirrer Speed SetPoint to integer
            var toBeSetStirrer1SpeedSetPoint = Convert.ToInt16(StirrerSetpoint_1);

            if (toBeSetStirrer1SpeedSetPoint <= 200 && toBeSetStirrer1SpeedSetPoint >= 0)
            {
                fieldDevicesCommunicator
                        .SendCommandToDevice(DeviceId, "StirrerSetpoint_1", "double", toBeSetStirrer1SpeedSetPoint.ToString());
                //auditTrailManager.RecordEventAsync($"Changed {DeviceId} Stirrer Speed SetPoint from {OldStirrerSetPoint} to {toBeSetStirrerSpeedSetPoint}", UserDetails.Name, EventTypeEnum.ChangedSetPoint);
            }
            else if (toBeSetStirrer1SpeedSetPoint > 200)
            {
                MessageBox.Show("Maximum Stirrer Speed is less than 200 rpm");
                StirrerSetpoint_1 = null;
            }
            else if (toBeSetStirrer1SpeedSetPoint < 0)
            {
                MessageBox.Show("Minimum Stirrer Speed is 0 rpm");
                StirrerSetpoint_1 = null;
            }
        }
        private void ChangeStirrer2SetPoint()
        {
            //first convert the user entered stirrer Speed SetPoint to integer
            var toBeSetStirrer2SpeedSetPoint = Convert.ToInt16(StirrerSetpoint_2);

            if (toBeSetStirrer2SpeedSetPoint <= 200 && toBeSetStirrer2SpeedSetPoint >= 0)
            {
                fieldDevicesCommunicator
                        .SendCommandToDevice(DeviceId, "StirrerSetpoint_2", "double", toBeSetStirrer2SpeedSetPoint.ToString());
                //auditTrailManager.RecordEventAsync($"Changed {DeviceId} Stirrer Speed SetPoint from {OldStirrerSetPoint} to {toBeSetStirrerSpeedSetPoint}", UserDetails.Name, EventTypeEnum.ChangedSetPoint);
            }
            else if (toBeSetStirrer2SpeedSetPoint > 200)
            {
                MessageBox.Show("Maximum Stirrer Speed is less than 200 rpm");
                StirrerSetpoint_2 = null;
            }
            else if (toBeSetStirrer2SpeedSetPoint < 0)
            {
                MessageBox.Show("Minimum Stirrer Speed is 0 rpm");
                StirrerSetpoint_2 = null;
            }
        }
        private void ChangeStirrer3SetPoint()
        {
            //first convert the user entered stirrer Speed SetPoint to integer
            var toBeSetStirrer3SpeedSetPoint = Convert.ToInt16(StirrerSetpoint_3);

            if (toBeSetStirrer3SpeedSetPoint <= 200 && toBeSetStirrer3SpeedSetPoint >= 0)
            {
                fieldDevicesCommunicator
                        .SendCommandToDevice(DeviceId, "StirrerSetpoint_3", "double", toBeSetStirrer3SpeedSetPoint.ToString());
                //auditTrailManager.RecordEventAsync($"Changed {DeviceId} Stirrer Speed SetPoint from {OldStirrerSetPoint} to {toBeSetStirrerSpeedSetPoint}", UserDetails.Name, EventTypeEnum.ChangedSetPoint);
            }
            else if (toBeSetStirrer3SpeedSetPoint > 200)
            {
                MessageBox.Show("Maximum Stirrer Speed is less than 200 rpm");
                StirrerSetpoint_3 = null;
            }
            else if (toBeSetStirrer3SpeedSetPoint < 0)
            {
                MessageBox.Show("Minimum Stirrer Speed is 0 rpm");
                StirrerSetpoint_3 = null;
            }
        }
        private void ChangeChillerSetPoint()
        {
            //first convert the user entered stirrer Speed SetPoint to integer
            var toBeSetChillerSetPoint = Convert.ToInt16(ChillerSetpoint);

            if (toBeSetChillerSetPoint <= 200 && toBeSetChillerSetPoint >= 0)
            {
                fieldDevicesCommunicator
                        .SendCommandToDevice(DeviceId, "ChillerSetpoint", "double", toBeSetChillerSetPoint.ToString());
                //auditTrailManager.RecordEventAsync($"Changed {DeviceId} Stirrer Speed SetPoint from {OldStirrerSetPoint} to {toBeSetStirrerSpeedSetPoint}", UserDetails.Name, EventTypeEnum.ChangedSetPoint);
            }
            else if (toBeSetChillerSetPoint > 50)
            {
                MessageBox.Show("Maximum Chiller Temperature is less than 50 °C");
                StirrerSetpoint_3 = null;
            }
            else if (toBeSetChillerSetPoint < 0)
            {
                MessageBox.Show("Minimum  Chiller Temperature is 0 °C");
                StirrerSetpoint_3 = null;
            }
        }
        public ICommand ChangeStirrer1SetPointCommand
        {
            get => new DelegateCommand(ChangeStirrer1SetPoint);
        }
        public ICommand ChangeStirrer2SetPointCommand
        {
            get => new DelegateCommand(ChangeStirrer2SetPoint);
        }
        public ICommand ChangeStirrer3SetPointCommand
        {
            get => new DelegateCommand(ChangeStirrer3SetPoint);
        }
        public ICommand ChangeChillerSetPointCommand
        {
            get => new DelegateCommand(ChangeChillerSetPoint);
        }
        public ICommand SendCommandToDevice
        {
            get => new DelegateCommand<string>(str => {
                string[] strArray = str.Split('|');
                fieldDevicesCommunicator.SendCommandToDevice(DeviceId, strArray[0], strArray[1], strArray[2]);
            });
        }

        public ICommand NavigateCommand
        {
            get => new DelegateCommand<string>(str => regionManager.RequestNavigate("SelectedViewPane", str));
        }

        public bool KeepAlive { get; set; } = false;
        public string DeviceId => "Reactor_1";
        public Dictionary<string, string> ParameterDictionary { get; set; } = new Dictionary<string, string>();
        private string _stirrerSetpoint_1;
        public string StirrerSetpoint_1
        {
            get { return _stirrerSetpoint_1; }
            set
            {
                _stirrerSetpoint_1 = value;
                RaisePropertyChanged();
            }
        }
        private string _stirrerSetpoint_2;
        public string StirrerSetpoint_2
        {
            get { return _stirrerSetpoint_2; }
            set
            {
                _stirrerSetpoint_2 = value;
                RaisePropertyChanged();
            }
        }
        private string _stirrerSetpoint_3;
        public string StirrerSetpoint_3
        {
            get { return _stirrerSetpoint_3; }
            set
            {
                _stirrerSetpoint_3 = value;
                RaisePropertyChanged();
            }
        }
        private string _chillerSetpoint = "25";
        public string ChillerSetpoint
        {
            get { return _chillerSetpoint; }
            set
            {
                _chillerSetpoint = value;
                RaisePropertyChanged();
            }
        }
        private string _stirrerFeedback_1;
        public string StirrerFeedback_1
        {
            get { return _stirrerFeedback_1; }
            set
            {
                _stirrerFeedback_1 = value;
                RaisePropertyChanged();
            }
        }
        private string _stirrerFeedback_2;
        public string StirrerFeedback_2
        {
            get { return _stirrerFeedback_2; }
            set
            {
                _stirrerFeedback_2 = value;
                RaisePropertyChanged();
            }
        }
        private string _stirrerFeedback_3;
        public string StirrerFeedback_3
        {
            get { return _stirrerFeedback_3; }
            set
            {
                _stirrerFeedback_3 = value;
                RaisePropertyChanged();
            }
        }

    }
}
