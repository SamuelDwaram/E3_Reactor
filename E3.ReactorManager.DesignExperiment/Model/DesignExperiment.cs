using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using E3.AuditTrailManager.Model;
using E3.AuditTrailManager.Model.Enums;
using E3.ReactorManager.DesignExperiment.Model.Data;
using E3.ReactorManager.Interfaces.DataAbstractionLayer;
using E3.ReactorManager.Interfaces.DataAbstractionLayer.Data;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;
using E3.UserManager.Model.Data;
using E3.UserManager.Model.Interfaces;
using Unity;

namespace E3.ReactorManager.DesignExperiment.Model
{
    public class DesignExperiment : IDesignExperiment
    {
        private readonly IDatabaseWriter databaseWriter;
        private readonly IDatabaseReader databaseReader;
        private readonly IExperimentInfoProvider experimentInfoProvider;
        private readonly IFieldDevicesCommunicator fieldDevicesCommunicator;
        private readonly IAuditTrailManager auditTrailManager;
        private readonly IUserManager userManager;

        public DesignExperiment(IDatabaseReader databaseReader, IDatabaseWriter databaseWriter, IExperimentInfoProvider experimentInfoProvider, IFieldDevicesCommunicator fieldDevicesCommunicator, IAuditTrailManager auditTrailManager, IUserManager userManager)
        {
            this.databaseReader = databaseReader;
            this.databaseWriter = databaseWriter;
            this.experimentInfoProvider = experimentInfoProvider;
            this.fieldDevicesCommunicator = fieldDevicesCommunicator;
            this.auditTrailManager = auditTrailManager;
            this.userManager = userManager;

            //Update the controller with the running batches after 15 seconds giving time for the hardware layer to initialize
            Task.Factory.StartNew(new Action(() => Thread.Sleep(15000)))
                .ContinueWith(t => SyncBatchesWithController());
        }

        private void SyncBatchesWithController()
        {
            experimentInfoProvider.GetAllRunningBatchesInThePlant().ToList()
                .ForEach(b =>
                {
                    fieldDevicesCommunicator.SendCommandToDevice(b.FieldDeviceIdentifier, "RunningBatchStatus", "bool", bool.TrueString);
                });
        }

        public bool EndBatch(Credential adminCredential, string batchIdentifier, string batchName, string deviceIdContainingBatch, string cleaningSolvent)
        {
            var name = ValidateCredentials(adminCredential);
            if (!string.IsNullOrWhiteSpace(name))
            {
                UpdateBatchStatus(batchIdentifier, batchName, deviceIdContainingBatch, name, cleaningSolvent);

                return true;
            }

            return false;
        }
        public bool EndBatchCompact(Credential adminCredential, string batchIdentifier, string batchName, string deviceIdContainingBatch)
        {
            var name = ValidateCredentials(adminCredential);
            if (!string.IsNullOrWhiteSpace(name))
            {
                UpdateBatchStatusCompact(batchIdentifier, batchName, deviceIdContainingBatch);

                return true;
            }

            return false;
        }

        private void UpdateBatchStatusCompact(string batchIdentifier, string batchName, string deviceIdContainingBatch)
        {
            databaseWriter.EndBatchCompact(batchIdentifier);
            auditTrailManager.RecordEventAsync(" ended Batch " + batchName + " in " + fieldDevicesCommunicator.GetFieldDeviceLabel(deviceIdContainingBatch), "user", EventTypeEnum.Batch);
            fieldDevicesCommunicator.SendCommandToDevice(deviceIdContainingBatch, "RunningBatchStatus", "bool", bool.FalseString);
        }
        private void UpdateBatchStatus(string batchIdentifier, string batchName, string deviceIdContainingBatch, string cleanedBy, string cleaningSolvent)
        {
            databaseWriter.EndBatch(batchIdentifier, cleanedBy, cleaningSolvent);
            auditTrailManager.RecordEventAsync(cleanedBy + " ended Batch " + batchName + " in " + fieldDevicesCommunicator.GetFieldDeviceLabel(deviceIdContainingBatch), cleanedBy, EventTypeEnum.Batch);
            fieldDevicesCommunicator.SendCommandToDevice(deviceIdContainingBatch, "RunningBatchStatus", "bool", bool.FalseString);
        }
      

        
        private string ValidateCredentials(Credential adminCredential)
        {
            User userDetails = userManager.AuthenticateCredential(adminCredential);

            if (userDetails != null)
            {
                if (userDetails.Roles.Any(role => role.ModulesAccessable.Any(module => module == "DesignExperiment")))
                {
                    return userDetails.Name;
                }
            }

            return string.Empty;
        }

