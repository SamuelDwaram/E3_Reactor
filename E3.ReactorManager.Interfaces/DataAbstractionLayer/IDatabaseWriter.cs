using System;
using System.Collections.Generic;
using System.Data;
using E3.ReactorManager.Interfaces.DataAbstractionLayer.Data;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;

namespace E3.ReactorManager.Interfaces.DataAbstractionLayer
{
    /// <summary>
    /// Contract for all Database Writes
    /// </summary>
    public interface IDatabaseWriter
    {
        void ExecuteWriteCommand(string commandText, CommandType commandType, IList<DbParameterInfo> parameters = null);

        object GetScalarValue(string commandText, CommandType commandType, IList<DbParameterInfo> parameters = null);

        #region New User Management Functions
        void UpdateUser(string userId, string fieldToBeUpdated, string updatedValue);

        void AddRole(string roleName, IList<string> modulesAccessable);

        void AssignRoleToUser(string userId, string roleName);

        void DeleteRole(string roleName);

        void UpdateRole(string roleName, IList<string> modulesAccessable);
        #endregion

        void LogLiveData(IList<FieldDevice> fieldDevicesData);

        /// <summary>
        /// Logs all the given parameters and their values in the Table with name field device identifier
        /// </summary>
        /// <param name="fieldDeviceIdentifier"> Field Device Id </param>
        /// <param name="parametersData"> Dictionary<ParameterName, ParameterValue> </ParameterName></param>
        void LogLiveData(string fieldDeviceIdentifier, Dictionary<string, string> parametersData);

        /// <summary>
        /// Logs the Alarm and Returns the Alarm Identifier of the Newly Logged Alarm
        /// </summary>
        /// <param name="monitoringDataPoints"></param>
        /// <param name="resolutionMessage"></param>
        /// <param name="alarmType"></param>
        /// <param name="alarmSeverity"></param>
        /// <returns></returns>
        void LogAlarm(byte[] monitoringDataPoints, string resolutionMessage, string alarmType, string alarmSeverity);

        void AcknowledgeAlarm(string alarmIdentifier);

        void DismissAlarm(string alarmIdentifier);

        void EndBatch(string batchIdentifier, string cleanedBy, string cleaningSolvent);

        /// <summary>
        /// Uploads Pdf to database
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="pdfContent"></param>
        void UploadPdf(String fileName, byte[] pdfContent);

        void LogReactorImage(string fieldDeviceIdentifier, byte[] imgBytes);

        void LogReactorImageWithFieldDeviceParameters(string fieldDeviceIdentifier, byte[] imgBytes, Dictionary<string, string> deviceParametersData);
    }
}
