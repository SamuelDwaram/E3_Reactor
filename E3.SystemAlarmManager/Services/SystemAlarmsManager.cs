using E3.DialogServices.Model;
using E3.ReactorManager.Interfaces.DataAbstractionLayer;
using E3.ReactorManager.Interfaces.DataAbstractionLayer.Data;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;
using E3.SystemAlarmManager.Models;
using E3.SystemAlarmManager.ViewModels;
using E3.SystemAlarmManager.Views;
using E3.SystemHealthManager.Models;
using E3.SystemHealthManager.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity;

namespace E3.SystemAlarmManager.Services
{
    public class SystemAlarmsManager : ISystemAlarmsManager
    {
        private readonly IUnityContainer unityContainer;
        private readonly IFieldDevicesCommunicator fieldDevicesCommunicator;
        private readonly IDialogServiceProvider dialogServiceProvider;
        private readonly IDatabaseReader databaseReader;
        private readonly IDatabaseWriter databaseWriter;
        private readonly ISystemAlarmPoliciesManager systemAlarmPoliciesManager;
        private IList<SystemAlarm> systemAlarms = new List<SystemAlarm>();
        private IList<AlarmNotificationInUI> alarmNotificationsInUi = new List<AlarmNotificationInUI>();
        private readonly TaskScheduler taskScheduler;
        ISystemFailuresManager systemFailuresManager;
        IEnumerable<SystemFailure> systemFailures = new List<SystemFailure>();

        public event RefreshSystemAlarmsEventHandler RefreshSystemAlarms;

        public SystemAlarmsManager(IUnityContainer unityContainer, IFieldDevicesCommunicator fieldDevicesCommunicatorr, IDialogServiceProvider dialogServiceProvider, IDatabaseReader databaseReader, IDatabaseWriter databaseWriter, ISystemAlarmPoliciesManager systemAlarmPoliciesManager)
        {
            taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            this.unityContainer = unityContainer;
            this.fieldDevicesCommunicator = fieldDevicesCommunicatorr;
            this.dialogServiceProvider = dialogServiceProvider;
            this.databaseReader = databaseReader;
            this.databaseWriter = databaseWriter;
            this.systemAlarmPoliciesManager = systemAlarmPoliciesManager;

            if (unityContainer.IsRegistered<ISystemFailuresManager>())
            {
                this.systemFailuresManager = unityContainer.Resolve<ISystemFailuresManager>();
            }
            
            Task.Factory.StartNew(new Action(() => Thread.Sleep(15000)))
                .ContinueWith(new Func<Task, IList<SystemAlarm>>(LoadSystemAlarms))
                .ContinueWith(new Func<Task<IList<SystemAlarm>>, IList<SystemAlarm>>(PublishAlarmsToOtherModules))
                .ContinueWith(new Func<Task<IList<SystemAlarm>>, IList<SystemAlarm>>(NotifyAlarmsToUi))
                .ContinueWith(new Func<Task<IList<SystemAlarm>>, IList<SystemAlarm>>(RaiseAudioBeep))
                .ContinueWith(new Func<Task<IList<SystemAlarm>>, IList<SystemAlarm>>(CheckForAlarms))
                .ContinueWith(new Action<Task<IList<SystemAlarm>>>(CheckForSystemFailures))
                .ContinueWith(new Action<Task>(RestartCheckForAlarmsTask));
        }