        public Dictionary<string, string> GetAvailableFieldDevicesForRunningBatch()
        {
            var availableFieldDevices = new Dictionary<string, string>();
            var identifierList = new List<string>(fieldDevicesCommunicator.GetAllFieldDeviceIdentifiers());

            foreach (var fieldDeviceIdentifier in identifierList)
            {
                bool runningBatchStatus = fieldDevicesCommunicator.ReadFieldPointValue<bool>(fieldDeviceIdentifier, "RunningBatchStatus");

                if (runningBatchStatus)
                {
                    /* batch is already started in the Field Device so don't add it to the available field devices */
                }
                else
                {
                    /* add the field device to availableFieldDevices */
                    availableFieldDevices.Add(fieldDeviceIdentifier, fieldDevicesCommunicator.GetFieldDeviceLabel(fieldDeviceIdentifier));
                }
            }

            return availableFieldDevices;
        }

        /// <summary>
        /// saves the batch details into database
        /// </summary>
        /// <param name="currentBatchData"></param>
        public bool StartBatchCompact(Batch currentBatchData)
        {
            if (ValidateBatchCompact(currentBatchData.Name))
            {
                fieldDevicesCommunicator.SendCommandToDevice(currentBatchData.FieldDeviceIdentifier, "BatchName", "string", currentBatchData.Name);
                fieldDevicesCommunicator.SendCommandToDevice(currentBatchData.FieldDeviceIdentifier, "BatchNumber", "string", currentBatchData.Number);
                fieldDevicesCommunicator.SendCommandToDevice(currentBatchData.FieldDeviceIdentifier, "BatchRemarks", "string", currentBatchData.Comments);
                fieldDevicesCommunicator.SendCommandToDevice(currentBatchData.FieldDeviceIdentifier, "BatchStatus", "string", "Running");
                fieldDevicesCommunicator.SendCommandToDevice(currentBatchData.FieldDeviceIdentifier, "BatchScientistName", "string", currentBatchData.ScientistName);
                fieldDevicesCommunicator.SendCommandToDevice(currentBatchData.FieldDeviceIdentifier, "RunningBatchStatus", "bool", bool.TrueString);
                SaveBatchToDatabaseCompact(currentBatchData);
                auditTrailManager.RecordEventAsync(currentBatchData.ScientistName + " created Batch " + currentBatchData.Name + " in " + fieldDevicesCommunicator.GetFieldDeviceLabel(currentBatchData.FieldDeviceIdentifier), currentBatchData.ScientistName, EventTypeEnum.Batch);

                return true;
            }

            return false;
        }
        public bool StartBatch(Batch currentBatchData)
        {
            if (ValidateBatch(currentBatchData.Name, currentBatchData.Number, currentBatchData.Stage))
            {
                fieldDevicesCommunicator.SendCommandToDevice(currentBatchData.FieldDeviceIdentifier, "BatchName", "string", currentBatchData.Name);
                fieldDevicesCommunicator.SendCommandToDevice(currentBatchData.FieldDeviceIdentifier, "BatchNumber", "string", currentBatchData.Number);
                fieldDevicesCommunicator.SendCommandToDevice(currentBatchData.FieldDeviceIdentifier, "BatchRemarks", "string", currentBatchData.Comments);
                fieldDevicesCommunicator.SendCommandToDevice(currentBatchData.FieldDeviceIdentifier, "BatchStatus", "string", "Running");
                fieldDevicesCommunicator.SendCommandToDevice(currentBatchData.FieldDeviceIdentifier, "BatchScientistName", "string", currentBatchData.ScientistName);
                fieldDevicesCommunicator.SendCommandToDevice(currentBatchData.FieldDeviceIdentifier, "RunningBatchStatus", "bool", bool.TrueString);
                SaveBatchToDatabase(currentBatchData);
                auditTrailManager.RecordEventAsync(currentBatchData.ScientistName + " created Batch " + currentBatchData.Name + " in " + fieldDevicesCommunicator.GetFieldDeviceLabel(currentBatchData.FieldDeviceIdentifier), currentBatchData.ScientistName, EventTypeEnum.Batch);

                return true;
            }

            return false;
        }

