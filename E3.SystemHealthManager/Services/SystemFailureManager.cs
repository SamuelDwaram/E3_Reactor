using E3.DialogServices.Model;
using E3.ReactorManager.Interfaces.DataAbstractionLayer;
using E3.ReactorManager.Interfaces.DataAbstractionLayer.Data;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;
using E3.SystemHealthManager.Models;
using E3.SystemHealthManager.ViewModels;
using E3.SystemHealthManager.Views;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity;

namespace E3.SystemHealthManager.Services
{
    public class SystemFailuresManager : ISystemFailuresManager
    {
        private readonly IUnityContainer unityContainer;
        private readonly IFieldDevicesCommunicator fieldDevicesCommunicator;
        private readonly IDialogServiceProvider dialogServiceProvider;
        private readonly IDatabaseReader databaseReader;
        private readonly IDatabaseWriter databaseWriter;
        private readonly ISystemFailurePoliciesManager systemFailurePoliciesManager;
        private IList<SystemFailure> systemFailures = new List<SystemFailure>();
        private IList<FailureNotificationInUI> FailureNotificationsInUi = new List<FailureNotificationInUI>();
        private readonly TaskScheduler taskScheduler;

        public event RefreshSystemFailuresEventHandler RefreshSystemFailures;

        public SystemFailuresManager(IUnityContainer unityContainer, IFieldDevicesCommunicator fieldDevicesCommunicatorr, IDialogServiceProvider dialogServiceProvider, IDatabaseReader databaseReader, IDatabaseWriter databaseWriter, ISystemFailurePoliciesManager systemFailurePoliciesManager)
        {
            taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            this.unityContainer = unityContainer;
            this.fieldDevicesCommunicator = fieldDevicesCommunicatorr;
            this.dialogServiceProvider = dialogServiceProvider;
            this.databaseReader = databaseReader;
            this.databaseWriter = databaseWriter;
            this.systemFailurePoliciesManager = systemFailurePoliciesManager;

            Task.Factory.StartNew(new Action(() => Thread.Sleep(5000)))
                .ContinueWith(new Func<Task, IList<SystemFailure>>(LoadSystemFailures))
                .ContinueWith(new Func<Task<IList<SystemFailure>>, IList<SystemFailure>>(PublishFailuresToOtherModules))
                .ContinueWith(new Func<Task<IList<SystemFailure>>, IList<SystemFailure>>(NotifyFailuresToUi))
                .ContinueWith(new Func<Task<IList<SystemFailure>>, IList<SystemFailure>>(RaiseAudioBeep))
                .ContinueWith(new Action<Task<IList<SystemFailure>>>(CheckForFailures))
                .ContinueWith(new Action<Task>(RestartCheckForFailuresTask));
        }

        private IList<SystemFailure> RaiseAudioBeep(Task<IList<SystemFailure>> task)
        {
            if (task.Result.Any(Failure => Failure.FailureState == FailureState.Raised))
            {
                Console.Beep(5000, 800);
            }

            return task.Result;
        }

        private IList<SystemFailure> PublishFailuresToOtherModules(Task<IList<SystemFailure>> task)
        {
            PublishRefreshSystemFailuresEvent(task.Result);
            return task.Result;
        }

        private IList<SystemFailure> LoadSystemFailures(Task task)
        {
            IList<SystemFailure> FailuresInDB = new List<SystemFailure>();
            foreach (DataRow row in databaseReader.ExecuteReadCommand("GetAllSystemFailures", CommandType.StoredProcedure).AsEnumerable())
            {
                FailuresInDB.Add(new SystemFailure
                {
                    Id = Convert.ToInt32(row["Id"]),
                    SystemFailurePolicyId = Convert.ToInt32(row["SystemFailurePolicyId"]),
                    DeviceId = Convert.ToString(row["DeviceId"]),
                    DeviceLabel = Convert.ToString(row["DeviceLabel"]),
                    Title = Convert.ToString(row["Title"]),
                    TroubleShootMessage = Convert.ToString(row["TroubleShootMessage"]),
                    FailedResourceLabel = Convert.ToString(row["FailedResourceLabel"]),
                    FailureState = (FailureState)Enum.Parse(typeof(FailureState), Convert.ToString(row["FailureState"])),
                    FailureType = (FailureType)Enum.Parse(typeof(FailureType), Convert.ToString(row["FailureType"])),
                    TimeStamp = Convert.ToDateTime(row["TimeStamp"]),
                    RaisedTimeStamp = row["RaisedTimeStamp"].ToString() == string.Empty ? default : Convert.ToDateTime(row["RaisedTimeStamp"]),
                    AcknowledgedTimeStamp = row["AcknowledgedTimeStamp"].ToString() == string.Empty ? default : Convert.ToDateTime(row["AcknowledgedTimeStamp"]),
                });
            }

            //Update the local Failures variable 
            systemFailures = FailuresInDB;

            return FailuresInDB;
        }

