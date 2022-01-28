using System;
using System.Collections.Generic;
using System.Data;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;
using E3.ReactorManager.Interfaces.DataAbstractionLayer.Data;

namespace E3.ReactorManager.Interfaces.DataAbstractionLayer
{
    /// <summary>
    /// Data base Read
    /// </summary>
    public interface IDatabaseReader
    {
        DataTable ExecuteReadCommand(string commandText, CommandType commandType, IList<DbParameterInfo> parameters = null);

        #region New Field devices functions

        DataTable GetFieldDevicesList();

        DataTable GetSensorsDataSetInFieldDevice(string fieldDeviceIdentifier);

        DataTable GetFieldPointsInSensorsDataSet(string sensorsDataSetIdentifier);

        DataTable GetControllersConnectedToFieldDevice(string controllerIdentifier);

        DataTable GetControllerInfo(string controllerIdentifier);

        Dictionary<string, string> GetAvailableFieldDevices();

        IList<string> GetAvailableFieldPoints(string fieldDeviceIdentifier);
        #endregion

        #region New Batch Info Extractor Functions
        /// <summary>
        /// Returns all the completed batches
        /// </summary>
        /// <returns></returns>
        DataTable GetCompletedBatches();

        /// <summary>
        /// Returns batch info using batch identifier
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        DataTable GetBatchInfo(string batchId);

        /// <summary>
        /// Returns all the running batches in the plant
        /// </summary>
        /// <returns></returns>
        DataTable GetAllRunningBatchesInThePlant();

        /// <summary>
        /// Returns info of batch running in FieldDevice using device Identifier
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        DataTable GetBatchRunningInDevice(string deviceId);
        #endregion

        #region New User Management Functions
        DataTable GetAllUsersInTheDatabase();

        DataTable AuthenticateCredentials(string username, string password);

        DataTable GetAllRoles();

        DataTable GetAccessibleModulesByRole(string roleName);

        DataTable GetAssignedRolesOfUser(string userId);

        #endregion

        /// <summary>
        /// Reads the logged field device data
        /// </summary>
        /// <param name="fieldDeviceIdentifier"></param>
        /// <param name="selectedFieldDeviceParameters"></param>
        /// <returns></returns>
        DataTable ReadFieldDeviceData(string fieldDeviceIdentifier, IList<string> selectedFieldDeviceParameters, DateTime startTime, DateTime endTime);
        
        IList<FieldDevice> FetchFieldDevicesData(object callBack);

        /// <summary>
        /// Gets the Sensors Data set for the field device
        /// </summary>
        /// <param name="fieldDeviceIdentifier"></param>
        /// <returns></returns>
        IList<SensorsDataSet> FetchSensorDataSets(string fieldDeviceIdentifier);

        /// <summary>
        /// Gets the field points for the Sensors Data Set
        /// </summary>
        /// <param name="sensorDataSetIdentifier"></param>
        /// <returns></returns>
        IList<FieldPoint> FetchFieldPoints(string sensorDataSetIdentifier);

        /// <summary>
        /// Gets the pdf details with the given identifier
        /// </summary>
        /// <param name="identifier"></param>
        DataTable GetDocumentDetails(string identifier);

        /// <summary>
        /// Gets the list of pdf's available in the database
        /// </summary>
        /// <returns></returns>
        DataTable GetAvailablePdfList();
        string FetchRunningBatch();
    }
}
