using E3.ReactorManager.Interfaces.DataAbstractionLayer;
using E3.ReactorManager.Interfaces.DataAbstractionLayer.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Unity;

namespace E3Tech.RecipeBuilding.Model.RecipeExecutionInfoProvider
{
    public class RecipeExecutionInfoHandler : IRecipeExecutionInfoHandler
    {
        IDatabaseReader databaseReader;
        IDatabaseWriter databaseWriter;

        public RecipeExecutionInfoHandler(IUnityContainer containerProvider, IDatabaseWriter databaseWriter, IDatabaseReader databaseReader)
        {
            this.databaseWriter = databaseWriter;
            this.databaseReader = databaseReader;
        }

        public void AddRecipeExecutionInfo(string deviceId, string startTime, string endTime, string duration, string executionMessage)
        {
            IList<DbParameterInfo> parameters = new List<DbParameterInfo>
            {
                new DbParameterInfo("@FieldDeviceIdentifier", deviceId, DbType.String),
                new DbParameterInfo("@StartTime", startTime, DbType.String),
                new DbParameterInfo("@EndTime", endTime, DbType.String),
                new DbParameterInfo("@Duration", duration, DbType.String),
                new DbParameterInfo("@ExecutionMessage", executionMessage, DbType.String),
            };

            databaseWriter.ExecuteWriteCommand("AddRecipeBlockExecutionInfo", CommandType.StoredProcedure, parameters);
        }

        public IList<RecipeBlockExecutionInfo> GetRecipeExecutionInfo(string deviceId, DateTime startTime, DateTime endTime)
        {
            IList<DbParameterInfo> parameters = new List<DbParameterInfo>
            {
                new DbParameterInfo("@FieldDeviceIdentifier", deviceId, DbType.String),
                new DbParameterInfo("@StartTime", startTime.ToString(), DbType.String),
                new DbParameterInfo("@EndTime", endTime.ToString(), DbType.String),
            };

            return (from DataRow row in databaseReader.ExecuteReadCommand("GetRecipeBlockExecutionInfo", CommandType.StoredProcedure, parameters).AsEnumerable()
                    select new RecipeBlockExecutionInfo { 
                        FieldDeviceIdentifier = row["FieldDeviceIdentifier"].ToString(),
                        StartTime = row["StartTime"].ToString(),
                        EndTime = row["EndTime"].ToString(),
                        Duration = row["Duration"].ToString(),
                        ExecutionMessage = row["ExecutionMessage"].ToString(),
                    }).ToList();
        }
    }
}
