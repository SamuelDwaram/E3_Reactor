using E3.ReactorManager.Interfaces.AlarmUnit;
using E3.ReactorManager.Interfaces.DataAbstractionLayer;
using E3.ReactorManager.Interfaces.ErrorHandlingUnit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace E3.ReactorManager.HarwareAbstractionLayer
{
    public class DatabaseAccessor
    {
        #region Fields
        SqlConnection connectionSQL
            = new SqlConnection
            {
                //ConnectionString = "Data Source=127.0.0.1\\SQLEXPRESS;Initial Catalog=ReactorDB;User ID=sa;Password=SQLEXPRESS",
                //ConnectionString = "Data Source=192.168.0.113\\SQLEXPRESS;Initial Catalog=ReactorDB;User ID=sa;Password=SQLEXPRESS",
                ConnectionString = "Data Source=127.0.0.1\\SQLEXPRESS;Initial Catalog=BhelDB;User ID=sa;Password=SQLEXPRESS",
                //ConnectionString = "Data Source=127.0.0.1\\SQLEXPRESS;Initial Catalog=Etp1;User ID=sa;Password=SQLEXPRESS",
                //ConnectionString = "Data Source=EPMSLIMS006;Initial Catalog=EIL_KL_PRD_RD;User ID=kl_prd_rd;Password=Eisai@789",
            };

        SqlCommand commandSQL;
        #endregion

        #region Properties

        #endregion

        #region Functions
        /// <summary>
        /// Connect to Database (SQL Server)
        /// </summary>
        public async void ConnectDb()
        {
            try
            {
                if (connectionSQL != null
                        && connectionSQL.State == ConnectionState.Closed)
                {
                    //Open connection to Database
                    connectionSQL.Open();
                }
                else
                {
                    Console.WriteLine("Can't Connect to Database since already a connection is opened");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Connecting to Database");
                Console.WriteLine("Error messgae : " + ex.Message);
                Console.WriteLine("Error StackTrace : " + ex.StackTrace);
            }
            await Task.Yield();
        }

        /// <summary>
        /// Log events to Database
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <param name="dbTableName"></param>
        /// <param name="eventType"></param>
        /// <param name="messages"></param>
        public async void LogEvent(DateTime timeStamp, string dbTableName, EventType eventType, params string[] messages)
        {
            if (eventType.Equals(EventType.DeviceDataRead))
            {
                commandSQL
                    = new SqlCommand("insert into [" + dbTableName + "](ReactorTemperature, VapourTemperature, VentTemperature, Pressure, PHvalue, RPM, JacketOutletTemperature,TimeStamp) values(@reactorTemperature,@vapourTemperature,@ventTemperature,@pressure,@pHvalue,@rPM,@jacketOutletTemperature,@timeStamp)",
                                        connectionSQL);
                //commandSQL = new SqlCommand("InsertLiveData",connectionSQL);
                //commandSQL.CommandType = CommandType.StoredProcedure;
                commandSQL.Parameters.Add("@reactorTemperature", SqlDbType.Float).Value = messages[0];
                commandSQL.Parameters.Add("@vapourTemperature", SqlDbType.Float).Value = messages[1];
                commandSQL.Parameters.Add("@ventTemperature", SqlDbType.Float).Value = messages[2];
                commandSQL.Parameters.Add("@pressure", SqlDbType.Float).Value = messages[3];
                commandSQL.Parameters.Add("@pHvalue", SqlDbType.Float).Value = messages[4];
                commandSQL.Parameters.Add("@rPM", SqlDbType.Float).Value = messages[5];
                commandSQL.Parameters.Add("@jacketOutletTemperature", SqlDbType.Float).Value = messages[6];
                commandSQL.Parameters.Add("@timeStamp", SqlDbType.DateTime).Value = timeStamp;
            }
            else if (eventType.Equals(EventType.AlarmsRaised))
            {
                commandSQL
                    = new SqlCommand("insert into " + dbTableName + "(AlarmId, TimeOccured, MonitoringDataPoint, ResolutionMessage, AlarmType, AlarmState, AlarmSeverity) values(@alarmId, @timeOccured, @monitoringDataPoint, @resolutionMessage, @alarmType, @alarmState, @alarmSeverity)",
                                        connectionSQL);
                commandSQL.Parameters.Add("@timeOccured", SqlDbType.DateTime).Value = timeStamp;
                commandSQL.Parameters.Add("@alarmId", SqlDbType.NVarChar).Value = messages[0];
                commandSQL.Parameters.Add("@monitoringDataPoint", SqlDbType.VarChar).Value = messages[1];
                commandSQL.Parameters.Add("@resolutionMessage", SqlDbType.VarChar).Value = messages[2];
                commandSQL.Parameters.Add("@alarmType", SqlDbType.VarChar).Value = messages[3];
                commandSQL.Parameters.Add("@alarmState", SqlDbType.VarChar).Value = messages[4];
                commandSQL.Parameters.Add("@alarmSeverity", SqlDbType.VarChar).Value = messages[5];
            }
            else if (eventType.Equals(EventType.CreatedBatch))
            {
                commandSQL
                    = new SqlCommand("insert into " + dbTableName + "(BatchName, BatchNumber, ScientistName, FieldDeviceIdentifier, HCIdentifier, StirrerIdentifier, DosingPumpUsage, Comments, BatchState, TimeStarted)" +
                                        " values(@batchName, @batchNumber, @scientistName, @fieldDeviceIdentifier, @hCIdentifier, @stirrerIdentifier, @dosingPumpUsage, @comments, @batchState, @timeStarted)",
                                        connectionSQL);
                commandSQL.Parameters.Add("@batchName", SqlDbType.VarChar).Value = messages[0];
                commandSQL.Parameters.Add("@batchNumber", SqlDbType.VarChar).Value = messages[1];
                commandSQL.Parameters.Add("@scientistName", SqlDbType.VarChar).Value = messages[2];
                commandSQL.Parameters.Add("@fieldDeviceIdentifier", SqlDbType.VarChar).Value = messages[3];
                commandSQL.Parameters.Add("@hCIdentifier", SqlDbType.VarChar).Value = messages[4];
                commandSQL.Parameters.Add("@stirrerIdentifier", SqlDbType.VarChar).Value = messages[5];
                commandSQL.Parameters.Add("@dosingPumpUsage", SqlDbType.VarChar).Value = messages[6];
                commandSQL.Parameters.Add("@comments", SqlDbType.VarChar).Value = messages[7];
                commandSQL.Parameters.Add("@batchState", SqlDbType.VarChar).Value = messages[8];
                commandSQL.Parameters.Add("@timeStarted", SqlDbType.DateTime).Value = timeStamp;
            }
            else if (eventType.Equals(EventType.CreatedUser))
            {
                commandSQL
                    = new SqlCommand("insert into Users values(@userId, @name, @designation, @accessLevel, @currentStatus, @createdDate);" +
                                        "insert into Credentials values(@userId, @name, @username, @password)",
                                        connectionSQL);
                commandSQL.Parameters.Add("@userId", SqlDbType.NVarChar).Value = messages[0];
                commandSQL.Parameters.Add("@name", SqlDbType.NVarChar).Value = messages[1];
                commandSQL.Parameters.Add("@username", SqlDbType.NVarChar).Value = messages[2];
                commandSQL.Parameters.Add("@designation", SqlDbType.NVarChar).Value = messages[3];
                commandSQL.Parameters.Add("@accessLevel", SqlDbType.NVarChar).Value = messages[4];
                commandSQL.Parameters.Add("@currentStatus", SqlDbType.NVarChar).Value = messages[5];
                commandSQL.Parameters.Add("@password", SqlDbType.NVarChar).Value = messages[6];
                commandSQL.Parameters.Add("@createdDate", SqlDbType.DateTime).Value = timeStamp;
            }
            else if (eventType.Equals(EventType.AuditLog))
            {
                commandSQL
                    = new SqlCommand("insert into " + dbTableName + " values(@category, @auditMessage, @scientistName, @timeStamp)",
                                     connectionSQL);
                commandSQL.Parameters.Add("@auditMessage", SqlDbType.VarChar).Value = messages[0];
                commandSQL.Parameters.Add("@scientistName", SqlDbType.VarChar).Value = messages[1];
                commandSQL.Parameters.Add("@category", SqlDbType.VarChar).Value = messages[2];
                commandSQL.Parameters.Add("@timeStamp", SqlDbType.DateTime).Value = timeStamp;
            }
            else if (eventType.Equals(EventType.SaveActionComments))
            {
                commandSQL
                    = new SqlCommand("insert into " + dbTableName + " values(@fieldDeviceIdentifier, @actionComments, @user, @timeStamp)",
                                      connectionSQL);
                commandSQL.Parameters.Add("@fieldDeviceIdentifier", SqlDbType.VarChar).Value = messages[0];
                commandSQL.Parameters.Add("@actionComments", SqlDbType.VarChar).Value = messages[1];
                commandSQL.Parameters.Add("@user", SqlDbType.VarChar).Value = messages[2];
                commandSQL.Parameters.Add("@timeStamp", SqlDbType.DateTime).Value = timeStamp;
            }
            else if (eventType.Equals(EventType.SaveReactorImage))
            {
                var deserializer = new JavaScriptSerializer();

                commandSQL
                    = new SqlCommand("insert into " + dbTableName + " values(@fieldDeviceIdentifier, @reactorImage, @timeStamp)",
                                      connectionSQL);
                commandSQL.Parameters.Add("@fieldDeviceIdentifier", SqlDbType.NVarChar).Value = messages[0];
                commandSQL.Parameters.Add("@reactorImage", SqlDbType.VarBinary).Value = deserializer.Deserialize<byte[]>(messages[1]);
                commandSQL.Parameters.Add("@timeStamp", SqlDbType.DateTime).Value = timeStamp;
            }

            try
            {
                if (connectionSQL.State == ConnectionState.Open)
                {
                    commandSQL.ExecuteNonQuery();
                }
                else
                {
                    Console.WriteLine("Can't Execute Command since connection is in closed state");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Logging into Database");
                Console.WriteLine("Error messgae : " + ex.Message);
                Console.WriteLine("Error StackTrace : " + ex.StackTrace);
            }
            await Task.Yield();
        }

        public async void LogEvent(DateTime timeStamp, string dbTableName, EventType eventType, params dynamic[] messages)
        {
            if (eventType.Equals(EventType.SaveReactorImage))
            {
                commandSQL
                    = new SqlCommand("insert into " + dbTableName + " values(@fieldDeviceIdentifier, @reactorImage, @timeStamp)",
                                      connectionSQL);
                commandSQL.Parameters.Add("@fieldDeviceIdentifier", SqlDbType.NVarChar).Value = messages[0];
                commandSQL.Parameters.Add("@reactorImage", SqlDbType.VarBinary).Value = (byte[])(messages[1]);
                commandSQL.Parameters.Add("@timeStamp", SqlDbType.DateTime).Value = timeStamp;
            }
            try
            {
                if (connectionSQL.State == ConnectionState.Open)
                {
                    commandSQL.ExecuteNonQuery();
                }
                else
                {
                    Console.WriteLine("Can't Execute Command since connection is in closed state");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Logging into Database");
                Console.WriteLine("Error messgae : " + ex.Message);
                Console.WriteLine("Error StackTrace : " + ex.StackTrace);
            }
            await Task.Yield();
        }

        /// <summary>
        /// Function for reading Data from database
        /// </summary>
        /// <param name="dbTableName"></param>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public async Task<DataTable> ReadFromDb(string dbTableName, DateTime startDateTime, DateTime endDateTime, EventType eventType, List<string> Parameters)
        {
            //create a data table for storing the fetched data
            DataTable dataTable = new DataTable();

            if (eventType.Equals(EventType.AlarmsAccessed))
            {
                commandSQL
                    = new SqlCommand("select * from " + dbTableName + " order by TimeOccured desc",
                                     connectionSQL);
            }
            else if (eventType.Equals(EventType.ReadAlarmDetails))
            {
                commandSQL
                    = new SqlCommand("select * from " + dbTableName + " where AlarmId = @alarmIdentifier",
                                       connectionSQL);
                commandSQL.Parameters.Add("@alarmIdentifier", SqlDbType.NVarChar).Value = Parameters[0];
            }
            else if (eventType.Equals(EventType.AllUsersDetailsAccessed))
            {
                commandSQL
                    = new SqlCommand("select * from " + dbTableName + " order by CreatedDate desc",
                                     connectionSQL);
            }
            else if (eventType.Equals(EventType.ReadUserData))
            {
                commandSQL
                    = new SqlCommand("select * from " + dbTableName + " where UserID = @userID",
                                     connectionSQL);
                commandSQL.Parameters.Add("@userID", SqlDbType.BigInt).Value = Parameters[0];
            }
            else if (eventType.Equals(EventType.CheckUserStatus))
            {
                commandSQL
                    = new SqlCommand("select CurrentStatus from " + dbTableName + " where UserID = @userID",
                                     connectionSQL);
                commandSQL.Parameters.Add("@userID", SqlDbType.NVarChar).Value = Parameters[0];
            }
            else if (eventType.Equals(EventType.CheckSameUsername))
            {
                commandSQL
                    = new SqlCommand("select Username from " + dbTableName, connectionSQL);
            }
            else if (eventType.Equals(EventType.AuditTrailAccessed))
            {
                commandSQL
                    = new SqlCommand("select * from " + dbTableName + " order by TimeStamp desc",
                                     connectionSQL);
            }
            else if (eventType.Equals(EventType.AuditReportAccessed))
            {
                commandSQL
                    = new SqlCommand("select * from " + dbTableName + " where TimeStamp between @startDateTime and @endDateTime order by TimeStamp ",
                                     connectionSQL);
                commandSQL.Parameters.Add("@startDateTime", SqlDbType.DateTime).Value = startDateTime;
                commandSQL.Parameters.Add("@endDateTime", SqlDbType.DateTime).Value = endDateTime;
            }
            else if (eventType.Equals(EventType.DeviceDataRead))
            {
                commandSQL
                    = new SqlCommand("select " + GetSelectedParametersString(Parameters) + " from [" + dbTableName + "] where TimeStamp between @startDateTime and @endDateTime order by TimeStamp",
                                     connectionSQL);
                commandSQL.Parameters.Add("@startDateTime", SqlDbType.DateTime).Value = startDateTime;
                commandSQL.Parameters.Add("@endDateTime", SqlDbType.DateTime).Value = endDateTime;
            }
            else if (eventType.Equals(EventType.LiveTrendsAccessed))
            {
                //desc keyword not required in query
                commandSQL
                    = new SqlCommand("select * from (select top 12 " + GetSelectedParametersString(Parameters) + " from [" + dbTableName + "] order by TimeStamp desc) as LiveTrendsData order by TimeStamp asc", connectionSQL);
            }
            else if (eventType.Equals(EventType.TrendsAccessed))
            {
                commandSQL
                    = new SqlCommand("select * from [" + dbTableName + "] order by TimeStamp desc", connectionSQL);
            }
            else if (eventType.Equals(EventType.Login))
            {
                //use COLLATE SQL_Latin1_General_CP1_CS_AS in Sql Command for checking Case Sensitivity in the Parameter
                commandSQL
                    = new SqlCommand("select * from [" + dbTableName + "] where Username = @username COLLATE SQL_Latin1_General_CP1_CS_AS",
                                        connectionSQL);

                commandSQL.Parameters.Add("@username", SqlDbType.VarChar).Value = Parameters[0];
            }
            else if (eventType.Equals(EventType.RunningBatchesAccessed))
            {
                commandSQL = new SqlCommand("select * from [" + dbTableName + "] where FieldDeviceIdentifier = @rowIdentifier and BatchState = @batchState", connectionSQL);
                commandSQL.Parameters.Add("@rowIdentifier", SqlDbType.VarChar).Value = Parameters[0];
                commandSQL.Parameters.Add("@batchState", SqlDbType.VarChar).Value = "Running";
            }
            else if (eventType.Equals(EventType.BatchDataAccessed))
            {
                commandSQL = new SqlCommand("select * from [" + dbTableName + "] where BatchName = @batchName", connectionSQL);
                commandSQL.Parameters.Add("@batchName", SqlDbType.VarChar).Value = Parameters[0];
            }
            else if (eventType.Equals(EventType.ListAllRunningBatches))
            {
                commandSQL = new SqlCommand("select * from [" + dbTableName + "] where BatchState = 'Running'", connectionSQL);
            }
            else if (eventType.Equals(EventType.ListAllBatches))
            {
                commandSQL = new SqlCommand("select * from BatchTable", connectionSQL);
            }
            else if (eventType.Equals(EventType.ActionCommentsAccessed))
            {
                commandSQL = new SqlCommand("select * from [" + dbTableName + "] where TimeStamp between @startDateTime and @endDateTime and FieldDeviceIdentifier = @fieldDeviceIdentifier order by TimeStamp", connectionSQL);
                commandSQL.Parameters.Add("@startDateTime", SqlDbType.DateTime).Value = startDateTime;
                commandSQL.Parameters.Add("@endDateTime", SqlDbType.DateTime).Value = endDateTime;
                commandSQL.Parameters.Add("@fieldDeviceIdentifier", SqlDbType.VarChar).Value = Parameters[0];
            }
            else if (eventType.Equals(EventType.LatestActionCommentsAccessed))
            {
                commandSQL = new SqlCommand("select top 10 * from [" + dbTableName + "] where FieldDeviceIdentifier = @fieldDeviceIdentifier order by TimeStamp desc", connectionSQL);
                commandSQL.Parameters.Add("@fieldDeviceIdentifier", SqlDbType.VarChar).Value = Parameters[0];
            }
            else if (eventType.Equals(EventType.ReactorImagesAccessed))
            {
                commandSQL = new SqlCommand("select * from [" + dbTableName + "] where TimeStamp between @startDateTime and @endDateTime and FieldDeviceIdentifier = @fieldDeviceIdentifier order by TimeStamp", connectionSQL);
                commandSQL.Parameters.Add("@startDateTime", SqlDbType.DateTime).Value = startDateTime;
                commandSQL.Parameters.Add("@endDateTime", SqlDbType.DateTime).Value = endDateTime;
                commandSQL.Parameters.Add("@fieldDeviceIdentifier", SqlDbType.VarChar).Value = Parameters[0];
            }
            try
            {
                if (connectionSQL.State == ConnectionState.Open)
                {
                    //suspend the ReadFromDb function here
                    //to execute the command and fetch the data asynchronously
                    commandSQL.ExecuteNonQuery();

                    //data Adapter for capturing the fetched data through commandSQL
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(commandSQL);

                    sqlDataAdapter.Fill(dataTable);
                }
                else
                {
                    Console.WriteLine("Can't execute command since connection is not in open state");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in reading from Database");
                Console.WriteLine("Error messgae : " + ex.Message);
                Console.WriteLine("Error StackTrace : " + ex.StackTrace);
            }
            await Task.Yield();

            return dataTable;
        }
        /// <summary>
        /// Function for returning the string(for using in SQL query)
        /// which contains the join of all the selected parameters
        /// </summary>
        /// <param name="selectedParametersList"></param>
        /// <returns></returns>
        public string GetSelectedParametersString(List<string> selectedParametersList)
        {
            //initialize selectedParametersString to null
            string selectedParametersString = null;

            foreach (var selectedParameter in selectedParametersList)
            {
                if (string.IsNullOrEmpty(selectedParametersString))
                {
                    selectedParametersString = selectedParameter;
                }
                else
                {
                    selectedParametersString += ",";
                    selectedParametersString += selectedParameter;
                }
            }

            return selectedParametersString;
        }
        /// <summary>
        /// Updates a particular row in Database
        /// </summary>
        public async Task<bool> UpdateDb(string rowIdentifier, string dbTableName, EventType eventType, params dynamic[] messages)
        {
            bool result = false;

            if (eventType.Equals(EventType.AlarmAcknowledged))
            {
                commandSQL
                    = new SqlCommand("update " + dbTableName + " set TimeActionDone = @timeActionDone, AlarmState = @alarmState where AlarmId = @alarmIdentifier", connectionSQL);
                commandSQL.Parameters.Add("@alarmIdentifier", SqlDbType.NVarChar).Value = rowIdentifier;
                commandSQL.Parameters.Add("@timeActionDone", SqlDbType.DateTime).Value = DateTime.Now;
                commandSQL.Parameters.Add("@alarmState", SqlDbType.NVarChar).Value = AlarmState.Acknowledged;
            }
            else if (eventType.Equals(EventType.AlarmDismissed))
            {
                commandSQL
                    = new SqlCommand("update " + dbTableName + " set TimeActionDone = @timeActionDone, AlarmState = @alarmState where AlarmId = @alarmIdentifier", connectionSQL);
                commandSQL.Parameters.Add("@alarmIdentifier", SqlDbType.NVarChar).Value = rowIdentifier;
                commandSQL.Parameters.Add("@timeActionDone", SqlDbType.DateTime).Value = DateTime.Now;
                commandSQL.Parameters.Add("@alarmState", SqlDbType.NVarChar).Value = AlarmState.Dismissed;
            }
            else if (eventType.Equals(EventType.EndedBatch))
            {
                commandSQL = new SqlCommand("update " + dbTableName + " set BatchState = 'Completed', TimeCompleted = @timeCompleted where BatchName = @rowIdentifier ", connectionSQL);
                commandSQL.Parameters.Add("@timeCompleted", SqlDbType.DateTime).Value = DateTime.Now;
                commandSQL.Parameters.Add("@rowIdentifier", SqlDbType.NVarChar).Value = rowIdentifier;
            }
            else if (eventType.Equals(EventType.EnabledUser))
            {
                commandSQL = new SqlCommand("update " + dbTableName + " set CurrentStatus = 'Active' where UserID = @rowIdentifier ", connectionSQL);
                commandSQL.Parameters.Add("@rowIdentifier", SqlDbType.BigInt).Value = rowIdentifier;
            }
            else if (eventType.Equals(EventType.DisabledUser))
            {
                commandSQL = new SqlCommand("update " + dbTableName + " set CurrentStatus = 'InActive' where UserID = @rowIdentifier ", connectionSQL);
                commandSQL.Parameters.Add("@rowIdentifier", SqlDbType.BigInt).Value = rowIdentifier;
            }
            else if (eventType.Equals(EventType.ModifyPassword))
            {
                commandSQL
                    = new SqlCommand("update Credentials set Credentials.Password = @password where Credentials.UserID = @rowIdentifier",
                                     connectionSQL);
                commandSQL.Parameters.Add("@rowIdentifier", SqlDbType.BigInt).Value = rowIdentifier;
                commandSQL.Parameters.Add("@password", SqlDbType.NVarChar).Value = messages[0];
            }
            else if (eventType.Equals(EventType.ModifyAccessLevel))
            {
                commandSQL
                    = new SqlCommand("update Users set Users.AccessLevel = @accessLevel where Users.UserID = @rowIdentifier",
                                     connectionSQL);
                commandSQL.Parameters.Add("@rowIdentifier", SqlDbType.BigInt).Value = rowIdentifier;
                commandSQL.Parameters.Add("@accessLevel", SqlDbType.NVarChar).Value = messages[0];
            }

            try
            {
                if (connectionSQL.State == ConnectionState.Open)
                {
                    commandSQL.ExecuteNonQuery();

                    result = true;
                }
                else
                {
                    Console.WriteLine("Can't execute command since connection is not in open state");
                    result = false;
                }
            }
            catch (Exception ex)
            {
                result = false;
                Console.WriteLine("command execution failed");
                Console.WriteLine("Error in updating the Database");
                Console.WriteLine("Error in message : " + ex.Message);
                Console.WriteLine("Error StackTrace : " + ex.StackTrace);
            }

            await Task.Yield();

            return result;
        }

        /// <summary>
        /// Function for closing connection to database
        /// </summary>
        public async void CloseConnectionDb()
        {
            if (connectionSQL.State == ConnectionState.Open)
            {
                connectionSQL.Close();
            }
            else
            {
                Console.WriteLine("Can't close connection since connection is not in open state");
            }
            await Task.Yield();
        }
        #endregion
    }
}