        private void CheckForSystemFailures(Task<IList<SystemAlarm>> task)
        {
            IList<SystemAlarm> currentSystemAlarms = task.Result;
            if (systemFailuresManager == null)
            {
                //SystemFailureManager not registered. Continue.
            }
            else
            {
                foreach (SystemFailure systemFailure in systemFailuresManager.GetAll())
                {
                    List<SystemAlarm> failureAlarms
                        = currentSystemAlarms.Where(al => al.SystemFailureId == systemFailure.Id).ToList();
                    if (failureAlarms.Count() > 0)
                    {
                        //check for changes in the failure state 
                        if (failureAlarms.All(al => al.State.ToString() == systemFailure.FailureState.ToString()))
                        {
                            // All alarms are in sync with the failures. Continue.
                        }
                        else
                        {
                            failureAlarms.Where(al => al.State.ToString() != systemFailure.FailureState.ToString())
                                .ToList().ForEach(al => {
                                    UpdateSystemAlarm(al.Id, systemFailure.FailureState.ToString(), systemFailure.TimeStamp, systemFailure.RaisedTimeStamp, systemFailure.AcknowledgedTimeStamp);
                                });
                        }
                    }
                    else
                    {
                        SaveSystemAlarmToDatabase(new SystemAlarm { 
                            DeviceId = systemFailure.DeviceId,
                            DeviceLabel = systemFailure.DeviceLabel,
                            FieldPointLabel = systemFailure.FailedResourceLabel,
                            SystemFailureId = systemFailure.Id,
                            Title = systemFailure.Title,
                            Message = systemFailure.TroubleShootMessage,
                            State = (AlarmState)Enum.Parse(typeof(AlarmState), systemFailure.FailureState.ToString()),
                            Type = (AlarmType)Enum.Parse(typeof(AlarmType), systemFailure.FailureType.ToString()),
                            TimeStamp = systemFailure.TimeStamp,
                            RaisedTimeStamp = systemFailure.RaisedTimeStamp,
                            AcknowledgedTimeStamp = systemFailure.AcknowledgedTimeStamp,
                        });
                    }
                }
            }
        }

        private void UpdateSystemAlarm(int alarmId, string alarmState, DateTime timeStamp, DateTime raisedTimeStamp, DateTime acknowledgedTimeStamp)
        {
            IList<DbParameterInfo> dbParameters = new List<DbParameterInfo>
            {
                new DbParameterInfo("Id", alarmId, DbType.Int32),
                new DbParameterInfo("State", alarmState, DbType.String),
                new DbParameterInfo("TimeStamp", timeStamp, DbType.DateTime),
                new DbParameterInfo("RaisedTimeStamp", raisedTimeStamp, DbType.DateTime),
                new DbParameterInfo("AcknowledgedTimeStamp", acknowledgedTimeStamp, DbType.DateTime)
            };
            databaseWriter.ExecuteWriteCommand("UpdateSystemAlarm", CommandType.StoredProcedure, dbParameters);
        }

        private IList<SystemAlarm> RaiseAudioBeep(Task<IList<SystemAlarm>> task)
        {
            
            if (task.Result.Any(alarm => alarm.State == AlarmState.Raised))
            {
                fieldDevicesCommunicator
                       .SendCommandToDevice(DeviceId, "AlarmRaised", "bool", Boolean.TrueString.ToString());

                Console.Beep(5000,100);
            }
            else
            {
                

            }

            return task.Result;
        }

        private IList<SystemAlarm> PublishAlarmsToOtherModules(Task<IList<SystemAlarm>> task)
        {
            PublishRefreshSystemAlarmsEvent(task.Result);
            return task.Result;
        }

        private IList<SystemAlarm> LoadSystemAlarms(Task task)
        {
            IList<SystemAlarm> alarmsInDB = new List<SystemAlarm>();
            foreach (DataRow row in databaseReader.ExecuteReadCommand("GetAllSystemAlarms", CommandType.StoredProcedure).AsEnumerable())
            {
                alarmsInDB.Add(new SystemAlarm
                {
                    Id = Convert.ToInt32(row["Id"]),
                    SystemAlarmPolicyId = (string.IsNullOrWhiteSpace(row["SystemAlarmPolicyId"].ToString()) ? 0 : Convert.ToInt32(row["SystemAlarmPolicyId"])),
                    SystemAlarmParameterName = (string.IsNullOrWhiteSpace(row["SystemAlarmParameterName"].ToString()) ? "" : Convert.ToString(row["SystemAlarmParameterName"])),
                    SystemFailureId = (string.IsNullOrWhiteSpace(row["SystemFailureId"].ToString()) ? 0 : Convert.ToInt32(row["SystemFailureId"])),
                    DeviceId = Convert.ToString(row["DeviceId"]),
                    DeviceLabel = Convert.ToString(row["DeviceLabel"]),
                    FieldPointLabel = Convert.ToString(row["FieldPointLabel"]),
                    Title = Convert.ToString(row["Title"]),
                    Message = Convert.ToString(row["Message"]),
                    State = (AlarmState)Enum.Parse(typeof(AlarmState), Convert.ToString(row["State"])),
                    Type = (AlarmType)Enum.Parse(typeof(AlarmType), Convert.ToString(row["Type"])),
                    TimeStamp = Convert.ToDateTime(row["TimeStamp"]),
                    RaisedTimeStamp = row["RaisedTimeStamp"].ToString() == string.Empty ? default : Convert.ToDateTime(row["RaisedTimeStamp"]),
                    AcknowledgedTimeStamp = row["AcknowledgedTimeStamp"].ToString() == string.Empty ? default : Convert.ToDateTime(row["AcknowledgedTimeStamp"])
                });
            }

            //Update the local alarms variable 
            systemAlarms = alarmsInDB;

            return alarmsInDB;
        }

