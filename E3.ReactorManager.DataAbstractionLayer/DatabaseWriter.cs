using E3.ReactorManager.DataAccessHandler;
using E3.ReactorManager.Interfaces.DataAbstractionLayer;
using E3.ReactorManager.Interfaces.DataAbstractionLayer.Data;
using E3.ReactorManager.Interfaces.Framework.Logging;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Unity;

namespace E3.ReactorManager.DataAbstractionLayer
{
    public class DatabaseWriter : IDatabaseWriter
    {
        private readonly DBManager _dbManager;
        private readonly ILogger logger;

        public DatabaseWriter(IUnityContainer containerProvider)
        {
            logger = containerProvider.Resolve<ILogger>();
            _dbManager = new DBManager("DBconnection");
        }

        public void ExecuteWriteCommand(string commandText, CommandType commandType, IList<DbParameterInfo> parametersList = null)
        {
            IList<IDbDataParameter> dbParameters = new List<IDbDataParameter>();
            if (parametersList != null)
            {
                foreach (DbParameterInfo parameterInfo in parametersList)
                {
                    dbParameters.Add(_dbManager.CreateParameter(parameterInfo.Name, parameterInfo.Value, parameterInfo.DbType));
                }
            }
            //invoke insert command of the DBmanager
            _dbManager.Insert(commandText, commandType, dbParameters.ToArray(), out int lastId);
            Console.WriteLine("Executed : " + commandText);
        }

        public object GetScalarValue(string commandText, CommandType commandType, IList<DbParameterInfo> parametersList = null)
        {
            IList<IDbDataParameter> dbParameters = new List<IDbDataParameter>();
            if (parametersList != null)
            {
                foreach (DbParameterInfo parameterInfo in parametersList)
                {
                    dbParameters.Add(_dbManager.CreateParameter(parameterInfo.Name, parameterInfo.Value, parameterInfo.DbType));
                }
            }

            //invoke GetScalarValue command of the DBmanager
            return _dbManager.GetScalarValue(commandText, CommandType.StoredProcedure, dbParameters.ToArray());
        }

        public void LogLiveData(IList<FieldDevice> fieldDevicesData)
        {
            foreach (var fieldDevice in fieldDevicesData)
            {
                var parameters = new List<IDbDataParameter>();
                
                foreach (var sensorDataSet in fieldDevice.SensorsData)
                {
                    foreach (var fieldPoint in sensorDataSet.SensorsFieldPoints)
                    {
                        if (fieldPoint.ToBeLogged)
                        {
                            parameters.Add(_dbManager.CreateParameter("@" + fieldPoint.Description, fieldPoint.Value, DbType.Double));
                        }
                    }
                }
                /* Add the Field Device Identifier to the parameters only
                 * if there are any field points to be logged
                 */
                if (parameters.Count > 0)
                {
                    /* Add the field Device identifier as parameter*/
                    parameters.Add(_dbManager.CreateParameter("@FieldDeviceIdentifier", fieldDevice.Identifier, DbType.String));

                    //invoke insert command of the DBmanager
                    int lastId = 0;
                    _dbManager.Insert("LogLiveData", CommandType.StoredProcedure, parameters.ToArray(), out lastId);
                    Console.WriteLine("Inserted Id : " + lastId);
                }
            }
        }

        public void LogLiveData(string fieldDeviceIdentifier, Dictionary<string, string> parametersData)
        {
            try
            {
                var parameters = new List<IDbDataParameter>();

                foreach (var keyValuePair in parametersData)
                {
                    parameters.Add(_dbManager.CreateParameter("@" + keyValuePair.Key, keyValuePair.Value, DbType.Double));
                }

                /* Add the Field Device Identifier to the parameters only
                 * if there are any field points to be logged
                 */
                if (parameters.Count > 0)
                {
                    /* Add the field Device identifier as parameter*/
                    parameters.Add(_dbManager.CreateParameter("@FieldDeviceIdentifier", fieldDeviceIdentifier, DbType.String));

                    //invoke insert command of the DBmanager
                    int lastId = 0;
                    _dbManager.Insert("LogLiveData", CommandType.StoredProcedure, parameters.ToArray(), out lastId);
                    logger.Log(LogType.Information, "Logged Live Data for " + fieldDeviceIdentifier);
                    Console.WriteLine("Inserted Id : " + lastId);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogType.Error, "Error in Logging Live Data for " + fieldDeviceIdentifier, ex, parametersData);
            }
        }