        private void RestartCheckForFailuresTask(Task task)
        {
            Task.Factory.StartNew(new Action(() => Thread.Sleep(2000)))
                .ContinueWith(new Func<Task, IList<SystemFailure>>(LoadSystemFailures))
                .ContinueWith(new Func<Task<IList<SystemFailure>>, IList<SystemFailure>>(PublishFailuresToOtherModules))
                .ContinueWith(new Func<Task<IList<SystemFailure>>, IList<SystemFailure>>(NotifyFailuresToUi))
                .ContinueWith(new Func<Task<IList<SystemFailure>>, IList<SystemFailure>>(RaiseAudioBeep))
                .ContinueWith(new Action<Task<IList<SystemFailure>>>(CheckForFailures))
                .ContinueWith(new Action<Task>(RestartCheckForFailuresTask));
        }

        private IList<SystemFailure> NotifyFailuresToUi(Task<IList<SystemFailure>> task)
        {
            task.Result.ToList().ForEach(Failure => {
                if (Failure.FailureState == FailureState.Raised && !FailureNotificationsInUi.Any(n => n.FailureId == Failure.Id && !n.UiAcknowledged))
                {
                    //Add Failure to the notification list
                    FailureNotificationsInUi.Add(new FailureNotificationInUI { 
                        FailureId = Failure.Id,
                    });

                    //Send Notification to Ui
                    SystemFailureNotificationViewModel failureNotificationViewModel = unityContainer.Resolve<SystemFailureNotificationViewModel>();
                    failureNotificationViewModel.SystemFailure = Failure;
                    Task.Factory.StartNew(new Action(() =>
                    {
                        dialogServiceProvider.ShowDynamicDialogWindow("System Failure", default,
                            new SystemFailureNotificationView() { DataContext = failureNotificationViewModel });
                    }), CancellationToken.None, TaskCreationOptions.None, taskScheduler);
                }
            });

            return task.Result;
        }

        public void CheckForFailures(Task<IList<SystemFailure>> task)
        {
            if (fieldDevicesCommunicator.GetAllFieldDeviceIdentifiers().Count == 0)
            {
                //return if field devices communicator is not loaded yet
                return;
            }

            IList<SystemFailure> currentSystemFailures = task.Result;
            foreach (SystemFailurePolicy systemFailurePolicy in systemFailurePoliciesManager.GetAll().Where(p => p.Status == true))
            {
                string liveData = fieldDevicesCommunicator.ReadFieldPointValue<string>(systemFailurePolicy.DeviceId, systemFailurePolicy.FailedResourceLabel);

                if (liveData == systemFailurePolicy.TargetValue)
                {
                    if (systemFailurePolicy.FailureResourceType == FailureResourceType.Device)
                    {
                        string title = $"{systemFailurePolicy.DeviceLabel} Not responding";
                        //FailedResourceLabel = FieldPointLabel
                        CheckExistingFailuresAndRaiseFailureIfRequired(currentSystemFailures, systemFailurePolicy, systemFailurePolicy.FailedResourceLabel, title, FailureType.System);
                    }
                    else if (systemFailurePolicy.FailureResourceType == FailureResourceType.Controller)
                    {
                        //FailedResourceLabel = ControllerLabel
                        string deviceType = fieldDevicesCommunicator.GetFieldDeviceType(systemFailurePolicy.DeviceId);
                        Dictionary<string, string> controller = fieldDevicesCommunicator.GetConnectedController(systemFailurePolicy.DeviceId, systemFailurePolicy.FailedResourceLabel);
                        string title = $"{controller["Label"]} Not responding";
                        CheckExistingFailuresAndRaiseFailureIfRequired(currentSystemFailures, systemFailurePolicy, systemFailurePolicy.FailedResourceLabel, title, FailureType.System);
                    }
                }
                else
                {
                    // Live Data Restored to normal conditions
                    ModifyFailureState(FailureState.Resolved, systemFailurePolicyId: systemFailurePolicy.Id, currentSystemFailures);
                }
            }
        }