        private void RestartCheckForAlarmsTask(Task task)
        {
            Task.Factory.StartNew(new Action(() => Thread.Sleep(1000)))
                .ContinueWith(new Func<Task, IList<SystemAlarm>>(LoadSystemAlarms))
                .ContinueWith(new Func<Task<IList<SystemAlarm>>, IList<SystemAlarm>>(PublishAlarmsToOtherModules))
                .ContinueWith(new Func<Task<IList<SystemAlarm>>, IList<SystemAlarm>>(NotifyAlarmsToUi))
                .ContinueWith(new Func<Task<IList<SystemAlarm>>, IList<SystemAlarm>>(RaiseAudioBeep))
                .ContinueWith(new Func<Task<IList<SystemAlarm>>, IList<SystemAlarm>>(CheckForAlarms))
                .ContinueWith(new Action<Task<IList<SystemAlarm>>>(CheckForSystemFailures))
                .ContinueWith(new Action<Task>(RestartCheckForAlarmsTask));
        }

        private IList<SystemAlarm> NotifyAlarmsToUi(Task<IList<SystemAlarm>> task)
        {
            task.Result.ToList().ForEach(alarm => {
                if (alarm.State == AlarmState.Raised && !alarmNotificationsInUi.Any(n => n.AlarmId == alarm.Id && !n.UiAcknowledged)
                        && alarm.Type == AlarmType.Process)
                {
                    //Add Alarm to the notification list
                    alarmNotificationsInUi.Add(new AlarmNotificationInUI { 
                        AlarmId = alarm.Id,
                    });

                    //Send Notification to Ui
                    AlarmNotificationViewModel alarmNotificationViewModel = unityContainer.Resolve<AlarmNotificationViewModel>();
                    alarmNotificationViewModel.SystemAlarm = alarm;
                    Task.Factory.StartNew(new Action(() =>
                    {
                        dialogServiceProvider.ShowDynamicDialogWindow("System Alarm", default,
                            new AlarmNotificationView() { DataContext = alarmNotificationViewModel });
                    }), CancellationToken.None, TaskCreationOptions.None, taskScheduler);
                }
            });

            return task.Result;
        }

