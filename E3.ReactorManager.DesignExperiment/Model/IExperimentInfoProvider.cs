using E3.ReactorManager.DesignExperiment.Model.Data;
using System.Collections.Generic;

namespace E3.ReactorManager.DesignExperiment.Model
{
    public interface IExperimentInfoProvider
    {
        /// <summary>
        /// Returns all the completed batches
        /// </summary>
        /// <returns></returns>
        IList<Batch> GetAllCompletedBatches();

        /// <summary>
        /// Returns batch info using batch identifier
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        Batch GetBatchInfo(string batchId);

        /// <summary>
        /// Returns the batch details with the given name
        /// </summary>
        /// <param name="batchName"></param>
        /// <returns></returns>
        Batch GetBatchDetailsByName(string batchName);

        /// <summary>
        /// Returns all the running batches in the plant
        /// </summary>
        /// <returns></returns>
        IList<Batch> GetAllRunningBatchesInThePlant();

        /// <summary>
        /// Returns info of batch running in FieldDevice using device Identifier
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        Batch GetBatchRunningInDevice(string deviceId);
    }
}
