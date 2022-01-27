using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using E3.ReactorManager.Interfaces.DataAbstractionLayer;
using E3.ReactorManager.Interfaces.Framework.Logging;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer.Data;
using TwinCAT.Ads;

namespace E3.ReactorManager.HardwareAbstractionLayer
{
    /// <summary>
    /// Filed Devices Wrapper
    /// </summary>
    public class FieldDevicesWrapper
    {
        private readonly IDatabaseReader databaseReader;
        private readonly IDatabaseWriter databaseWriter;
        private readonly ILogger logger;

        public FieldDevicesWrapper(IDatabaseWriter databaseWriter, IDatabaseReader databaseReader, ILogger logger)
        {
            this.databaseWriter = databaseWriter;
            this.databaseReader = databaseReader;
            this.logger = logger;
        }

        public event EventHandler<FieldPointDataReceivedArgs> FieldPointDataReceived;
        public IList<FieldDevice> FieldDevices { get; set; } = new List<FieldDevice>();

        public IEnumerable<int> CreateVariableHandles(string deviceId, IEnumerable<string> memoryAddresses)
        {
            return from FieldDevice fd in FieldDevices.Where(fd => fd.Identifier == deviceId)
                   from string memoryAddress in memoryAddresses
                   select fd.RelatedPlc.CreateVariableHandle(memoryAddress);
        }

        public void DeleteVariableHandles(string deviceId, IEnumerable<int> variableHandles)
        {
            FieldDevices.Where(fd => fd.Identifier == deviceId).ToList().ForEach(fd => {
                variableHandles.ToList().ForEach(handle => {
                    fd.RelatedPlc.DeleteVariableHandle(handle);
                });
            });
        }

        #region Initialize Field Devices
        internal Task<IList<FieldDevice>> Initialize(TaskScheduler taskScheduler)
        {
            var tcs = new TaskCompletionSource<IList<FieldDevice>>();
            CancellationToken token = new CancellationToken();
            var task1 = InitializeFieldDeviceData(token);
            var task2 = task1.ContinueWith(t => ConnectFieldDevices(t.Result, token));
            var task3 = task2.ContinueWith(t => CreateNotificationHandles(t.Result.Result, token, taskScheduler));
            task3.ContinueWith(t => tcs.SetResult(t.Result.Result));
            return tcs.Task;
        }

        /// <summary>
        /// Pass the OnFieldPointDataChangedNotification Receiver method as a callback
        /// to avoid multiple subscription for the same field point value change notification
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<IList<FieldDevice>> InitializeFieldDeviceData(CancellationToken token)
        {
            return Task<IList<FieldDevice>>.Factory.StartNew(new Func<object, IList<FieldDevice>>(databaseReader.FetchFieldDevicesData), new Action<object, AdsNotificationExEventArgs>(OnFieldPointDataChangedNotification), token);
        }

        /// <summary>
        /// Connect to PLC using TwinCAT client
        /// </summary>
        public Task<IList<FieldDevice>> ConnectFieldDevices(IList<FieldDevice> fieldDevices, CancellationToken token)
        {
            return Task<IList<FieldDevice>>.Factory.StartNew(new Func<object, IList<FieldDevice>>(ConnectToFieldDevices), fieldDevices, token);
        }

        private IList<FieldDevice> ConnectToFieldDevices(object fieldDevices)
        {
            //Connect to the PLC's of all field Devices
            foreach (var fieldDevice in (IList<FieldDevice>)fieldDevices)
            {
                try
                {
                    //Connect to the PLC of the Field Device
                    fieldDevice.RelatedPlc.Connect();
                    logger.Log(LogType.Information, "Connection to : " + fieldDevice.RelatedPlc.Address + " successful");

                    foreach (var sensorDataSet in fieldDevice.SensorsData)
                    {
                        foreach (var fieldPoint in sensorDataSet.SensorsFieldPoints)
                        {
                            //Create Variable Handles for field Points in the Field Device
                            try
                            {
                                fieldPoint.PLCHandle = fieldDevice.RelatedPlc.CreateVariableHandle(fieldPoint.MemoryAddress);
                            }
                            catch (Exception variableHanldeCreationFailureException)
                            {
                                logger.Log(LogType.Error, "Failed to Create Variable Handle for " + fieldPoint.MemoryAddress, variableHanldeCreationFailureException);
                                Console.WriteLine("Failed to Create Variable Handle for " + fieldPoint.MemoryAddress);
                                Console.WriteLine(variableHanldeCreationFailureException.Message);
                            }
                        }
                    }
                }
                catch (Exception connectionFailureException)
                {
                    logger.Log(LogType.Fatal, "Connection to Plc Failed", connectionFailureException);
                }
            }
            return (IList<FieldDevice>)fieldDevices;
        }