        public IList<SystemAlarm> CheckForAlarms(Task<IList<SystemAlarm>> task)
        {
            IList<SystemAlarm> currentSystemAlarms = task.Result;
            foreach (SystemAlarmPolicy systemAlarmPolicy in systemAlarmPoliciesManager.GetAll().Where(p => p.Status == true))
            {
                if (systemAlarmPolicy.PolicyType == PolicyType.Group)
                {
                    Dictionary<string, string> actualData = fieldDevicesCommunicator.ReadFieldPointsInDataUnit<string>(systemAlarmPolicy.DeviceId, systemAlarmPolicy.Parameters.Name);
                    if (actualData.Any(keyValuePair => keyValuePair.Value == "NC"))
                    {
                        //this case will be dealt by SystemFailures Module
                        //alarm condition need not be checked during the failure situation
                        continue;
                    }
                    Dictionary<string, float> liveData = actualData.ToDictionary(keyValuePair => keyValuePair.Key, keyValuePair => Convert.ToSingle(keyValuePair.Value));
                    if (systemAlarmPolicy.Parameters.ParametersType == AlarmParametersType.RatedValueVariations)
                    {
                        float ratedValue = systemAlarmPolicy.Parameters.RatedValue;
                        float variationPercentage = systemAlarmPolicy.Parameters.VariationPercentage;
                        float variationValue = ratedValue * variationPercentage / 100;
                        float aboveLimit = ratedValue + variationValue;
                        float belowLimit = ratedValue - variationValue;

                        if (systemAlarmPolicy.Parameters.VariationType == AlarmParametersVariationType.Above)
                        {
                            if (liveData.Any(dataPoint => dataPoint.Value > aboveLimit))
                            {
                                IEnumerable<string> fieldPointList
                                        = liveData.Where(dataPoint => dataPoint.Value > aboveLimit).Select(dataPoint => dataPoint.Key);
                                CheckExistingAlarmsAndRaiseAlarmIfRequired(currentSystemAlarms, systemAlarmPolicy, fieldPointList);
                            }
                            else
                            {
                                // Live Data Restored to normal conditions
                                ModifyAlarmState(AlarmState.Resolved, systemAlarmPolicyId:systemAlarmPolicy.Id, currentSystemAlarms);
                            }
                        }
                        else if (systemAlarmPolicy.Parameters.VariationType == AlarmParametersVariationType.Below)
                        {
                            if (liveData.Any(dataPoint => dataPoint.Value < belowLimit))
                            {
                                IEnumerable<string> fieldPointList
                                        = liveData.Where(dataPoint => dataPoint.Value < belowLimit).Select(dataPoint => dataPoint.Key);
                                CheckExistingAlarmsAndRaiseAlarmIfRequired(currentSystemAlarms, systemAlarmPolicy, fieldPointList);
                            }
                            else
                            {
                                // Live Data Restored to normal conditions
                                ModifyAlarmState(AlarmState.Resolved, systemAlarmPolicyId: systemAlarmPolicy.Id, currentSystemAlarms);
                            }
                        }
                        else if (systemAlarmPolicy.Parameters.VariationType == AlarmParametersVariationType.Both)
                        {
                            string[] policyTitleArray = systemAlarmPolicy.Title.Split('&');
                            SystemAlarmPolicy systemAlarmPolicyClone = (SystemAlarmPolicy)systemAlarmPolicy.Clone();
                            if (liveData.Any(dataPoint => dataPoint.Value > aboveLimit))
                            {
                                systemAlarmPolicyClone.Title = policyTitleArray[0] + policyTitleArray[2];
                                IEnumerable<string> fieldPointList
                                        = liveData.Where(dataPoint => dataPoint.Value > aboveLimit).Select(dataPoint => dataPoint.Key);
                                CheckExistingAlarmsAndRaiseAlarmIfRequired(currentSystemAlarms, systemAlarmPolicyClone, fieldPointList);
                            }
                            else if (liveData.Any(dataPoint => dataPoint.Value < belowLimit))
                            {
                                systemAlarmPolicyClone.Title = policyTitleArray[1] + policyTitleArray[2];
                                IEnumerable<string> fieldPointList
                                        = liveData.Where(dataPoint => dataPoint.Value < belowLimit).Select(dataPoint => dataPoint.Key);
                                CheckExistingAlarmsAndRaiseAlarmIfRequired(currentSystemAlarms, systemAlarmPolicyClone, fieldPointList);
                            }
                            else if(liveData.All(dataPoint => dataPoint.Value <= aboveLimit && dataPoint.Value >= belowLimit))
                            {
                                // Live Data Restored to normal conditions
                                ModifyAlarmState(AlarmState.Resolved, systemAlarmPolicyId: systemAlarmPolicy.Id, currentSystemAlarms);
                            }
                        }
                    }
                }
                else if(systemAlarmPolicy.PolicyType == PolicyType.Individual)
                {
                    string actualData = fieldDevicesCommunicator.ReadFieldPointValue<string>(systemAlarmPolicy.DeviceId, systemAlarmPolicy.Parameters.Name);
                    if (actualData == "NC")
                    {
                        //this case will be dealt by SystemFailures Module
                        //alarm condition need not be checked during the failure situation
                        continue;
                    }
                    float liveData = Convert.ToSingle(actualData);
                    float ratedValue = systemAlarmPolicy.Parameters.RatedValue;
                    float variationPercentage = systemAlarmPolicy.Parameters.VariationPercentage;
                    float variationValue = ratedValue * variationPercentage / 100;
                    float aboveLimit = (systemAlarmPolicy.Parameters.ParametersType == AlarmParametersType.RatedValueVariations ? ratedValue + variationValue
                        : systemAlarmPolicy.Parameters.UpperLimit);
                    float belowLimit = (systemAlarmPolicy.Parameters.ParametersType == AlarmParametersType.RatedValueVariations ? ratedValue - variationValue
                        : systemAlarmPolicy.Parameters.LowerLimit);
                    if (systemAlarmPolicy.Parameters.VariationType == AlarmParametersVariationType.Above)
                    {
                        if (liveData > aboveLimit)
                        {
                            IEnumerable<string> fieldPointList = new List<string> { systemAlarmPolicy.Parameters.Name };
                            CheckExistingAlarmsAndRaiseAlarmIfRequired(currentSystemAlarms, systemAlarmPolicy, fieldPointList);
                        }
                        else
                        {
                            // Live Data Restored to normal conditions
                            ModifyAlarmState(AlarmState.Resolved, systemAlarmPolicyId: systemAlarmPolicy.Id, currentSystemAlarms);
                        }
                    }
                    else if (systemAlarmPolicy.Parameters.VariationType == AlarmParametersVariationType.Below)
                    {
                        if (liveData < belowLimit)
                        {
                            IEnumerable<string> fieldPointList = new List<string> { systemAlarmPolicy.Parameters.Name };
                            CheckExistingAlarmsAndRaiseAlarmIfRequired(currentSystemAlarms, systemAlarmPolicy, fieldPointList);
                        }
                        else
                        {
                            // Live Data Restored to normal conditions
                            ModifyAlarmState(AlarmState.Resolved, systemAlarmPolicyId: systemAlarmPolicy.Id, currentSystemAlarms);
                        }
                    }
                    else if (systemAlarmPolicy.Parameters.VariationType == AlarmParametersVariationType.Both)
                    {
                        string[] policyTitleArray = systemAlarmPolicy.Title.Split('&');
                        SystemAlarmPolicy systemAlarmPolicyClone = (SystemAlarmPolicy)systemAlarmPolicy.Clone();
                        IEnumerable<string> fieldPointList = new List<string> { systemAlarmPolicy.Parameters.Name };
                        if (liveData > aboveLimit)
                        {
                            systemAlarmPolicyClone.Title = policyTitleArray[0] + policyTitleArray[2];
                            CheckExistingAlarmsAndRaiseAlarmIfRequired(currentSystemAlarms, systemAlarmPolicyClone, fieldPointList);
                        }
                        else if (liveData < belowLimit)
                        {
                            systemAlarmPolicyClone.Title = policyTitleArray[1] + policyTitleArray[2];
                            CheckExistingAlarmsAndRaiseAlarmIfRequired(currentSystemAlarms, systemAlarmPolicyClone, fieldPointList);
                        }
                        else if (liveData <= aboveLimit && liveData >= belowLimit)
                        {
                            // Live Data Restored to normal conditions
                            ModifyAlarmState(AlarmState.Resolved, systemAlarmPolicyId: systemAlarmPolicy.Id, currentSystemAlarms);
                        }
                    }
                }
            }
            return currentSystemAlarms;
        }

