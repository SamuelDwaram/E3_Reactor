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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Anathem.Ui.ViewModels
{
    public class ParametersHostViewModel : BindableBase
    {
        private readonly IFieldDevicesCommunicator fieldDevicesCommunicator;
        private readonly IRegionManager regionManager;
        private readonly IAuditTrailManager auditTrailManager;
        private readonly TaskScheduler taskScheduler;
        private readonly User user;

        public ParametersHostViewModel(IFieldDevicesCommunicator fieldDevicesCommunicator, IRegionManager regionManager, IAuditTrailManager auditTrailManager)
        {
            taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            this.fieldDevicesCommunicator = fieldDevicesCommunicator;
            this.regionManager = regionManager;
            this.auditTrailManager = auditTrailManager;
            Task.Factory.StartNew(new Func<Dictionary<string, string>>(() => {
                return (from SensorsDataSet s in fieldDevicesCommunicator.GetFieldDeviceData(DeviceId).SensorsData
                from FieldPoint fp in s.SensorsFieldPoints
                select new KeyValuePair<string, string>(fp.Label,fp.Value)).ToDictionary(kv => kv.Key, kv => kv.Value);
            })).ContinueWith(t => {
                foreach (KeyValuePair<string, string> item in t.Result)
                {
                    AddToParameters(item.Key, item.Value);
                }
            }).ContinueWith(t => this.fieldDevicesCommunicator.FieldPointDataReceived += FieldDevicesCommunicator_FieldPointDataReceived);
            user = (User)Application.Current.Resources["LoggedInUser"];
        }

        private void FieldDevicesCommunicator_FieldPointDataReceived(object sender, FieldPointDataReceivedArgs args)
        {
            AddToParameters(args.FieldPointIdentifier, args.NewFieldPointData);
        }

        private void AddToParameters(string fieldPointIdentifier, string newFieldPointData)
        {
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
    }
}