        private Task<IList<FieldDevice>> CreateNotificationHandles(IList<FieldDevice> fieldDevices, CancellationToken token, TaskScheduler taskScheduler)
        {
            return Task<IList<FieldDevice>>.Factory.StartNew(new Func<object, IList<FieldDevice>>(CreateNotificationHandlesSync), fieldDevices, token, TaskCreationOptions.LongRunning, taskScheduler);
        }

        private IList<FieldDevice> CreateNotificationHandlesSync(object fieldDevices)
        {
            FieldDevices = (IList<FieldDevice>)fieldDevices;
            foreach (var fieldDevice in FieldDevices)
            {
                foreach (var sensorDataSet in fieldDevice.SensorsData)
                {
                    foreach (var fieldPoint in sensorDataSet.SensorsFieldPoints)
                    {
                        if (fieldPoint.RequireNotificationService)
                        {
                            var fieldPointData = new FieldPointDataReceivedArgs
                            {
                                FieldDeviceIdentifier = fieldDevice.Identifier,
                                FieldPointIdentifier = fieldPoint.Label,
                                SensorDataSetIdentifier = fieldPoint.SensorDataSetIdentifier,
                                FieldPointDataType = fieldPoint.FieldPointDataType,
                                FieldPointType = fieldPoint.TypeOfAddress,
                                FieldPointDescription = fieldPoint.Description,
                            };
                            try
                            {
                                //Creating Notification Handles for the Field Points
                                if (fieldPoint.FieldPointDataType != "string")
                                {
                                    fieldPoint.NotificationHandle
                                                = fieldDevice.RelatedPlc
                                                        .TwinCATClient.AddDeviceNotificationEx(fieldPoint.MemoryAddress, AdsTransMode.OnChange, 100, 0,
                                                                                                    fieldPointData, GetFieldPointDataType(fieldPoint.FieldPointDataType));
                                }
                                else if (fieldPoint.FieldPointDataType.Equals("string"))
                                {
                                    fieldPoint.NotificationHandle
                                                = fieldDevice.RelatedPlc
                                                        .TwinCATClient.AddDeviceNotificationEx(fieldPoint.MemoryAddress, AdsTransMode.OnChange, 500, 0,
                                                                                                    fieldPointData, GetFieldPointDataType(fieldPoint.FieldPointDataType), new int[] { 80 });
                                }
                            }
                            catch (Exception notificationHandleCreationException)
                            {
                                logger.Log(LogType.Error, "Failed to Create Notification Handle for " + fieldPoint.MemoryAddress, notificationHandleCreationException);
                                Console.WriteLine("Failed to Create Notification Handle for " + fieldPoint.MemoryAddress);
                                Console.WriteLine(notificationHandleCreationException.Message);
                            }
                        }
                    }
                }
            }
            return FieldDevices;
        }

        #endregion

        #region Notification FieldPoint Data Readers from Plc

        /// <summary>
        /// Receives the Notification event(when value of variable changes) 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="adsNotificationEventArgs"></param>
        private void OnFieldPointDataChangedNotification(object sender, AdsNotificationExEventArgs adsNotificationExEventArgs)
        {
            var changedFieldPointData = (FieldPointDataReceivedArgs)adsNotificationExEventArgs.UserData;

            changedFieldPointData.NewFieldPointData
                        = adsNotificationExEventArgs.Value.ToString();

            Task.Factory.StartNew(new Func<object, FieldPointDataReceivedArgs>(UpdateChangedDataWithLiveData), changedFieldPointData)
                .ContinueWith(new Action<Task<FieldPointDataReceivedArgs>>(NotifyLiveDataToAllOtherModules));
        }

        private void NotifyLiveDataToAllOtherModules(Task<FieldPointDataReceivedArgs> obj)
        {
            var liveData = obj.Result;

            //Raise FieldPointDataReceived event to notify live data to other modules
            if (FieldPointDataReceived != null)
            {
                var receivers = FieldPointDataReceived.GetInvocationList();
                foreach (var receiver in receivers)
                {
                    ((EventHandler<FieldPointDataReceivedArgs>)receiver).BeginInvoke(this, liveData, null, null);
                }
            }
        }