        private void ModifyAlarmState(AlarmState changedAlarmState, int systemAlarmPolicyId = 0, IList<SystemAlarm> existingAlarms = null, int alarmId = 0)
        {
            IList<DbParameterInfo> dbParameters = new List<DbParameterInfo>();
            if (alarmId == 0)
            {
                IEnumerable<SystemAlarm> alarmsWithGivenPolicyId = existingAlarms.Where(alarm => alarm.SystemAlarmPolicyId == systemAlarmPolicyId && alarm.State != AlarmState.Resolved);
                if (alarmsWithGivenPolicyId.Count() > 0)
                {
                    //Find the Alarm Id
                    dbParameters.Add(new DbParameterInfo("Id", alarmsWithGivenPolicyId.OrderByDescending(alarm => alarm.TimeStamp).First().Id, DbType.Int32));
                }
            }
            else
            {
                dbParameters.Add(new DbParameterInfo("Id", alarmId, DbType.Int32));
            }
            dbParameters.Add(new DbParameterInfo("AlarmState", changedAlarmState, DbType.String));

            //Execute the DbWrite Command only if Id parameter is added to DbParameters
            if (dbParameters.Any(parameter => parameter.Name == "Id"))
            {
                databaseWriter.ExecuteWriteCommand("ModifyAlarmState", CommandType.StoredProcedure, dbParameters);
            }
        }