        private void SaveBatchToDatabaseCompact(Batch batchData)
        {
            IList<DbParameterInfo> parameters = new List<DbParameterInfo>
            {
                new DbParameterInfo("@Identifier", DateTime.Now.ToString("yyyyMMddHHmmssfff"), DbType.String),
                new DbParameterInfo("@Name", batchData.Name, DbType.String),
                new DbParameterInfo("@ScientistName", batchData.ScientistName, DbType.String),
                new DbParameterInfo("@Comments", batchData.Comments, DbType.String),
                new DbParameterInfo("@State", BatchState.Running, DbType.String),
                new DbParameterInfo("@TimeStarted", DateTime.Now, DbType.DateTime),
            };
            databaseWriter.ExecuteWriteCommand("AddBatchCompact", CommandType.StoredProcedure, parameters);
        }
        private void SaveBatchToDatabase(Batch batchData)
        {
            IList<DbParameterInfo> parameters = new List<DbParameterInfo>
            {
                new DbParameterInfo("@Identifier", DateTime.Now.ToString("yyyyMMddHHmmssfff"), DbType.String),
                new DbParameterInfo("@Name", batchData.Name, DbType.String),
                new DbParameterInfo("@Number", batchData.Number, DbType.String),
                new DbParameterInfo("@Stage", batchData.Stage, DbType.String),
                new DbParameterInfo("@ScientistName", batchData.ScientistName, DbType.String),
                new DbParameterInfo("@FieldDeviceIdentifier", batchData.FieldDeviceIdentifier, DbType.String),
                new DbParameterInfo("@FieldDeviceLabel", batchData.FieldDeviceLabel, DbType.String),
                new DbParameterInfo("@HCIdentifier", batchData.HCIdentifier ?? string.Empty, DbType.String),
                new DbParameterInfo("@StirrerIdentifier", batchData.StirrerIdentifier ?? string.Empty, DbType.String),
                new DbParameterInfo("@DosingPumpUsage", batchData.DosingPumpUsage, DbType.String),
                new DbParameterInfo("@ChemicalDatabaseIdentifier", batchData.ChemicalDatabaseIdentifier ?? string.Empty, DbType.String),
                new DbParameterInfo("@ImageOfReaction", batchData.ImageOfReaction ?? new byte[1], DbType.Binary),
                new DbParameterInfo("@Comments", batchData.Comments, DbType.String),
                new DbParameterInfo("@State", BatchState.Running, DbType.String),
                new DbParameterInfo("@TimeStarted", DateTime.Now, DbType.DateTime),
            };
            databaseWriter.ExecuteWriteCommand("AddBatch", CommandType.StoredProcedure, parameters);
        }
        public bool ValidateBatchCompact(string batchName)
        {
            Batch batchData = experimentInfoProvider.GetBatchDetailsByName(batchName);
            if (batchData != null
                && batchData.Name == batchName)
            {
                /*
                 * Return false indicates there is already batch with the given name, number, stage combination
                 * So the batch with the given name, number, stage should not be allowed to start
                 */
                return false;
            }

            return true;
        }
        public bool ValidateBatch(string batchName, string batchNumber, string batchStage)
        {
            Batch batchData = experimentInfoProvider.GetBatchDetailsByName(batchName);
            if (batchData != null
                && batchData.Name == batchName
                && batchData.Number == batchNumber
                && batchData.Stage == batchStage)
            {
                /*
                 * Return false indicates there is already batch with the given name, number, stage combination
                 * So the batch with the given name, number, stage should not be allowed to start
                 */
                return false;
            }
            
            return true;
        }

        public string FetchRunningBatch()
        {
            return databaseReader.FetchRunningBatch();
        }
    }
}