        public void AcknowledgeAlarm(string alarmIdentifier)
        {
            var parameters = new List<IDbDataParameter>
            {
                _dbManager.CreateParameter("@AlarmIdentifier" , alarmIdentifier, DbType.String),
            };

            int lastId = 0;
            _dbManager.Update("AcknowledgeAlarm", CommandType.StoredProcedure, parameters.ToArray());
            Console.WriteLine("Inserted Id : " + lastId);
        }

        public void DismissAlarm(string alarmIdentifier)
        {
            var parameters = new List<IDbDataParameter>
            {
                _dbManager.CreateParameter("@AlarmIdentifier" , alarmIdentifier, DbType.String),
            };

            int lastId = 0;
            _dbManager.Update("DismissAlarm", CommandType.StoredProcedure, parameters.ToArray());
            Console.WriteLine("Inserted Id : " + lastId);
        }

        public void UploadPdf(string fileName, byte[] pdfContent)
        {
            var parameters = new List<IDbDataParameter>
            {
                _dbManager.CreateParameter("@Identifier", DateTime.Now.ToString("yyyyMMddHHmmssfff"), DbType.String),
                _dbManager.CreateParameter("@Name", fileName, DbType.String),
                _dbManager.CreateParameter("@Content", pdfContent, DbType.Binary),
            };

            int lastId = 0;
            _dbManager.Insert("UploadPdf", CommandType.StoredProcedure, parameters.ToArray(), out lastId);
            Console.WriteLine("Inserted ID : " + lastId);
        }
        public void EndBatchCompact(string batchIdentifier)
        {
            var parameters = new List<IDbDataParameter>
            {
                _dbManager.CreateParameter("@BatchIdentifier", batchIdentifier, DbType.String),
            };

            int lastId = 0;
            _dbManager.Update("EndBatchAndUpdateStatusCompact", CommandType.StoredProcedure, parameters.ToArray());
            Console.WriteLine("Inserted Id : " + lastId);
        }
        public void EndBatch(string batchIdentifier, string cleanedBy, string cleaningSolvent)
        {
            var parameters = new List<IDbDataParameter>
            {
                _dbManager.CreateParameter("@BatchIdentifier", batchIdentifier, DbType.String),
                _dbManager.CreateParameter("@CleanedBy", cleanedBy, DbType.String),
                _dbManager.CreateParameter("@CleaningSolvent", cleaningSolvent, DbType.String),
            };

            int lastId = 0;
            _dbManager.Update("EndBatchAndUpdateCleaningStatus", CommandType.StoredProcedure, parameters.ToArray());
            Console.WriteLine("Inserted Id : " + lastId);
        }

        public void LogReactorImage(string fieldDeviceIdentifier, byte[] imgBytes)
        {
            var parameters = new List<IDbDataParameter>
            {
                _dbManager.CreateParameter("@FieldDeviceIdentifier", fieldDeviceIdentifier, DbType.String),
                _dbManager.CreateParameter("@ImageData", imgBytes, DbType.Binary),
            };

            int lastId = 0;
            _dbManager.Update("LogReactorImage", CommandType.StoredProcedure, parameters.ToArray());
            Console.WriteLine("Inserted Id : " + lastId);
        }

        public void LogReactorImageWithFieldDeviceParameters(string fieldDeviceIdentifier, byte[] imgBytes, Dictionary<string, string> fieldDeviceParameters)
        {
            var parameters = new List<IDbDataParameter>
            {
                _dbManager.CreateParameter("@FieldDeviceIdentifier", fieldDeviceIdentifier, DbType.String),
                _dbManager.CreateParameter("@ImageData", imgBytes, DbType.Binary),
            };

            var binFormatter = new BinaryFormatter();
            var memStream = new MemoryStream();
            binFormatter.Serialize(memStream, fieldDeviceParameters);

            parameters.Add(_dbManager.CreateParameter("@ParametersArray", memStream.ToArray(), DbType.Binary));

            int lastId = 0;
            _dbManager.Insert("LogReactorImageWithFieldDeviceParameters", CommandType.StoredProcedure, parameters.ToArray());
            Console.WriteLine("Inserted Id : " + lastId);
        }