        private void ModifyFailureState(FailureState changedFailureState, int systemFailurePolicyId = 0, IList<SystemFailure> existingFailures = null, int FailureId = 0)
        {
            IList<DbParameterInfo> dbParameters = new List<DbParameterInfo>();
            if (FailureId == 0)
            {
                IEnumerable<SystemFailure> FailuresWithGivenPolicyId = existingFailures.Where(Failure => Failure.SystemFailurePolicyId == systemFailurePolicyId && Failure.FailureState != FailureState.Resolved);
                if (FailuresWithGivenPolicyId.Count() > 0)
                {
                    //Find the Failure Id
                    dbParameters.Add(new DbParameterInfo("Id", FailuresWithGivenPolicyId.OrderByDescending(Failure => Failure.TimeStamp).First().Id, DbType.Int32));
                }
            }
            else
            {
                dbParameters.Add(new DbParameterInfo("Id", FailureId, DbType.Int32));
            }
            dbParameters.Add(new DbParameterInfo("FailureState", changedFailureState, DbType.String));

            //Execute the DbWrite Command only if Id parameter is added to DbParameters
            if (dbParameters.Any(parameter => parameter.Name == "Id"))
            {
                databaseWriter.ExecuteWriteCommand("ModifyFailureState", CommandType.StoredProcedure, dbParameters);
            }
        }

        private void CheckExistingFailuresAndRaiseFailureIfRequired(IList<SystemFailure> existingFailures, SystemFailurePolicy systemFailurePolicy, string failedResourceLabel, string title = null, FailureType failureType = FailureType.System)
        {
            if (existingFailures.Any(Failure => Failure.SystemFailurePolicyId == systemFailurePolicy.Id
                                        && Failure.FailureState != FailureState.Resolved))
            {
                // Failure already raised. Skip.
            }
            else
            {
                RaiseFailure(systemFailurePolicy, failureType, failedResourceLabel, title);
            }
        }

        private void RaiseFailure(SystemFailurePolicy systemFailurePolicy, FailureType failureType, string failedResourceLabel, string title = null)
        {
            IList<DbParameterInfo> dbParameters = new List<DbParameterInfo>
            {
                new DbParameterInfo("SystemFailurePolicyId", systemFailurePolicy.Id, DbType.Int32),
                new DbParameterInfo("DeviceId", systemFailurePolicy.DeviceId, DbType.String),
                new DbParameterInfo("DeviceLabel", systemFailurePolicy.DeviceLabel, DbType.String),
                new DbParameterInfo("Title", title ?? systemFailurePolicy.Title, DbType.String),
                new DbParameterInfo("TroubleShootMessage", systemFailurePolicy.TroubleShootMessage, DbType.String),
                new DbParameterInfo("FailedResourceLabel", failedResourceLabel, DbType.String),
                new DbParameterInfo("FailureState", FailureState.Raised, DbType.String),
                new DbParameterInfo("FailureType", failureType, DbType.String),
                new DbParameterInfo("TimeStamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), DbType.String)
            };
            databaseWriter.ExecuteWriteCommand("InsertSystemFailure", CommandType.StoredProcedure, dbParameters);
        }

        private void PublishRefreshSystemFailuresEvent(IList<SystemFailure> systemFailures)
        {
            if (RefreshSystemFailures == null)
            {
                //No Subscribers
            }
            else
            {
                foreach (Delegate receiver in RefreshSystemFailures.GetInvocationList())
                {
                    ((RefreshSystemFailuresEventHandler)receiver).BeginInvoke(systemFailures, null, null);
                }
            }
        }

        public void Acknowledge(int FailureId)
        {
            ModifyFailureState(FailureState.Acknowledged, FailureId:FailureId);

            //Update FailureNotification List
            FailureNotificationsInUi.Where(n => n.FailureId == FailureId).ToList().ForEach(n => n.UiAcknowledged = true);
        }

        #region Get Failures
        public IEnumerable<SystemFailure> GetAll() => systemFailures;

        public IEnumerable<SystemFailure> GetSystemFailuresForDevice(string deviceId)
        {
            IEnumerable<SystemFailure> FailuresInDevice = new List<SystemFailure>();
            lock (systemFailures)
            {
                FailuresInDevice = systemFailures.Where(Failure => Failure.DeviceId == deviceId);
            }
            return FailuresInDevice;
        }
        #endregion
    }
}