        private FieldPointDataReceivedArgs UpdateChangedDataWithLiveData(object arg)
        {
            var liveData = arg as FieldPointDataReceivedArgs;

            foreach (var fieldDevice in FieldDevices)
            {
                if (fieldDevice.Identifier.Equals(liveData.FieldDeviceIdentifier))
                {
                    foreach (var sensorDataSet in fieldDevice.SensorsData)
                    {
                        foreach (var fieldPoint in sensorDataSet.SensorsFieldPoints)
                        {
                            if (fieldPoint.Label.Equals(liveData.FieldPointIdentifier))
                            {
                                fieldPoint.Value = liveData.NewFieldPointData;
                            }
                        }
                    }
                }
            }

            return liveData;
        }

        /// <summary>
        /// update only changed fieldpoint data
        /// </summary>
        /// <param name="fieldPointEventArgs"></param>
        public void CheckAndReadChangedSensorsData(FieldPointDataReceivedArgs fieldPointEventArgs)
        {
            //update only changed fieldpoint data
            foreach (var fieldDevice in FieldDevices)
            {
                if (fieldDevice.Identifier.Equals(fieldPointEventArgs.FieldDeviceIdentifier))
                {
                    foreach (var sensorDataSet in fieldDevice.SensorsData)
                    {
                        if (sensorDataSet.Identifier == fieldPointEventArgs.SensorDataSetIdentifier)
                        {
                            sensorDataSet.SensorsFieldPoints
                                .Where(fieldPoint => fieldPoint.Label == fieldPointEventArgs.FieldPointIdentifier).ToList()
                                .ForEach(item => item.Value = fieldPointEventArgs.NewFieldPointData);

                            return;
                        }
                    }
                }
            }
        }

        #endregion

        #region Non Notification FieldPoint Data Readers from Plc
        /// <summary>
        /// Read all field Devices
        /// </summary>
        /// <returns></returns>
        public IList<FieldDevice> ReadAllFieldDevicesData()
        {
            foreach (var fieldDevice in FieldDevices)
            {
                /*
                 * Read all the field points in the field device
                 * only if the connection between the plc of the field device exists
                 */
                if (!fieldDevice.RelatedPlc.TwinCATClient.Disposed)
                {
                    foreach (var sensorsDataSet in fieldDevice.SensorsData)
                    {
                        foreach (var fieldPoint in sensorsDataSet.SensorsFieldPoints)
                        {
                            // Read data of the field point only
                            // if it does not subscribes to NotificationService in plc
                            if (!fieldPoint.RequireNotificationService)
                            {
                                string fieldPointValue;
                                Type fieldPointDataType = GetFieldPointDataType(fieldPoint.FieldPointDataType);

                                if (fieldPoint.FieldPointDataType.Equals("string"))
                                {
                                    fieldPointValue = fieldDevice.RelatedPlc.ReadString(fieldPoint.PLCHandle);
                                }
                                else
                                {
                                    fieldPointValue = fieldDevice.RelatedPlc.Read(fieldPoint.PLCHandle, fieldPointDataType);
                                }
                                /*
                                 * Raise FieldPointDataReceived event if the
                                 * latest read field point value is different from old field point value
                                 */
                                if (fieldPointValue != fieldPoint.Value
                                        && fieldPointValue != null)
                                {
                                    fieldPoint.Value = fieldPointValue;

                                    /* update the value changed boolean bit */
                                    fieldPoint.ValueChanged = true;
                                }
                            }
                        }
                    }
                }
            }

            return FieldDevices;
        }

        #endregion

        #region FieldDevice and FieldPoint Data Providers to Other Modules
        /// <summary>
        /// Read particular field Device data
        /// </summary>
        /// <returns></returns>
        public async Task<FieldDevice> ReadRequestedFieldDeviceData(string requestedFieldDeviceIdentifier)
        {
            FieldDevice toReturnFieldDevice = new FieldDevice();

            foreach (var fieldDevice in FieldDevices)
            {
                if (requestedFieldDeviceIdentifier.Equals(fieldDevice.Identifier))
                {
                    toReturnFieldDevice = fieldDevice;
                }
            }

            await Task.Yield();

            return toReturnFieldDevice;
        }

        /// <summary>
        /// Returns all FieldDevicesRunning Status
        /// </summary>
        /// <returns></returns>
        public IList<FieldDevice> GetAllFieldDevicesData()
        {
            return FieldDevices;
        }

