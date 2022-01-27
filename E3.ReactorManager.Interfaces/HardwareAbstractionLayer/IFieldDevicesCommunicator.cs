using E3.ReactorManager.Interfaces.HardwareAbstractionLayer.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace E3.ReactorManager.Interfaces.HardwareAbstractionLayer
{
    /// <summary>
    /// Fields Devices 
    /// </summary>
    public interface IFieldDevicesCommunicator
    {
        /// <summary>
        /// History of Commands Waiting for Acknowledgement
        /// </summary>
        IList<Command> CommandHistory { set; get; }
        
        /// <summary>
        /// Invokes when live data is logged into database
        /// </summary>
        event EventHandler NewDataLoggedIntoDatabase;

        /// <summary>
        /// Event to raise when Field Data Received 
        /// </summary>
        event EventHandler<FieldPointDataReceivedArgs> FieldPointDataReceived;

        /// <summary>
        /// Event to raise when Command Acknowledgement Received 
        /// </summary>
        event EventHandler<CommandAckEventArgs> CommandAcknowledgementReceived;

        /// <summary>
        /// Returns the list of all field device identifiers
        /// </summary>
        /// <returns></returns>
        IList<string> GetAllFieldDeviceIdentifiers();

        /// <summary>
        /// Returns the Identifier of the field device
        /// </summary>
        /// <param name="fieldDeviceLabel"></param>
        /// <returns></returns>
        string GetFieldDeviceIdentifier(string fieldDeviceLabel);

        Dictionary<string, string> GetConnectedController(string deviceId, string fieldPointLabel);

        string GetFieldDeviceType(string deviceId);

        void StartCyclicPollingOfFieldDevices(Action<Task> callback, TaskScheduler taskScheduler);

        void StartDataLogging();

        /// <summary>
        /// Returns the Label of the Field Device
        /// </summary>
        /// <param name="fieldDeviceIdentifier"></param>
        /// <returns></returns>
        string GetFieldDeviceLabel(string fieldDeviceIdentifier);

        FieldDevice GetFieldDeviceData(string fieldDeviceIdentifier);

        Device GetBasicDeviceInfo(string fieldDeviceIdentifier);

        /// <summary>
        /// Call initially to read Sensor Data 
        /// </summary>
        /// <returns></returns>
        IList<FieldDevice> ReadAllSensorsLiveData();

        Task<FieldDevice> ReadRequestedFieldDeviceData(string fieldDeviceIdentifier);

        /// <summary>
        /// Reads the field point value of the field device
        /// </summary>
        /// <param name="fieldDeviceIdentifier"></param>
        /// <param name="fieldPointIdentifier"></param>
        /// <returns></returns>
        dynamic ReadFieldPointValue(string fieldDeviceIdentifier, string fieldPointIdentifier);

        T ReadFieldPointValue<T>(string fieldDeviceIdentifier, string fieldPointIdentifier);

        Dictionary<string, T> ReadFieldPointsInDataUnit<T>(string deviceId, string dataUnit);

        /// <summary>
        /// Gets All Field Devices data
        /// </summary>
        /// <returns></returns>
        IList<FieldDevice> GetAllFieldDevicesData();

        /// <summary>
        /// Send Command To Field Device
        /// </summary>
        /// <param name="fieldDeviceIdentifier"></param>
        /// <param name="fieldPointLabel"></param>
        void SendCommandToDevice(string fieldDeviceIdentifier, string fieldPointLabel, string dataTypeOfCommand, string writeValue);

        void ShareLiveDataToAllModules(FieldPointDataReceivedArgs fieldPointDataReceivedArgs);

        void UpdateFieldDevicesDataForMobileDevicesInitialization(Task<IList<FieldDevice>> task);
        IEnumerable<int> CreateVariableHandles(string deviceId, IEnumerable<string> memoryAddresses);

        void DeleteVariableHandles(string deviceId, IEnumerable<int> variableHandles);

        T ReadAny<T>(string deviceId, int plcHandle);
        void WriteAny<T>(string deviceId, int plcHandle, T data);
    }
}