        public void LogAlarm(byte[] monitoringDataPoints, string resolutionMessage, string alarmType, string alarmSeverity)
        {
            var parameters = new List<IDbDataParameter>
            {
                _dbManager.CreateParameter("@AlarmIdentifier", DateTime.Now.ToString("yyyyMMddHHmmssfff"), DbType.String),
                _dbManager.CreateParameter("@MonitoringDataPointsByteArray", monitoringDataPoints, DbType.Binary),
                _dbManager.CreateParameter("@ResolutionMessage", resolutionMessage, DbType.String),
                _dbManager.CreateParameter("@AlarmType", alarmType, DbType.String),
                _dbManager.CreateParameter("@AlarmSeverity", alarmSeverity, DbType.String),
            };

            int lastId = 0;
            _dbManager.Insert("LogAlarm", CommandType.StoredProcedure, parameters.ToArray(), out lastId);
            Console.WriteLine("Inserted ID : " + lastId);
        }

        #region New User Management functions
        public void UpdateUser(string userId, string fieldToBeUpdated, string updatedValue)
        {
            //create parameters array for adding new user to the database
            var parameters = new List<IDbDataParameter>
            {
                _dbManager.CreateParameter("@UserID", userId, DbType.String),
                _dbManager.CreateParameter("@FieldToBeUpdated", fieldToBeUpdated, DbType.String),
                _dbManager.CreateParameter("@UpdatedValue", updatedValue, DbType.String),
            };
            //invoke insert command of the DBmanager
            int lastId = 0;
            _dbManager.Insert("UpdateUser", CommandType.StoredProcedure, parameters.ToArray(), out lastId);
            Console.WriteLine("Inserted Id : " + lastId);
        }

        public void AddRole(string roleName, IList<string> modulesAccessable)
        {
            //create parameters array for adding new user to the database
            var parameters = new List<IDbDataParameter>
            {
                _dbManager.CreateParameter("@RoleName", roleName, DbType.String),
                _dbManager.CreateParameter("@ModulesAccessible", string.Join(",", modulesAccessable), DbType.String),
            };
            //invoke insert command of the DBmanager
            int lastId = 0;
            _dbManager.Insert("AddRole", CommandType.StoredProcedure, parameters.ToArray(), out lastId);
            Console.WriteLine("Inserted Id : " + lastId);
        }

        public void AssignRoleToUser(string userId, string roleName)
        {
            //create parameters array for adding new user to the database
            var parameters = new List<IDbDataParameter>
            {
                _dbManager.CreateParameter("@UserID", userId, DbType.String),
                _dbManager.CreateParameter("@RoleName", roleName, DbType.String),
            };
            //invoke insert command of the DBmanager
            int lastId = 0;
            _dbManager.Insert("AssignRoleToUser", CommandType.StoredProcedure, parameters.ToArray(), out lastId);
            Console.WriteLine("Inserted Id : " + lastId);
        }

        public void DeleteRole(string roleName)
        {
            //create parameters array for adding new user to the database
            var parameters = new List<IDbDataParameter>
            {
                _dbManager.CreateParameter("@RoleName", roleName, DbType.String),
            };
            //invoke insert command of the DBmanager
            int lastId = 0;
            _dbManager.Insert("DeleteRole", CommandType.StoredProcedure, parameters.ToArray(), out lastId);
            Console.WriteLine("Inserted Id : " + lastId);
        }

        public void UpdateRole(string roleName, IList<string> modulesAccessable)
        {
            //create parameters array for adding new user to the database
            var parameters = new List<IDbDataParameter>
            {
                _dbManager.CreateParameter("@RoleName", roleName, DbType.String),
                _dbManager.CreateParameter("@ModulesAccessible", string.Join(",", modulesAccessable), DbType.String),
            };

            //invoke insert command of the DBmanager
            int lastId = 0;
            _dbManager.Insert("UpdateRole", CommandType.StoredProcedure, parameters.ToArray(), out lastId);
            Console.WriteLine("Inserted Id : " + lastId);
        }
        #endregion
    }
}
