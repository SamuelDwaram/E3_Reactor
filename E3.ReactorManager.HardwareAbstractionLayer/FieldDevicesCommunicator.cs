using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using E3.ReactorManager.Interfaces.DataAbstractionLayer;
using E3.ReactorManager.Interfaces.Framework.Logging;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer.Data;
using Timer = System.Timers.Timer;

namespace E3.ReactorManager.HardwareAbstractionLayer
{
    public class FieldDevicesCommunicator : IFieldDevicesCommunicator
    {
        private readonly FieldDevicesWrapper fieldDeviceWrapper;
        private readonly ILogger logger;
        private readonly Timer timer;
        private readonly Timer cyclicPollDevicesTimer = new Timer(TimeSpan.FromMilliseconds(500).TotalMilliseconds);
        private readonly IDatabaseWriter databaseWriter;
        private readonly IDatabaseReader databaseReader;
        private bool cyclicPollInProgress = false;

        public FieldDevicesCommunicator(IDatabaseWriter databaseWriter, IDatabaseReader databaseReader, ILogger logger)
        {
            this.databaseWriter = databaseWriter;
            this.databaseReader = databaseReader;
            this.logger = logger;
            timer = new Timer(GetLoggingIntervalDetails().TotalMilliseconds);
            timer.Elapsed += Timer_Tick;
            cyclicPollDevicesTimer.Elapsed += CyclicPollDevicesTimer_Tick;
            fieldDeviceWrapper = new FieldDevicesWrapper(databaseWriter, databaseReader, logger);
            fieldDeviceWrapper.FieldPointDataReceived += OnFieldPointDataReceived;
        }

        public IEnumerable<int> CreateVariableHandles(string deviceId, IEnumerable<string> memoryAddresses) => fieldDeviceWrapper.CreateVariableHandles(deviceId, memoryAddresses);

        public void DeleteVariableHandles(string deviceId, IEnumerable<int> variableHandles) => fieldDeviceWrapper.DeleteVariableHandles(deviceId, variableHandles);

        #region Cyclic Poll field devices
        public void StartCyclicPollingOfFieldDevices(Action<Task> callback, TaskScheduler taskScheduler)
        {
            logger.Log(LogType.Information, "Starting Initialization of field devices");

            var task1 = fieldDeviceWrapper.Initialize(taskScheduler);
            task1.ContinueWith((t) =>
            {
                cyclicPollDevicesTimer.Start();
            }).ContinueWith((t) => StartDataLogging()).ContinueWith(callback, taskScheduler);
        }

        private void CyclicPollDevicesTimer_Tick(object sender, EventArgs e)
        {
            if (cyclicPollInProgress)
            {
                return;
            }
            else
            {
                cyclicPollInProgress = true;
                //Turn off cyclicPollInProgress after the task is completed
                Task<IList<FieldDevice>>.Factory.StartNew(new Func<IList<FieldDevice>>(ReadAllSensorsLiveData))
                    .ContinueWith(new Action<Task<IList<FieldDevice>>>(UpdateFieldDevicesWithLiveData))
                    .ContinueWith(t => cyclicPollInProgress = false);
            }
        }

        private void UpdateFieldDevicesWithLiveData(Task<IList<FieldDevice>> task)
        {
            foreach (var fieldDevice in task.Result)
            {
                foreach (var sensorsDataSet in fieldDevice.SensorsData)
                {
                    foreach (var fieldPoint in sensorsDataSet.SensorsFieldPoints)
                    {
                        if (fieldPoint.ValueChanged)
                        {
                            var fieldPointDataReceivedArgs = new FieldPointDataReceivedArgs
                            {
                                FieldDeviceIdentifier = fieldDevice.Identifier,
                                FieldPointIdentifier = fieldPoint.Label,
                                SensorDataSetIdentifier = fieldPoint.SensorDataSetIdentifier,
                                FieldPointDataType = fieldPoint.FieldPointDataType,
                                FieldPointType = fieldPoint.TypeOfAddress,
                                FieldPointDescription = fieldPoint.Description,
                                NewFieldPointData = fieldPoint.Value,
                            };

                            //Raise FieldPointDataReceived event to notify latest data to other modules
                            if (FieldPointDataReceived != null)
                            {
                                var receivers = FieldPointDataReceived.GetInvocationList();
                                foreach (var receiver in receivers)
                                {
                                    ((EventHandler<FieldPointDataReceivedArgs>)receiver).BeginInvoke(this, fieldPointDataReceivedArgs, null, null);
                                }
                            }

                            /* Update ValueChanged boolean bit after updating the UI with latest fieldPoint value */
                            fieldPoint.ValueChanged = false;
                        }
                    }
                }
            }
        }
        #endregion

