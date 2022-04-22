using E3.AuditTrailManager.Model;
using E3.AuditTrailManager.Model.Enums;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer.Data;
using E3.UserManager.Model.Data;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Anathem.Ui.ViewModels
{
    public class ProcessFlowViewModel : BindableBase
    {
        private readonly IFieldDevicesCommunicator fieldDevicesCommunicator;
        private readonly IRegionManager regionManager;
        private readonly IAuditTrailManager auditTrailManager;
        private readonly TaskScheduler taskScheduler;
        private readonly User user;
        
        public ProcessFlowViewModel(IFieldDevicesCommunicator fieldDevicesCommunicator, IRegionManager regionManager, IAuditTrailManager auditTrailManager)
        {

            taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            this.fieldDevicesCommunicator = fieldDevicesCommunicator;
            this.regionManager = regionManager;
            this.auditTrailManager = auditTrailManager;
            Task.Factory.StartNew(new Func<Dictionary<string, string>>(() => {
                return (from SensorsDataSet s in fieldDevicesCommunicator.GetFieldDeviceData(DeviceId).SensorsData
                        from FieldPoint fp in s.SensorsFieldPoints
                        select new KeyValuePair<string, string>(fp.Label, fp.Value)).ToDictionary(kv => kv.Key, kv => kv.Value);
            })).ContinueWith(t => {
                foreach (KeyValuePair<string, string> item in t.Result)
                {
                    AddToParameters(item.Key, item.Value);
                }
            }).ContinueWith(t => this.fieldDevicesCommunicator.FieldPointDataReceived += FieldDevicesCommunicator_FieldPointDataReceived);
            user = (User)Application.Current.Resources["LoggedInUser"];
            RecipeMessage = fieldDevicesCommunicator.ReadFieldPointValue<string>(DeviceId, "RecipeMessage");
        }

        private void FieldDevicesCommunicator_FieldPointDataReceived(object sender, FieldPointDataReceivedArgs args)
        {
            AddToParameters(args.FieldPointIdentifier, args.NewFieldPointData);
            Test();

        }
        private void Test()
        {
            if (Parameters.ContainsKey("MVA_Pressure"))
            {
                Pressure_1 = Parameters["MVA_Pressure"];
            }
            if (Parameters.ContainsKey("RV50L_Pressure"))
            {
                Pressure_2 = Parameters["RV50L_Pressure"];
            }
            if (Parameters.ContainsKey("RV25L_Pressure"))
            {
                Pressure_3 = Parameters["RV25L_Pressure"];
            }
            if (Parameters.ContainsKey("InstrumentAirPressure"))
            {
                Pressure_4 = Parameters["InstrumentAirPressure"];
            }
            if (Parameters.ContainsKey("MVA_Temperature"))
            {
                Temperature_1 = Parameters["MVA_Temperature"];
            }
            if (Parameters.ContainsKey("RV50L_Temperature"))
            {
                Temperature_2 = Parameters["RV50L_Temperature"];
            }
            if (Parameters.ContainsKey("RV25L_Temperature"))
            {
                Temperature_3 = Parameters["RV25L_Temperature"];
            }
            if (Parameters.ContainsKey("ChillerTemperature"))
            {
                Temperature_4 = Parameters["ChillerTemperature"];
            }
            if (Parameters.ContainsKey("MVA_Level"))
            {
                Level_1 = Parameters["MVA_Level"];
            }
            if (Parameters.ContainsKey("RV50L_Level"))
            {
                Level_2 = Parameters["RV50L_Level"];
            }
            if (Parameters.ContainsKey("RV25L_Level"))
            {
                Level_3 = Parameters["RV25L_Level"];
            }
            if (Parameters.ContainsKey("SSTank_Level"))
            {
                Level_4 = Parameters["SSTank_Level"];
            }
            if (Parameters.ContainsKey("MVA_StirrerSpeed"))
            {
                StirrerFeedback_1 = Parameters["MVA_StirrerSpeed"];
            }
            if (Parameters.ContainsKey("RV50L_StirrerSpeed"))
            {
                StirrerFeedback_2 = Parameters["RV50L_StirrerSpeed"];
            }
            if (Parameters.ContainsKey("RV25L_StirrerSpeed"))
            {
                StirrerFeedback_3 = Parameters["RV25L_StirrerSpeed"];
            }
            if (Parameters.ContainsKey("RecipeMessage"))
            {
                RecipeMessage = Parameters["RecipeMessage"];
            }
        }
        private void AddToParameters(string fieldPointIdentifier, string newFieldPointData)
        {
            if(fieldPointIdentifier == "RecipeMessage")
            {
                RecipeMessage = newFieldPointData;
            }
            if (Parameters.ContainsKey(fieldPointIdentifier))
            {
                if (Parameters[fieldPointIdentifier] == newFieldPointData)
                {
                    // skip. no need to do anything.
                }
                else
                {
                    Parameters[fieldPointIdentifier] = newFieldPointData;
                    NotifyUi(nameof(Parameters));

                }
            }
            else
            {
                Parameters.Add(fieldPointIdentifier, newFieldPointData);
            }
        }

        private void NotifyUi(string propName)
        {
            Task.Factory.StartNew(() => RaisePropertyChanged(propName), CancellationToken.None, TaskCreationOptions.None, taskScheduler);
        }

        public ICommand SendCommandToDevice
        {
            get => new DelegateCommand<string>(param => {
                string[] paramInfo = param.Split('|');
                fieldDevicesCommunicator.SendCommandToDevice(DeviceId, paramInfo[0], "bool", bool.TryParse(paramInfo[1], out bool parseResult) ? (!parseResult).ToString() : bool.FalseString);
                auditTrailManager.RecordEventAsync($"Changed {paramInfo[0]} in {DeviceId} from {parseResult} to {!parseResult}", user.Name, EventTypeEnum.ChangedSetPoint);
            });
        }

        public string DeviceId { get; } = "Reactor_1";
        public ICommand NavigateCommand => new DelegateCommand<string>(page => regionManager.RequestNavigate("SelectedViewPane", page));
        public Dictionary<string, string> Parameters { get; } = new Dictionary<string, string>();
        private void ChangeChillerSetPoint()
        {
            //first convert the user entered stirrer Speed SetPoint to integer
            var toBeSetChillerSetPoint = Convert.ToInt16(Temperature_5);

            if (toBeSetChillerSetPoint <= 50 && toBeSetChillerSetPoint >= -25)
            {
                fieldDevicesCommunicator
                        .SendCommandToDevice(DeviceId, "ChillerSetpoint", "double", toBeSetChillerSetPoint.ToString());
                //auditTrailManager.RecordEventAsync($"Changed {DeviceId} Stirrer Speed SetPoint from {OldStirrerSetPoint} to {toBeSetStirrerSpeedSetPoint}", UserDetails.Name, EventTypeEnum.ChangedSetPoint);
            }
            else if (toBeSetChillerSetPoint > 50)
            {
                MessageBox.Show("Maximum Chiller Temperature is less than 50 °C");
                Temperature_5 = null;
            }
            else if (toBeSetChillerSetPoint < -25)
            {
                MessageBox.Show("Minimum  Chiller Temperature is -25 °C");
                Temperature_5 = null;
            }
        }

        public ICommand ChangeChillerSetPointCommand
        {
            get => new DelegateCommand(ChangeChillerSetPoint);
        }
        private string pressure_1;

        public string Pressure_1
        {
            get { return pressure_1; }
            set
            {
                pressure_1 = value;
                RaisePropertyChanged();
            }
        }
        private string pressure_2;

        public string Pressure_2
        {
            get { return pressure_2; }
            set
            {
                pressure_2 = value;
                RaisePropertyChanged();
            }
        }
        private string pressure_3;

        public string Pressure_3
        {
            get { return pressure_3; }
            set
            {
                pressure_3 = value;
                RaisePropertyChanged();
            }
        }
        private string pressure_4;

        public string Pressure_4
        {
            get { return pressure_4; }
            set
            {
                pressure_4 = value;
                RaisePropertyChanged();
            }
        }
        private string temperature_1;

        public string Temperature_1
        {
            get { return temperature_1; }
            set
            {
                temperature_1 = value;
                RaisePropertyChanged();
            }
        }
        private string temperature_2;

        public string Temperature_2
        {
            get { return temperature_2; }
            set
            {
                temperature_2 = value;
                RaisePropertyChanged();
            }
        }
        private string temperature_3;

        public string Temperature_3
        {
            get { return temperature_3; }
            set
            {
                temperature_3 = value;
                RaisePropertyChanged();
            }
        }
        private string temperature_4;

        public string Temperature_4
        {
            get { return temperature_4; }
            set
            {
                temperature_4 = value;
                RaisePropertyChanged();
            }
        }
        private string temperature_5;

        public string Temperature_5
        {
            get { return temperature_5; }
            set
            {
                temperature_5 = value;
                RaisePropertyChanged();
            }
        }
        private string level_1;

        public string Level_1
        {
            get { return level_1; }
            set
            {
                level_1 = value;
                RaisePropertyChanged();
            }
        }
        private string level_2;

        public string Level_2
        {
            get { return level_2; }
            set
            {
                level_2 = value;
                RaisePropertyChanged();
            }
        }
        private string level_3;

        public string Level_3
        {
            get { return level_3; }
            set
            {
                level_3 = value;
                RaisePropertyChanged();
            }
        }
        private string level_4;

        public string Level_4
        {
            get { return level_4; }
            set
            {
                level_4 = value;
                RaisePropertyChanged();
            }
        }

        private string stirrerFeedback_1;

        public string StirrerFeedback_1
        {
            get { return stirrerFeedback_1; }
            set
            {
                stirrerFeedback_1 = value;
                RaisePropertyChanged();
            }
        }
        private string stirrerFeedback_2;

        public string StirrerFeedback_2
        {
            get { return stirrerFeedback_2; }
            set
            {
                stirrerFeedback_2 = value;
                RaisePropertyChanged();
            }
        }
        private string stirrerFeedback_3;

        public string StirrerFeedback_3
        {
            get { return stirrerFeedback_3; }
            set
            {
                stirrerFeedback_3 = value;
                RaisePropertyChanged();
            }
        }

        private string _recipeMessage;

        public string RecipeMessage
        {
            get { return _recipeMessage; }
            set
            {
                _recipeMessage = value;
                RaisePropertyChanged();
            }
        }
    }
}
