using System;
using System.Linq;
using System.Collections.Generic;
using E3.ReactorManager.Interfaces.DataAbstractionLayer;
using System.Data;
using System.Text;
using E3.ReactorManager.DesignExperiment.Model.Data;
using E3.ReactorManager.Interfaces.DataAbstractionLayer.Data;

namespace E3.ReactorManager.DesignExperiment.Model
{
    public class ExperimentInfoProvider : IExperimentInfoProvider
    {
        private readonly IDatabaseReader databaseReader;

        public ExperimentInfoProvider(IDatabaseReader databaseReader)
        {
            this.databaseReader = databaseReader;
        }

        public IList<Batch> GetAllCompletedBatches()
        {
            DataTable result = databaseReader.GetCompletedBatches();
            if (result.Rows.Count > 0)
            {
                return (from DataRow row in result.AsEnumerable()
                        select PrepareBatchFromDataTable(row)).ToList();
            }
            else
            {
                return new List<Batch>();
            }
        }

        public IList<Batch> GetAllRunningBatchesInThePlant()
        {
            DataTable result = databaseReader.GetAllRunningBatchesInThePlant();
            if (result.Rows.Count > 0)
            {
                return (from DataRow row in result.AsEnumerable()
                        select PrepareBatchFromDataTable(row)).ToList();
            }
            else
            {
                return new List<Batch>();
            }
        }

        public Batch GetBatchDetailsByName(string batchName)
        {
            IList<DbParameterInfo> parameters = new List<DbParameterInfo>()
            {
                new DbParameterInfo("@Name", batchName, DbType.String)
            };

            DataTable result = databaseReader.ExecuteReadCommand("GetBatchDetails", CommandType.StoredProcedure, parameters);
            if (result.Rows.Count > 0)
            {
                return PrepareBatchFromDataTable(result.Rows[0]);
            }
            else
            {
                return new Batch();
            }
        }

        public Batch GetBatchInfo(string batchId)
        {
            DataTable result = databaseReader.GetBatchInfo(batchId);
            if (result.Rows.Count > 0)
            {
                return PrepareBatchFromDataTable(result.Rows[0]);
            }
            else
            {
                return new Batch();
            }
        }

        public Batch GetBatchRunningInDevice(string deviceId)
        {
            DataTable result = databaseReader.GetBatchRunningInDevice(deviceId);
            if (result.Rows.Count > 0)
            {
                return PrepareBatchFromDataTable(result.Rows[0]);
            }
            else
            {
                return new Batch();
            }
        }

        private Batch PrepareBatchFromDataTable(DataRow row)
        {
            return new Batch
                    {
                        Identifier = row["Identifier"].ToString(),
                        Name = row["Name"].ToString(),
                        Number = row["Number"].ToString(),
                        Stage = row["Stage"].ToString(),
                        ScientistName = row["ScientistName"].ToString(),
                        FieldDeviceIdentifier = row["FieldDeviceIdentifier"].ToString(),
                        FieldDeviceLabel = row["FieldDeviceLabel"].ToString(),
                        HCIdentifier = row["HCIdentifier"].ToString(),
                        StirrerIdentifier = row["StirrerIdentifier"].ToString(),
                        DosingPumpUsage = row["DosingPumpUsage"].ToString(),
                        ChemicalDatabaseIdentifier = row["ChemicalDatabaseIdentifier"].ToString(),
                        ImageOfReaction = Encoding.ASCII.GetBytes(row["ImageOfReaction"].ToString()),
                        Comments = row["Comments"].ToString(),
                        State = row["State"].ToString() != null ? (BatchState)Enum.Parse(typeof(BatchState), row["State"].ToString()) : BatchState.NoBatch,
                        TimeStarted = DateTime.TryParse(row["TimeStarted"].ToString(), out DateTime timeStarted) ? DateTime.Parse(row["TimeStarted"].ToString()) : default,
                        TimeCompleted = DateTime.TryParse(row["TimeCompleted"].ToString(), out DateTime timeCompleted) ? DateTime.Parse(row["TimeCompleted"].ToString()) : default,
                    };
        }
    }
}
