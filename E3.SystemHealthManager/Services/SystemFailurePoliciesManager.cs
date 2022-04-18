using E3.ReactorManager.Interfaces.DataAbstractionLayer;
using E3.ReactorManager.Interfaces.DataAbstractionLayer.Data;
using E3.SystemHealthManager.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace E3.SystemHealthManager.Services
{
    public class SystemFailurePoliciesManager : ISystemFailurePoliciesManager
    {
        private readonly IDatabaseReader databaseReader;
        private readonly IDatabaseWriter databaseWriter;
        private IList<SystemFailurePolicy> systemFailurePolicies = new List<SystemFailurePolicy>();

        public SystemFailurePoliciesManager(IDatabaseReader databaseReader, IDatabaseWriter databaseWriter)
        {
            this.databaseReader = databaseReader;
            this.databaseWriter = databaseWriter;

            LoadSystemFailurePolicies();
        }

        private void LoadSystemFailurePolicies()
        {
            systemFailurePolicies = (from DataRow row in databaseReader.ExecuteReadCommand("GetAllSystemFailurePolicies", CommandType.StoredProcedure).AsEnumerable()
                    select new SystemFailurePolicy
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        DeviceId = Convert.ToString(row["DeviceId"]),
                        DeviceLabel = Convert.ToString(row["DeviceLabel"]),
                        FailedResourceLabel = Convert.ToString(row["FailedResourceLabel"]),
                        TargetValue = Convert.ToString(row["TargetValue"]),
                        Title = Convert.ToString(row["Title"]),
                        Message = Convert.ToString(row["Message"]),
                        TroubleShootMessage = Convert.ToString(row["TroubleShootMessage"]),
                        FailureResourceType = (FailureResourceType)Enum.Parse(typeof(FailureResourceType), Convert.ToString(row["FailureResourceType"])),
                        Status = Convert.ToBoolean(row["Status"]),
                        CreatedTimeStamp = Convert.ToDateTime(row["CreatedTimeStamp"]),
                    }).ToList();
        }

        public event RefreshSystemFailurePoliciesEventHandler RefreshSystemFailurePolicies;

        public void AddSystemFailurePolicy(SystemFailurePolicy systemFailurePolicy)
        {
            if (systemFailurePolicies.Any(p => p.Id == systemFailurePolicy.Id))
            {
                //Update the System Failure Policy If it already exists
                UpdateSystemFailurePolicy(systemFailurePolicy);
            }
            else
            {
                IList<DbParameterInfo> dbParameters = new List<DbParameterInfo>
                {
                    new DbParameterInfo("@DeviceId", systemFailurePolicy.DeviceId, DbType.String),
                    new DbParameterInfo("@DeviceLabel", systemFailurePolicy.DeviceLabel, DbType.String),
                    new DbParameterInfo("@FailedResourceLabel", systemFailurePolicy.FailedResourceLabel, DbType.String),
                    new DbParameterInfo("@TargetValue", systemFailurePolicy.TargetValue, DbType.String),
                    new DbParameterInfo("@Title", systemFailurePolicy.Title, DbType.String),
                    new DbParameterInfo("@Message", systemFailurePolicy.Message, DbType.String),
                    new DbParameterInfo("@TroubleShootMessage", systemFailurePolicy.TroubleShootMessage, DbType.String),
                    new DbParameterInfo("@FailureResourceType", systemFailurePolicy.FailureResourceType, DbType.String),
                    new DbParameterInfo("@Status", systemFailurePolicy.Status, DbType.Boolean),
                    new DbParameterInfo("@CreatedTimeStamp", DateTime.Now, DbType.DateTime),
                };
                databaseWriter.ExecuteWriteCommand("InsertSystemFailurePolicy", CommandType.StoredProcedure, dbParameters);
                LoadSystemFailurePolicies();
                PublishRefreshSystemFailurePoliciesEvent(systemFailurePolicies, systemFailurePolicy.DeviceId);
            }
        }

        private void PublishRefreshSystemFailurePoliciesEvent(IList<SystemFailurePolicy> systemFailurePolicies, string deviceId = null)
        {
            if (RefreshSystemFailurePolicies == null)
            {
                //No Subscribers for this event
            }
            else
            {
                foreach (Delegate receiver in RefreshSystemFailurePolicies.GetInvocationList())
                {
                    ((RefreshSystemFailurePoliciesEventHandler)receiver).BeginInvoke(systemFailurePolicies, deviceId, null, null);
                }
            }
        }

        public void DeleteSystemFailurePolicy(int policyId)
        {
            string deviceId = systemFailurePolicies.First(p => p.Id == policyId).DeviceId;
            IList<DbParameterInfo> dbParameters = new List<DbParameterInfo>
            {
                new DbParameterInfo("Id", policyId, DbType.Int32)
            };
            databaseWriter.ExecuteWriteCommand("DeleteSystemFailurePolicy", CommandType.StoredProcedure, dbParameters);
            systemFailurePolicies.ToList().RemoveAll(p => p.Id == policyId);
            PublishRefreshSystemFailurePoliciesEvent(systemFailurePolicies, deviceId);
        }

        public void UpdateSystemFailurePolicy(SystemFailurePolicy systemFailurePolicy)
        {
            IList<DbParameterInfo> dbParameters = new List<DbParameterInfo>
            {
                new DbParameterInfo("@Id", systemFailurePolicy.Id, DbType.Int32),
                new DbParameterInfo("@TargetValue", systemFailurePolicy.TargetValue, DbType.String),
                new DbParameterInfo("@Title", systemFailurePolicy.Title, DbType.String),
                new DbParameterInfo("@Message", systemFailurePolicy.Message, DbType.String),
                new DbParameterInfo("@TroubleShootMessage", systemFailurePolicy.TroubleShootMessage, DbType.String),
                new DbParameterInfo("@FailureResourceType", systemFailurePolicy.FailureResourceType, DbType.String),
                new DbParameterInfo("@Status", systemFailurePolicy.Status, DbType.Boolean),
            };
            databaseWriter.ExecuteWriteCommand("UpdateSystemFailurePolicy", CommandType.StoredProcedure, dbParameters);
            SystemFailurePolicy existingPolicy = systemFailurePolicies.ToList().Find(p => p.Id == systemFailurePolicy.Id);
            existingPolicy.FailedResourceLabel = systemFailurePolicy.FailedResourceLabel;
            existingPolicy.TargetValue = systemFailurePolicy.TargetValue;
            existingPolicy.Title = systemFailurePolicy.Title;
            existingPolicy.Message = systemFailurePolicy.Message;
            existingPolicy.TroubleShootMessage = systemFailurePolicy.TroubleShootMessage;
            existingPolicy.FailureResourceType = systemFailurePolicy.FailureResourceType;
            existingPolicy.Status = systemFailurePolicy.Status;
            PublishRefreshSystemFailurePoliciesEvent(systemFailurePolicies, systemFailurePolicy.DeviceId);
        }

        public IEnumerable<SystemFailurePolicy> GetAll()
        {
            return systemFailurePolicies;
        }

        public IEnumerable<SystemFailurePolicy> GetPoliciesForDevice(string deviceId)
        {
            return systemFailurePolicies.Where(p => p.DeviceId == deviceId);
        }

        public void ModifyFailurePolicyStatus(int policyId, bool status)
        {
            SystemFailurePolicy systemFailurePolicy = systemFailurePolicies.First(p => p.Id == policyId);
            systemFailurePolicy.Status = status;
            UpdateSystemFailurePolicy(systemFailurePolicy);
        }

        public SystemFailurePolicy GetSystemFailurePolicy(int policyId)
        {
            return systemFailurePolicies.First(p => p.Id == policyId);
        }
    }
}
