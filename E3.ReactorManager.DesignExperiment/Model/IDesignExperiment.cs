using E3.ReactorManager.DesignExperiment.Model.Data;
using E3.UserManager.Model.Data;
using System.Collections.Generic;

namespace E3.ReactorManager.DesignExperiment.Model
{
    public interface IDesignExperiment
    {
        /// <summary>
        /// Returns the dictionary of <FieldDeviceIdentifier, FieldDeviceLabel> of the field device
        /// which is available for running new batch
        /// </summary>
        /// <returns></returns>
        Dictionary<string, string> GetAvailableFieldDevicesForRunningBatch();

        /// <summary>
        /// After locking the controls 
        /// save the batch details in the database
        /// </summary>
        /// <param name="currentBatch"></param>
        bool StartBatchCompact(Batch currentBatchData);
        bool StartBatch(Batch currentBatchData);

        /// <summary>
        /// validates whether the batch with this name, number, stage is allowed to start
        /// </summary>
        /// <param name="batchName"></param>
        /// <returns></returns>
        bool ValidateBatchCompact(string batchName);
        bool ValidateBatch(string batchName, string batchNumber, string batchStage);

        /// <summary>
        /// Ends the batch by validating the given Admin Credentials
        /// </summary>
        /// <param name="adminCredential"></param>
        /// <param name="batchName"></param>
        /// <returns></returns>
        bool EndBatch(Credential adminCredential, string batchIdentifier, string batchName, string deviceIdContainingBatch, string cleaningSolvent);
        bool EndBatchCompact(Credential adminCredential, string batchIdentifier, string batchName, string deviceIdContainingBatch);
        string FetchRunningBatch();
    }
}