        public dynamic ReadFieldPointValue(string fieldDeviceIdentifier, string fieldPointLabel)
        {
            foreach (var fieldDevice in FieldDevices)
            {
                if (fieldDevice.Identifier.Equals(fieldDeviceIdentifier))
                {
                    foreach (var sensorsDataSet in fieldDevice.SensorsData)
                    {
                        foreach (var fieldPoint in sensorsDataSet.SensorsFieldPoints)
                        {
                            if (fieldPoint.Label.Equals(fieldPointLabel))
                            {
                                Type fieldPointDataType = GetFieldPointDataType(fieldPoint.FieldPointDataType);
                                return TryParse(fieldPointDataType, fieldPoint.Value);
                            }
                        }
                    }
                }
            }

            return null;
        }

        public T ReadFieldPointValue<T>(string fieldDeviceIdentifier, string fieldPointIdentifier)
        {
            var fieldDevice = new List<FieldDevice>(FieldDevices).Find(device => device.Identifier == fieldDeviceIdentifier);

            foreach (var sensorsDataSet in fieldDevice.SensorsData)
            {
                var fieldPoint = new List<FieldPoint>(sensorsDataSet.SensorsFieldPoints).Find(obj => obj.Label == fieldPointIdentifier);
                if (fieldPoint != null)
                {
                    return TConverter.ChangeType<T>(fieldPoint.Value);
                }
            }

            return default;
        }

        #endregion

        #region Command Senders to Plc
        /// <summary>
        /// Send Command to Device
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<bool> SendCommandToDevice(Command command)
        {
            foreach (var fieldDevice in FieldDevices)
            {
                if (fieldDevice.Identifier.Equals(command.FieldDeviceIdentifier))
                {
                    foreach (var fieldPoint in fieldDevice.CommandPoints)
                    {
                        if (fieldPoint.Label.Equals(command.FieldPointIdentifier))
                        {
                            //Write to PLC
                            if (command.DataType.Equals("string"))
                            {
                                return fieldDevice.RelatedPlc.WriteString(fieldPoint.PLCHandle, TryParse(GetFieldPointDataType(command.DataType), command.WriteValue));
                            }
                            else
                            {
                                return fieldDevice.RelatedPlc.Write(fieldPoint.PLCHandle, TryParse(GetFieldPointDataType(command.DataType), command.WriteValue));
                            }
                        }
                    }
                }
            }

            await Task.Yield();

            return false;
        }

        internal T ReadAny<T>(string deviceId, int plcHandle)
        {
            return (T)FieldDevices.First(fd => fd.Identifier == deviceId).RelatedPlc.TwinCATClient.ReadAny(plcHandle, typeof(T));
        }

        internal void WriteAny<T>(string deviceId, int plcHandle, T data)
        {
            FieldDevices.First(fd => fd.Identifier == deviceId).RelatedPlc.TwinCATClient.WriteAny(plcHandle, data);
        }
        #endregion

        #region Data parsing Functions
        public dynamic TryParse(Type fieldPointDataType, string fieldPointValue)
        {
            switch (Type.GetTypeCode(fieldPointDataType))
            {
                case TypeCode.Int32:
                    return Convert.ToInt32(fieldPointValue);
                case TypeCode.String:
                    return fieldPointValue;
                case TypeCode.Boolean:
                    return bool.Parse(fieldPointValue);
                case TypeCode.Double:
                    return double.Parse(fieldPointValue);
                case TypeCode.Single:
                    return float.Parse(fieldPointValue);
            }
            return null;
        }

        /// <summary>
        /// Returns the fieldPointDataType for
        ///     reading its value
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public Type GetFieldPointDataType(string dataType)
        {
            switch (dataType)
            {
                case "bool":
                    return typeof(bool);
                case "int":
                    return typeof(int);
                case "string":
                    return typeof(string);
                case "float":
                    return typeof(float);
                case "double":
                    return typeof(double);
            }
            return typeof(Nullable);
        }
        #endregion

        #region Close All Field Device Connections to Plc
        /// <summary>
        /// Close all fieldDevice connections on closing the application
        /// </summary>
        public async void CloseAllFieldDeviceConnections()
        {
            foreach (var fieldDevice in FieldDevices)
            {
                fieldDevice.RelatedPlc.Disconnect();
            }

            await Task.Yield();
        }
        #endregion
    }
}
