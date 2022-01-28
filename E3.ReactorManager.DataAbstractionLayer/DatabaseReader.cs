using E3.ReactorManager.DataAccessHandler;
using E3.ReactorManager.Interfaces.DataAbstractionLayer;
using E3.ReactorManager.Interfaces.DataAbstractionLayer.Data;
using E3.ReactorManager.Interfaces.Framework.Logging;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TwinCAT.Ads;
using Unity;

namespace E3.ReactorManager.DataAbstractionLayer
{
    public class DatabaseReader : IDatabaseReader
    {
        private readonly DBManager _dbManager;
        ILogger _logger;

        public DatabaseReader(IUnityContainer containerProvider)
        {
            _logger = containerProvider.Resolve<ILogger>();
            _dbManager = new DBManager("DBconnection");
        }

        public IList<Plc> FetchPlcData(Action<object, AdsNotificationExEventArgs> callBack)
        {
            //fetch the plc list first to give them to corresponding field device
            List<Plc> plcList = new List<Plc>();
            IDbConnection connection = null;
            var dataReader = _dbManager.GetDataReader("select * from dbo.PlcList", CommandType.Text, null, out connection);
            try
            {
                while (dataReader.Read())
                {
                    var plc = new Plc
                    {
                        TwinCATClient = new TcAdsClient(),
                        Identifier = dataReader["Identifier"].ToString(),
                        Label = dataReader["Label"].ToString(),
                        Address = dataReader["Address"].ToString(),
                        PortNumber = int.Parse(dataReader["PortNumber"].ToString()),
                    };

                    plc.TwinCATClient.AdsNotificationEx += new AdsNotificationExEventHandler(callBack);

                    plcList.Add(plc);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                dataReader.Close();
                _dbManager.CloseConnection(connection);
            }

            return plcList;
        }

        public TcAdsClient GetTwinCATClient(string plcIdentifier, List<Plc> plcList)
        {
            var plc = plcList.Where(client => client.Identifier == plcIdentifier).First();
            
            return plc.TwinCATClient;
        }
        
        /// <summary>
        /// Fetch field devices data from database
        /// </summary>
        public IList<FieldDevice> FetchFieldDevicesData(object callBack)
        {
            //fetch the plc data to match them with the corresponding field device
            List<Plc> plcList = new List<Plc>(FetchPlcData(callBack as Action<object, AdsNotificationExEventArgs>));

            List<FieldDevice> fieldDevices = new List<FieldDevice>();
            //fetch the field devices data
            IDbConnection connection = null;
            var dataReader = _dbManager.GetDataReader("select * from dbo.FieldDevices join dbo.PlcList on FieldDevices.PlcIdentifier = PlcList.Identifier", CommandType.Text, null, out connection);
            try
            {
                while (dataReader.Read())
                {
                    var fieldDevice = new FieldDevice
                    {
                        Identifier = dataReader["Identifier"].ToString(),
                        Label = dataReader["Label"].ToString(),
                        Type = dataReader["Type"].ToString(),
                        RelatedPlc = new Plc
                        {
                            TwinCATClient = GetTwinCATClient(dataReader["Address"].ToString(), plcList),
                            Address = dataReader["Address"].ToString(),
                            PortNumber = int.Parse(dataReader["PortNumber"].ToString()),
                        },
                    };
                    fieldDevices.Add(fieldDevice);
                }

                //update each field device's sensor data set
                foreach (var fieldDevice in fieldDevices)
                {
                    fieldDevice.SensorsData = FetchSensorDataSets(fieldDevice.Identifier);
                }

                //update each field device's command points
                foreach (var fieldDevice in fieldDevices)
                {
                    IList<string> fpLabels = ExecuteReadCommand($"select Label from dbo.CommandPoints where FieldDeviceIdentifier='{fieldDevice.Identifier}'", CommandType.Text)
                                                    .AsEnumerable().Select(r => Convert.ToString(r.Field<string>(0))).ToList();
                    foreach (SensorsDataSet sensorsDataSet in fieldDevice.SensorsData)
                    {
                        foreach (FieldPoint fieldPoint in sensorsDataSet.SensorsFieldPoints.Where(fp => fpLabels.Any(fpLabel => fpLabel == fp.Label)))
                        {
                            fieldDevice.CommandPoints.Add(fieldPoint);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                dataReader.Close();
                _dbManager.CloseConnection(connection);
            }

            return fieldDevices;
        }

        public IList<SensorsDataSet> FetchSensorDataSets(string fieldDeviceIdentifier)
        {
            List<SensorsDataSet> sensorsDataSets = new List<SensorsDataSet>();
            var dataReader = _dbManager.GetDataReader($"select * from dbo.SensorsDataSet where FieldDeviceIdentifier='{fieldDeviceIdentifier}'", CommandType.Text, null, out IDbConnection connection);
            try
            {
                while (dataReader.Read())
                {
                    var sensorDataSet = new SensorsDataSet
                    {
                        Identifier = dataReader["Identifier"].ToString(),
                        Label = dataReader["Label"].ToString(),
                        DataUnit = dataReader["DataUnit"].ToString(),
                    };

                    sensorsDataSets.Add(sensorDataSet);
                }

                //fetch the field points for each Sensor Data Set
                foreach (var sensorDataSet in sensorsDataSets)
                {
                    sensorDataSet.SensorsFieldPoints = FetchFieldPoints(sensorDataSet.Identifier);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                dataReader.Close();
                _dbManager.CloseConnection(connection);
            }

            return sensorsDataSets;
        }
        public string FetchRunningBatch()
        {
            string RunningBatch;
            var dataReader = _dbManager.GetScalarValue($"select top(1) Identifier from [dbo].BatchTableCompact  where State='Running' order by TimeStarted desc", CommandType.Text, null);
            try
            {
                RunningBatch = dataReader?.ToString();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
            }

            return RunningBatch;
        }
        public IList<FieldPoint> FetchFieldPoints(string sensorDataSetIdentifier)
        {
            List<FieldPoint> fieldPoints = new List<FieldPoint>();

            var parameters = new List<IDbDataParameter>
            {
                _dbManager.CreateParameter("@SensorDataSetIdentifier", sensorDataSetIdentifier, DbType.String),
            };

            var dataReader = _dbManager.GetDataReader($"select * from dbo.FieldPoints where SensorDataSetIdentifier='{sensorDataSetIdentifier}'", CommandType.Text, null, out IDbConnection connection);
            try
            {
                while (dataReader.Read())
                {
                    var fieldPoint = new FieldPoint
                    {
                        Label = dataReader["Label"].ToString(),
                        Description = dataReader["Description"].ToString(),
                        TypeOfAddress = dataReader["TypeOfAddress"].ToString(),
                        MemoryAddress = dataReader["MemoryAddress"].ToString(),
                        FieldPointDataType = dataReader["FieldPointDataType"].ToString(),
                        SensorDataSetIdentifier = dataReader["SensorDataSetIdentifier"].ToString(),
                        RequireNotificationService = (dataReader["RequireNotificationService"].ToString().Length > 0) ? bool.Parse(dataReader["RequireNotificationService"].ToString()) : false,
                        ToBeLogged = (dataReader["ToBeLogged"].ToString().Length > 0) ? bool.Parse(dataReader["ToBeLogged"].ToString()) : false,
                    };

                    fieldPoints.Add(fieldPoint);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                dataReader.Close();
                _dbManager.CloseConnection(connection);
            }

            return fieldPoints;
        }

        public DataTable ReadFieldDeviceData(string fieldDeviceIdentifier, IList<string> selectedFieldDeviceParameters, DateTime startTime, DateTime endTime)
        {
            var parameters = new List<IDbDataParameter>
            {
                _dbManager.CreateParameter("@FieldDeviceIdentifier", fieldDeviceIdentifier, DbType.String),
                _dbManager.CreateParameter("@startTime", startTime, DbType.DateTime),
                _dbManager.CreateParameter("@endTime", endTime, DbType.DateTime),
            };

            foreach (string fieldDeviceParameter in selectedFieldDeviceParameters)
            {
                parameters.Add(_dbManager.CreateParameter("@" + fieldDeviceParameter, bool.TrueString, DbType.String));
            }

            IDbConnection connection = null;
            var dataReader = _dbManager.GetDataReader("ReadFieldDeviceData", CommandType.StoredProcedure, parameters.ToArray(), out connection);

            DataTable fieldDeviceData = new DataTable();

            try
            {
                fieldDeviceData.Load(dataReader);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _dbManager.CloseConnection(connection);
            }

            return fieldDeviceData;
        }

        public DataTable GetDocumentDetails(string identifier)
        {
            var parameters = new List<IDbDataParameter>
            {
                _dbManager.CreateParameter("@Identifier", identifier, DbType.String),
            };

            return GetResultFromDatabase("GetPdfDetails", CommandType.StoredProcedure, parameters);
        }

        public DataTable GetAvailablePdfList()
        {
            return GetResultFromDatabase("GetAvailablePdfList", CommandType.StoredProcedure, null);
        }

        public DataTable ExecuteReadCommand(string commandText, CommandType commandType, IList<DbParameterInfo> parametersList = null)
        {
            IList<IDbDataParameter> dbParameters = new List<IDbDataParameter>();
            if (parametersList != null)
            {
                foreach (DbParameterInfo parameterInfo in parametersList)
                {
                    dbParameters.Add(_dbManager.CreateParameter(parameterInfo.Name, parameterInfo.Value, parameterInfo.DbType));
                }
            }

            DataTable resultDataTable = new DataTable();
            IDataReader dataReader = _dbManager.GetDataReader(commandText, commandType, dbParameters.ToArray(), out IDbConnection connection);

            try
            {
                resultDataTable.Load(dataReader);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _dbManager.CloseConnection(connection);
            }

            return resultDataTable;
        }

        private DataTable GetResultFromDatabase(string commandText, CommandType commandType, IList<IDbDataParameter> parameters = null)
        {
            if (parameters == null)
            {
                //Handle the case if Parameters were null
                parameters = new List<IDbDataParameter>();
            }

            var resultDataTable = new DataTable();

            IDbConnection connection = null;
            var dataReader = _dbManager.GetDataReader(commandText, commandType, parameters.ToArray(), out connection);

            try
            {
                resultDataTable.Load(dataReader);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _dbManager.CloseConnection(connection);
            }

            return resultDataTable;
        }

        #region New Field devices functions
        public DataTable GetFieldDevicesList()
        {
            return GetResultFromDatabase("GetFieldDevicesList", CommandType.StoredProcedure);
        }

        public DataTable GetSensorsDataSetInFieldDevice(string fieldDeviceIdentifier)
        {
            var parameters = new List<IDbDataParameter>()
            {
                _dbManager.CreateParameter("@FieldDeviceIdentifier", fieldDeviceIdentifier, DbType.String),
            };

            return GetResultFromDatabase("GetSensorsDataSetInFieldDevice", CommandType.StoredProcedure, parameters);
        }

        public DataTable GetFieldPointsInSensorsDataSet(string sensorsDataSetIdentifier)
        {
            var parameters = new List<IDbDataParameter>()
            {
                _dbManager.CreateParameter("@SensorsDataSetIdentifier", sensorsDataSetIdentifier, DbType.String),
            };

            return GetResultFromDatabase("GetFieldPointsInSensorsDataSet", CommandType.StoredProcedure, parameters);
        }

        public DataTable GetControllersConnectedToFieldDevice(string fieldDeviceIdentifier)
        {
            var parameters = new List<IDbDataParameter>()
            {
                _dbManager.CreateParameter("@FieldDeviceIdentifier", fieldDeviceIdentifier, DbType.String),
            };

            return GetResultFromDatabase("GetControllersConnectedToFieldDevice", CommandType.StoredProcedure, parameters);
        }

        public DataTable GetControllerInfo(string controllerIdentifier)
        {
            var parameters = new List<IDbDataParameter>()
            {
                _dbManager.CreateParameter("@ControllerIdentifier", controllerIdentifier, DbType.String),
            };

            return GetResultFromDatabase("GetControllerInfo", CommandType.StoredProcedure, parameters);
        }

        public Dictionary<string, string> GetAvailableFieldDevices()
        {
            Dictionary<string, string> availableFieldDevices = new Dictionary<string, string>();

            IDbConnection connection = null;
            var dataReader = _dbManager.GetDataReader("GetAvailableFieldDevices", CommandType.StoredProcedure, null, out connection);

            try
            {
                while (dataReader.Read())
                {
                    availableFieldDevices.Add(dataReader["Identifier"].ToString(), dataReader["Label"].ToString());
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                dataReader.Close();
                _dbManager.CloseConnection(connection);
            }

            return availableFieldDevices;
        }

        public IList<string> GetAvailableFieldPoints(string fieldDeviceIdentifier)
        {
            IList<string> availableFieldPoints = new List<string>();

            IList<IDbDataParameter> parameters = new List<IDbDataParameter>
            {
                _dbManager.CreateParameter("@FieldDeviceIdentifier", fieldDeviceIdentifier, DbType.String)
            };

            IDbConnection connection = null;
            var dataReader = _dbManager.GetDataReader("GetAvailableFieldPointsInFieldDevice", CommandType.StoredProcedure, parameters.ToArray(), out connection);

            try
            {
                while (dataReader.Read())
                {
                    availableFieldPoints.Add(dataReader["Label"].ToString());
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                dataReader.Close();
                _dbManager.CloseConnection(connection);
            }

            return availableFieldPoints;
        }
        #endregion

        #region New Batch Info Extractor Functions
        /// <summary>
        /// Returns all the completed batches
        /// </summary>
        /// <returns></returns>
        public DataTable GetCompletedBatches()
        {
            return GetResultFromDatabase("GetAllBatches", CommandType.StoredProcedure);
        }

        /// <summary>
        /// Returns batch info using batch identifier
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        public DataTable GetBatchInfo(string batchId)
        {
            var parameters = new List<IDbDataParameter>
            {
                _dbManager.CreateParameter("@Identifier", batchId, DbType.String)
            };

            return GetResultFromDatabase("GetBatchInfo", CommandType.StoredProcedure, parameters);
        }

        /// <summary>
        /// Returns all the running batches in the plant
        /// </summary>
        /// <returns></returns>
        public DataTable GetAllRunningBatchesInThePlant()
        {
            return GetResultFromDatabase("GetAllRunningBatches", CommandType.StoredProcedure);
        }

        /// <summary>
        /// Returns info of batch running in FieldDevice using device Identifier
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public DataTable GetBatchRunningInDevice(string deviceId)
        {
            var parameters = new List<IDbDataParameter>
            {
                _dbManager.CreateParameter("@FieldDeviceIdentifier", deviceId, DbType.String)
            };

            return GetResultFromDatabase("GetBatchRunningInDevice", CommandType.StoredProcedure, parameters);
        }
        #endregion

        #region New User management functions
        public DataTable GetAllUsersInTheDatabase()
        {
            return GetResultFromDatabase("GetAllUsers", CommandType.StoredProcedure);
        }

        public DataTable AuthenticateCredentials(string username, string password)
        {
            var parameters = new List<IDbDataParameter>
            {
                _dbManager.CreateParameter("@Username", username, DbType.String),
                _dbManager.CreateParameter("@Password", password, DbType.String),
            };

            return GetResultFromDatabase("AuthenticateCredentials", CommandType.StoredProcedure, parameters);
        }

        public DataTable GetAllRoles()
        {
            return GetResultFromDatabase("GetAllRoles", CommandType.StoredProcedure);
        }

        public DataTable GetAccessibleModulesByRole(string roleName)
        {
            var parameters = new List<IDbDataParameter>
            {
                _dbManager.CreateParameter("@RoleName", roleName, DbType.String),
            };

            return GetResultFromDatabase("GetAccessibleModulesByRole", CommandType.StoredProcedure, parameters);
        }

        public DataTable GetAssignedRolesOfUser(string userId)
        {
            var parameters = new List<IDbDataParameter>
            {
                _dbManager.CreateParameter("@UserId", userId, DbType.String),
            };

            return GetResultFromDatabase("GetAssignedRolesOfUser", CommandType.StoredProcedure, parameters);
        }
        #endregion
    }
}