        private void CheckExistingAlarmsAndRaiseAlarmIfRequired(IList<SystemAlarm> existingAlarms, SystemAlarmPolicy systemAlarmPolicy, IEnumerable<string> fieldPointList)
        {
            if (existingAlarms.Any(alarm => alarm.SystemAlarmPolicyId == systemAlarmPolicy.Id
                                        && alarm.State != AlarmState.Resolved))
            {
                // Alarm already raised. Skip.
            }
            else
            {
                SaveSystemAlarmToDatabase(systemAlarmPolicy, AlarmType.Process, fieldPointList);
            }
        }

        private void SaveSystemAlarmToDatabase(SystemAlarmPolicy systemAlarmPolicy, AlarmType alarmType, IEnumerable<string> fieldPointList)
        {
            #region Convert FieldPointList to String
            string fieldPointString = string.Empty;
            if (fieldPointList.Count() > 1)
            {
                for (int counter = 0; counter < fieldPointList.Count(); counter++)
                {
                    if (counter == fieldPointList.Count() - 1)
                    {
                        fieldPointString += fieldPointList.ElementAt(counter);
                    }
                    else
                    {
                        fieldPointString = fieldPointString + fieldPointList.ElementAt(counter) + "|";
                    }
                }
            }
            else
            {
                fieldPointString = fieldPointList.ElementAt(0);
            }
            #endregion

            IList<DbParameterInfo> dbParameters = new List<DbParameterInfo>
            {
                new DbParameterInfo("SystemAlarmPolicyId", systemAlarmPolicy.Id, DbType.Int32),
                new DbParameterInfo("SystemAlarmParameterName", systemAlarmPolicy.Parameters.Name, DbType.String),
                new DbParameterInfo("DeviceId", systemAlarmPolicy.DeviceId, DbType.String),
                new DbParameterInfo("DeviceLabel", systemAlarmPolicy.DeviceLabel, DbType.String),
                new DbParameterInfo("FieldPointLabel", fieldPointString, DbType.String),
                new DbParameterInfo("Title", systemAlarmPolicy.Title, DbType.String),
                new DbParameterInfo("Message", systemAlarmPolicy.Message, DbType.String),
                new DbParameterInfo("State", AlarmState.Raised, DbType.String),
                new DbParameterInfo("Type", alarmType, DbType.String),
                new DbParameterInfo("TimeStamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), DbType.String)
            };
            databaseWriter.ExecuteWriteCommand("InsertSystemAlarm", CommandType.StoredProcedure, dbParameters);
        }

        private void SaveSystemAlarmToDatabase(SystemAlarm systemAlarm)
        {
            IList<DbParameterInfo> dbParameters = new List<DbParameterInfo>
            {
                new DbParameterInfo("SystemAlarmPolicyId", systemAlarm.SystemAlarmPolicyId, DbType.Int32),
                new DbParameterInfo("SystemAlarmParameterName", systemAlarm.SystemAlarmParameterName, DbType.String),
                new DbParameterInfo("SystemFailureId", systemAlarm.SystemFailureId, DbType.Int32),
                new DbParameterInfo("DeviceId", systemAlarm.DeviceId, DbType.String),
                new DbParameterInfo("DeviceLabel", systemAlarm.DeviceLabel, DbType.String),
                new DbParameterInfo("FieldPointLabel", systemAlarm.FieldPointLabel, DbType.String),
                new DbParameterInfo("Title", systemAlarm.Title, DbType.String),
                new DbParameterInfo("Message", systemAlarm.Message, DbType.String),
                new DbParameterInfo("State", systemAlarm.State, DbType.String),
                new DbParameterInfo("Type", systemAlarm.Type, DbType.String),
                new DbParameterInfo("TimeStamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), DbType.String)
            };
            databaseWriter.ExecuteWriteCommand("InsertSystemAlarm", CommandType.StoredProcedure, dbParameters);
        }

        private void PublishRefreshSystemAlarmsEvent(IList<SystemAlarm> systemAlarms)
        {
            if (RefreshSystemAlarms == null)
            {
                //No Subscribers
            }
            else
            {
                foreach (Delegate receiver in RefreshSystemAlarms.GetInvocationList())
                {
                    ((RefreshSystemAlarmsEventHandler)receiver).BeginInvoke(systemAlarms, null, null);
                }
            }
        }

        #region Acknowledge & Dismiss Alarm
        public void Acknowledge(int alarmId)
        {
            ModifyAlarmState(AlarmState.Acknowledged, alarmId:alarmId);

            //Update AlarmNotification List
            alarmNotificationsInUi.Where(n => n.AlarmId == alarmId).ToList().ForEach(n => n.UiAcknowledged = true);
        }
        #endregion

        #region Get Alarms
        public IEnumerable<SystemAlarm> GetAll() => systemAlarms;

        public IEnumerable<SystemAlarm> GetSystemAlarmsForDevice(string deviceId)
        {
            IEnumerable<SystemAlarm> alarmsInDevice = new List<SystemAlarm>();
            lock (systemAlarms)
            {
                alarmsInDevice = systemAlarms.Where(alarm => alarm.DeviceId == deviceId);
            }
            return alarmsInDevice;
        }

        public IEnumerable<SystemAlarm> GetAll(string deviceId, DateTime startTime, DateTime endTime)
        {
            IList<DbParameterInfo> dbParameters = new List<DbParameterInfo> {
                new DbParameterInfo("DeviceId", deviceId, DbType.String),
                new DbParameterInfo("StartTime", startTime.ToString("yyyy-MM-dd HH:mm:ss"), DbType.String),
                new DbParameterInfo("EndTime", endTime.ToString("yyyy-MM-dd HH:mm:ss"), DbType.String),
            };
            return (from DataRow row in databaseReader.ExecuteReadCommand("GetAlarmsBetweenTimeStamps", CommandType.StoredProcedure, dbParameters).AsEnumerable()
                    select new SystemAlarm {
                        Id = Convert.ToInt32(row["Id"]),
                        SystemAlarmPolicyId = (string.IsNullOrWhiteSpace(row["SystemAlarmPolicyId"].ToString()) ? 0 : Convert.ToInt32(row["SystemAlarmPolicyId"])),
                        SystemAlarmParameterName = (string.IsNullOrWhiteSpace(row["SystemAlarmParameterName"].ToString()) ? "" : Convert.ToString(row["SystemAlarmParameterName"])),
                        SystemFailureId = (string.IsNullOrWhiteSpace(row["SystemFailureId"].ToString()) ? 0 : Convert.ToInt32(row["SystemFailureId"])),
                        DeviceId = Convert.ToString(row["DeviceId"]),
                        DeviceLabel = Convert.ToString(row["DeviceLabel"]),
                        FieldPointLabel = Convert.ToString(row["FieldPointLabel"]),
                        Title = Convert.ToString(row["Title"]),
                        Message = Convert.ToString(row["Message"]),
                        State = (AlarmState)Enum.Parse(typeof(AlarmState), Convert.ToString(row["State"])),
                        Type = (AlarmType)Enum.Parse(typeof(AlarmType), Convert.ToString(row["Type"])),
                        TimeStamp = Convert.ToDateTime(row["TimeStamp"]),
                        RaisedTimeStamp = row["RaisedTimeStamp"].ToString() == string.Empty ? default : Convert.ToDateTime(row["RaisedTimeStamp"]),
                        AcknowledgedTimeStamp = row["AcknowledgedTimeStamp"].ToString() == string.Empty ? default : Convert.ToDateTime(row["AcknowledgedTimeStamp"])
                    });
        }
        #endregion

        public string DeviceId => "Reactor_1";

    }
}
