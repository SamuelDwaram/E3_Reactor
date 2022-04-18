using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using E3.ActionComments.Model.Data;
using E3.ReactorManager.Interfaces.DataAbstractionLayer;
using E3.ReactorManager.Interfaces.DataAbstractionLayer.Data;

namespace E3.ActionComments.Model
{
    public class ActionCommentsHandler : IActionCommentsHandler
    {
        private readonly IDatabaseReader databaseReader;
        private readonly IDatabaseWriter databaseWriter;

        public ActionCommentsHandler(IDatabaseWriter databaseWriter, IDatabaseReader databaseReader)
        {
            this.databaseReader = databaseReader;
            this.databaseWriter = databaseWriter;
        }

        public IList<ActionCommentsInfo> GetActionComments(string fieldDeviceIdentifier, DateTime? startTime = null, DateTime? endTime = null)
        {
            if (startTime.HasValue && endTime.HasValue)
            {
                IList<DbParameterInfo> parameters = new List<DbParameterInfo>
                {
                    new DbParameterInfo("@FieldDeviceIdentifier", fieldDeviceIdentifier, DbType.String),
                    new DbParameterInfo("@startTime", $"{startTime.Value:yyyy-MM-dd HH:mm:ss}", DbType.DateTime),
                    new DbParameterInfo("@endTime", $"{endTime.Value:yyyy-MM-dd HH:mm:ss}", DbType.DateTime),
                };

                return (from DataRow row in databaseReader.ExecuteReadCommand("ReadActionComments", CommandType.StoredProcedure, parameters).AsEnumerable()
                        select new ActionCommentsInfo 
                        {
                            FieldDeviceLabel = GetFieldDeviceLabel(row["FieldDeviceIdentifier"]),
                            Comments = row["Comments"].ToString(),
                            NameOfUser = row["User"].ToString(),
                            TimeStamp = DateTime.TryParse(row["TimeStamp"].ToString(), out DateTime timeStamp) ? DateTime.Parse(row["TimeStamp"].ToString()) : default,
                        }).ToList();
            }
            else
            {
                //Return Top 10 Action Comments
                IList<DbParameterInfo> parameters = new List<DbParameterInfo>
                {
                    new DbParameterInfo("@FieldDeviceIdentifier", fieldDeviceIdentifier, DbType.String),
                };

                return (from DataRow row in databaseReader.ExecuteReadCommand("GetLatestActionComments", CommandType.StoredProcedure, parameters).AsEnumerable()
                        select new ActionCommentsInfo
                        {
                            FieldDeviceLabel = row["FieldDeviceIdentifier"].ToString(),
                            Comments = row["Comments"].ToString(),
                            NameOfUser = row["User"].ToString(),
                            TimeStamp = DateTime.TryParse(row["TimeStamp"].ToString(), out DateTime timeStamp) ? DateTime.Parse(row["TimeStamp"].ToString()) : default,
                        }).ToList();
            }
        }

        private string GetFieldDeviceLabel(object deviceId)
        {
            return databaseReader.ExecuteReadCommand($"select top(1) Label from dbo.FieldDevices where Identifier='{deviceId}'", CommandType.Text).AsEnumerable()
                    .First().ItemArray[0].ToString();
        }

        public void LogActionComments(string fieldDeviceIdentifier, string comments, string user)
        {
            IList<DbParameterInfo> parameters = new List<DbParameterInfo>
            {
                new DbParameterInfo("@FieldDeviceIdentifier" , fieldDeviceIdentifier, DbType.String),
                new DbParameterInfo("@Comments" , comments, DbType.String),
                new DbParameterInfo("@User" , user, DbType.String),
            };
            databaseWriter.ExecuteWriteCommand("LogActionComments", CommandType.StoredProcedure, parameters);

            UpdateActionCommentsView?.Invoke(this, new EventArgs());
        }

        public event EventHandler UpdateActionCommentsView;
    }
}