        #region Data Logging

        public void StartDataLogging() => timer.Start();

        private void Timer_Tick(object sender, EventArgs e)
        {
            Task.Factory.StartNew(new Action(LogLiveData))
                .ContinueWith(new Action<Task>(NotifyDataLoggedEventToAllModules));
        }

        private void LogLiveData()
        {
            foreach (FieldDevice fieldDevice in fieldDeviceWrapper.FieldDevices)
            {
                try
                {
                    string queryPart_1 = $"insert into dbo.{fieldDevice.Identifier}(";
                    string queryPart_2 = $"values(";
                    Dictionary<string, string> fpList = new Dictionary<string, string>();
                    foreach (SensorsDataSet sensorDataSet in fieldDevice.SensorsData)
                    {
                        foreach (FieldPoint fieldPoint in sensorDataSet.SensorsFieldPoints.Where(fp => fp.ToBeLogged))
                        {
                            var fpParsedValue = fieldDeviceWrapper.TryParse(fieldDeviceWrapper.GetFieldPointDataType(fieldPoint.FieldPointDataType),
                                (fieldPoint.Value.Contains("NC") || string.IsNullOrWhiteSpace(fieldPoint.Value)) ? "0" : fieldPoint.Value);
                            fpList.Add(fieldPoint.Label, fieldPoint.FieldPointDataType.Contains("bool") ? Convert.ToInt32(fpParsedValue).ToString() : fpParsedValue.ToString());
                        }
                    }

                    if (fpList.Count > 0)
                    {
                        queryPart_1 = queryPart_1 + string.Join(",", fpList.Keys) + ", TimeStamp)";
                        queryPart_2 = queryPart_2 + string.Join(",", fpList.Values) + $", '{DateTime.Now:yyyy-MM-dd HH:mm:ss}')";

                        databaseWriter.ExecuteWriteCommand($"{queryPart_1} {queryPart_2}", CommandType.Text);
                        logger.Log(LogType.Information, $"Data Logged for {fieldDevice.Label}");
                    }
                    else
                    {
                        logger.Log(LogType.Information, $"No parameters available to Log Data for {fieldDevice.Label}");
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(LogType.Error, $"Data Logging failed for {fieldDevice.Label}", ex);
                }
            }
        }

        private void NotifyDataLoggedEventToAllModules(Task task) => NewDataLoggedIntoDatabase?.Invoke(this, EventArgs.Empty);

        public TimeSpan GetLoggingIntervalDetails()
        {
            var intervalType = ConfigurationManager.AppSettings["LoggingIntervalType"];
            var timeInterval = ConfigurationManager.AppSettings["LoggingTimeInterval"];
            switch (intervalType)
            {
                case "Hours":
                    return TimeSpan.FromHours(int.Parse(timeInterval));
                case "Minutes":
                    return TimeSpan.FromMinutes(int.Parse(timeInterval));
                case "Seconds":
                    return TimeSpan.FromSeconds(int.Parse(timeInterval));
                case "MilliSeconds":
                    return TimeSpan.FromMilliseconds(int.Parse(timeInterval));
                case "Ticks":
                    return TimeSpan.FromTicks(int.Parse(timeInterval));
                default:
                    break;
            }
            return TimeSpan.FromMinutes(5);
        }
        #endregion

        #region Properties
        /// <summary>
        /// commands to execute
        /// </summary>
        private IList<Command> _commandHistory;
        public IList<Command> CommandHistory
        {
            get => _commandHistory ?? (_commandHistory = new List<Command>());
            set => _commandHistory = value;
        }
        #endregion

        #region Events
        public event EventHandler<FieldPointDataReceivedArgs> FieldPointDataReceived;
        public event EventHandler<CommandAckEventArgs> CommandAcknowledgementReceived;
        public event EventHandler NewDataLoggedIntoDatabase;
        #endregion

        #region FieldDevice and FieldPoint Data providers to other modules
        public string GetFieldDeviceIdentifier(string fieldDeviceLabel)
        {
            foreach (var fieldDevice in fieldDeviceWrapper.FieldDevices)
            {
                if (fieldDevice.Label == fieldDeviceLabel)
                {
                    return fieldDevice.Identifier;
                }
            }

            return null;
        }

        public string GetFieldDeviceLabel(string fieldDeviceIdentifier)
        {
            foreach (var fieldDevice in fieldDeviceWrapper.FieldDevices)
            {
                if (fieldDevice.Identifier == fieldDeviceIdentifier)
                {
                    return fieldDevice.Label;
                }
            }

            return null;
        }
        /// <summary>
        /// Read all the fieldPoint values 
        ///     initially at the start of program from the PLC
        /// </summary>
        public IList<FieldDevice> ReadAllSensorsLiveData()
        {
            IList<FieldDevice> fieldDevicesData = new List<FieldDevice>();

            fieldDevicesData =
                         fieldDeviceWrapper.ReadAllFieldDevicesData();

            return fieldDevicesData;
        }

        /// <summary>
        /// Gets the requested field device data from field device data
        /// </summary>
        /// <param name="fieldDeviceIdentifier"></param>
        public async Task<FieldDevice> ReadRequestedFieldDeviceData(string fieldDeviceIdentifier)
        {

            FieldDevice requestedFieldDeviceData = new FieldDevice();

            requestedFieldDeviceData
                = await fieldDeviceWrapper.ReadRequestedFieldDeviceData(fieldDeviceIdentifier);

            return requestedFieldDeviceData;
        }
        public dynamic ReadFieldPointValue(string fieldDeviceIdentifier, string fieldPointIdentifier)
        {
            return fieldDeviceWrapper.ReadFieldPointValue(fieldDeviceIdentifier, fieldPointIdentifier);
        }
        public IList<FieldDevice> GetAllFieldDevicesData()
        {
            return fieldDeviceWrapper.GetAllFieldDevicesData();
        }

        public IList<string> GetAllFieldDeviceIdentifiers()
        {
            var identifierList = new List<string>();
            foreach (var fieldDevice in fieldDeviceWrapper.FieldDevices)
            {
                if (fieldDevice.Type.Equals("Reactor"))
                {
                    identifierList.Add(fieldDevice.Identifier);
                }
            }
            return identifierList;
        }

        public T ReadFieldPointValue<T>(string fieldDeviceIdentifier, string fieldPointIdentifier)
        {
            return fieldDeviceWrapper.ReadFieldPointValue<T>(fieldDeviceIdentifier, fieldPointIdentifier);
        }

        public FieldDevice GetFieldDeviceData(string fieldDeviceIdentifier)
        {
            return fieldDeviceWrapper.FieldDevices.FirstOrDefault(s => s.Identifier == fieldDeviceIdentifier);
        }
        #endregion

        #region Command Senders to Plc
        /// <summary>
        /// Check and send command to Device
        /// </summary>
        /// <param name="fieldDeviceIdentifier"></param>
        /// <param name="fieldPoint"></param>
        public async void SendCommandToDevice(string fieldDeviceIdentifier, string fieldPointLabel, string dataTypeOfCommand, string writeValue)
        {
            try
            {
                //check if the command already invoked
                if (CommandHistory.Any(
                    x => x.FieldDeviceIdentifier.Equals(fieldDeviceIdentifier)
                         && x.FieldPointIdentifier.Equals(fieldPointLabel))) return;

                var command = new Command
                {
                    DataType = dataTypeOfCommand,
                    WriteValue = writeValue,
                    FieldDeviceIdentifier = fieldDeviceIdentifier,
                    FieldPointIdentifier = fieldPointLabel,
                    SentTime = DateTime.Now,
                    CommandInProgress = true
                };

                CommandHistory.Add(command);

                var status = await fieldDeviceWrapper.SendCommandToDevice(command);

                //Update Command Progress state
                command.CommandInProgress = false;

                var commandAckEventArgs = new CommandAckEventArgs
                {
                    ExecutionStatus = status,
                    WriteValue = command.WriteValue,
                    FieldDeviceIdentifier = command.FieldDeviceIdentifier,
                    FieldPointIdentifier = command.FieldPointIdentifier
                };

                //Remove command from Command History
                CommandHistory.Remove(command);
                CommandAcknowledgementReceived?.Invoke(this, commandAckEventArgs);
            }
            catch (Exception sendCommandFailedException)
            {
                Console.WriteLine("Error in SendCommandToFieldDevice");
                Console.WriteLine("Error message : " + sendCommandFailedException.Message);
                Console.WriteLine("Error stacktrace : " + sendCommandFailedException.StackTrace);

                logger.Log(LogType.Error, "Failed to Send Command With Value "+ writeValue +" to " + fieldPointLabel + " in " + fieldDeviceIdentifier, sendCommandFailedException);
            }
        }

        #endregion

        #region Close Connections to Plc
        public async Task CloseFieldDevicesCommunication()
        {
            fieldDeviceWrapper.CloseAllFieldDeviceConnections();

            await Task.Yield();
        }

        /// <summary>
        /// the following function executes when application is closed
        /// </summary>
        public async Task CloseAllConnections()
        {
            try
            {
                logger.Log(LogType.Information, "Closing Field Devices Communication");

                // Terminate all the field devices communication
                await CloseFieldDevicesCommunication();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in closing all connections");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                logger.Log(LogType.Error, "Error in closing all connections", ex);
            }

            await Task.Yield();

        }
        #endregion

        #region Live Data Sharing to All modules functions
        /// <summary>
        /// Receives the Field PointDataReceived event here
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="fieldPointEventArgs"></param>
        private void OnFieldPointDataReceived(object sender, FieldPointDataReceivedArgs fieldPointEventArgs)
        {
            //Raise FieldPointDataReceived event to notify live data to other modules
            if (FieldPointDataReceived != null)
            {
                var receivers = FieldPointDataReceived.GetInvocationList();
                foreach (var receiver in receivers)
                {
                    ((EventHandler<FieldPointDataReceivedArgs>)receiver).BeginInvoke(this, fieldPointEventArgs, null, null);
                }
            }
        }

        public void ShareLiveDataToAllModules(FieldPointDataReceivedArgs fieldPointDataReceivedArgs)
        {
            //update the FieldDevices variable in FieldDevicesWrapper
            fieldDeviceWrapper.CheckAndReadChangedSensorsData(fieldPointDataReceivedArgs);

            //Raise FieldPointDataReceived event to notify live data to other modules
            if (FieldPointDataReceived != null)
            {
                var receivers = FieldPointDataReceived.GetInvocationList();
                foreach (var receiver in receivers)
                {
                    ((EventHandler<FieldPointDataReceivedArgs>)receiver).BeginInvoke(this, fieldPointDataReceivedArgs, null, null);
                }
            }
        }

        #endregion

        public void UpdateFieldDevicesDataForMobileDevicesInitialization(Task<IList<FieldDevice>> task)
        {
            if (task.IsCompleted)
            {
                fieldDeviceWrapper.FieldDevices = task.Result;
            }
            else
            {
                if (task.IsFaulted)
                {
                    // display error to user
                }
            }
        }

        public Dictionary<string, T> ReadFieldPointsInDataUnit<T>(string deviceId, string dataUnit)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> GetConnectedController(string deviceId, string fieldPointLabel)
        {
            throw new NotImplementedException();
        }

        public string GetFieldDeviceType(string deviceId)
        {
            throw new NotImplementedException();
        }

        public Device GetBasicDeviceInfo(string fieldDeviceIdentifier)
        {
            FieldDevice fieldDevice = fieldDeviceWrapper.FieldDevices.First(d => d.Identifier == fieldDeviceIdentifier);
            return new Device
            {
                Id = fieldDevice.Identifier,
                Label = fieldDevice.Label,
                Type = fieldDevice.Type,
            };
        }

        public T ReadAny<T>(string deviceId, int plcHandle)
        {
            return fieldDeviceWrapper.ReadAny<T>(deviceId, plcHandle);
        }

        public void WriteAny<T>(string deviceId, int plcHandle, T data) => fieldDeviceWrapper.WriteAny<T>(deviceId, plcHandle, data);
    }
}
