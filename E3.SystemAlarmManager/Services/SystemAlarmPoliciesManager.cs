using E3.ReactorManager.Interfaces.DataAbstractionLayer;
using E3.ReactorManager.Interfaces.DataAbstractionLayer.Data;
using E3.SystemAlarmManager.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace E3.SystemAlarmManager.Services
{
    public class SystemAlarmPoliciesManager : ISystemAlarmPoliciesManager
    {
        private readonly IDatabaseReader databaseReader;
        private readonly IDatabaseWriter databaseWriter;
        private IList<SystemAlarmPolicy> systemAlarmPolicies = new List<SystemAlarmPolicy>();

        public SystemAlarmPoliciesManager(IDatabaseReader databaseReader, IDatabaseWriter databaseWriter)
        {
            this.databaseReader = databaseReader;
            this.databaseWriter = databaseWriter;

            LoadSystemAlarmPolicies();
        }

        private void LoadSystemAlarmPolicies()
        {
            systemAlarmPolicies = (from DataRow row in databaseReader.ExecuteReadCommand("GetAllSystemAlarmPolicies", CommandType.StoredProcedure).AsEnumerable()
                    select new SystemAlarmPolicy
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        DeviceId = Convert.ToString(row["DeviceId"]),
                        DeviceLabel = Convert.ToString(row["DeviceLabel"]),
                        PolicyType = (PolicyType)Enum.Parse(typeof(PolicyType), Convert.ToString(row["PolicyType"])),
                        Title = Convert.ToString(row["Title"]),
                        Message = Convert.ToString(row["Message"]),
                        Status = Convert.ToBoolean(row["Status"]),
                        CreatedTimeStamp = Convert.ToDateTime(row["CreatedTimeStamp"]),
                        Parameters = new SystemAlarmParameters
                        {
                            SystemAlarmPolicyId = Convert.ToInt32(row["Id"]),
                            Name = Convert.ToString(row["Name"]),
                            RatedValue = Convert.ToSingle(row["RatedValue"]),
                            VariationPercentage = Convert.ToSingle(row["VariationPercentage"]),
                            VariationType = (AlarmParametersVariationType)Enum.Parse(typeof(AlarmParametersVariationType), Convert.ToString(row["VariationType"])),
                            ParametersType = (AlarmParametersType)Enum.Parse(typeof(AlarmParametersType), Convert.ToString(row["ParametersType"])),
                            UpperLimit = Convert.ToSingle(row["UpperLimit"]),
                            LowerLimit = Convert.ToSingle(row["LowerLimit"])
                        }
                    }).ToList();
        }

        public event RefreshSystemAlarmPoliciesEventHandler RefreshSystemAlarmPolicies;

        public void AddSystemAlarmPolicy(SystemAlarmPolicy systemAlarmPolicy)
        {
            if (systemAlarmPolicies.Any(p => p.Id == systemAlarmPolicy.Id))
            {
                //Update the System Alarm Policy If it already exists
                UpdateSystemAlarmPolicy(systemAlarmPolicy);
            }
            else
            {
                IList<DbParameterInfo> dbParameters = new List<DbParameterInfo>
                {
                    new DbParameterInfo("@DeviceId", systemAlarmPolicy.DeviceId, DbType.String),
                    new DbParameterInfo("@DeviceLabel", systemAlarmPolicy.DeviceLabel, DbType.String),
                    new DbParameterInfo("@PolicyType", systemAlarmPolicy.PolicyType, DbType.String),
                    new DbParameterInfo("@Title", systemAlarmPolicy.Title, DbType.String),
                    new DbParameterInfo("@Message", systemAlarmPolicy.Message, DbType.String),
                    new DbParameterInfo("@Status", systemAlarmPolicy.Status, DbType.Boolean),
                    new DbParameterInfo("@ParameterName", systemAlarmPolicy.Parameters.Name, DbType.String),
                    new DbParameterInfo("@ParametersType", systemAlarmPolicy.Parameters.ParametersType, DbType.String),
                    new DbParameterInfo("@RatedValue", systemAlarmPolicy.Parameters.RatedValue, DbType.String),
                    new DbParameterInfo("@VariationPercentage", systemAlarmPolicy.Parameters.VariationPercentage, DbType.String),
                    new DbParameterInfo("@VariationType", systemAlarmPolicy.Parameters.VariationType, DbType.String),
                    new DbParameterInfo("@UpperLimit", systemAlarmPolicy.Parameters.UpperLimit, DbType.String),
                    new DbParameterInfo("@LowerLimit", systemAlarmPolicy.Parameters.LowerLimit, DbType.String),
                };
                databaseWriter.ExecuteWriteCommand("InsertSystemAlarmPolicy", CommandType.StoredProcedure, dbParameters);
                LoadSystemAlarmPolicies();
                PublishRefreshSystemAlarmPoliciesEvent(systemAlarmPolicies, systemAlarmPolicy.DeviceId);
            }
        }

        private void PublishRefreshSystemAlarmPoliciesEvent(IList<SystemAlarmPolicy> systemAlarmPolicies, string deviceId = null)
        {
            if (RefreshSystemAlarmPolicies == null)
            {
                //No Subscribers for this event
            }
            else
            {
                foreach (Delegate receiver in RefreshSystemAlarmPolicies.GetInvocationList())
                {
                    ((RefreshSystemAlarmPoliciesEventHandler)receiver).BeginInvoke(systemAlarmPolicies, deviceId, null, null);
                }
            }
        }

        public void DeleteSystemAlarmPolicy(int policyId)
        {
            string deviceId = systemAlarmPolicies.First(p => p.Id == policyId).DeviceId;
            IList<DbParameterInfo> dbParameters = new List<DbParameterInfo>
            {
                new DbParameterInfo("Id", policyId, DbType.Int32)
            };
            databaseWriter.ExecuteWriteCommand("DeleteSystemAlarmPolicy", CommandType.StoredProcedure, dbParameters);
            systemAlarmPolicies.ToList().RemoveAll(p => p.Id == policyId);
            PublishRefreshSystemAlarmPoliciesEvent(systemAlarmPolicies, deviceId);
        }

        public void UpdateSystemAlarmPolicy(SystemAlarmPolicy systemAlarmPolicy)
        {
            IList<DbParameterInfo> dbParameters = new List<DbParameterInfo>
            {
                new DbParameterInfo("@Id", systemAlarmPolicy.Id, DbType.Int32),
                new DbParameterInfo("@PolicyType", systemAlarmPolicy.PolicyType, DbType.String),
                new DbParameterInfo("@Title", systemAlarmPolicy.Title, DbType.String),
                new DbParameterInfo("@Message", systemAlarmPolicy.Message, DbType.String),
                new DbParameterInfo("@Status", systemAlarmPolicy.Status, DbType.Boolean),
                new DbParameterInfo("@ParametersType", systemAlarmPolicy.Parameters.ParametersType, DbType.String),
                new DbParameterInfo("@RatedValue", systemAlarmPolicy.Parameters.RatedValue, DbType.String),
                new DbParameterInfo("@VariationPercentage", systemAlarmPolicy.Parameters.VariationPercentage, DbType.String),
                new DbParameterInfo("@VariationType", systemAlarmPolicy.Parameters.VariationType, DbType.String),
                new DbParameterInfo("@UpperLimit", systemAlarmPolicy.Parameters.UpperLimit, DbType.String),
                new DbParameterInfo("@LowerLimit", systemAlarmPolicy.Parameters.LowerLimit, DbType.String),
            };
            databaseWriter.ExecuteWriteCommand("UpdateSystemAlarmPolicyAndAlarmParameters", CommandType.StoredProcedure, dbParameters);
            SystemAlarmPolicy existingPolicy = systemAlarmPolicies.ToList().Find(p => p.Id == systemAlarmPolicy.Id);
            existingPolicy.Title = systemAlarmPolicy.Title;
            existingPolicy.Message = systemAlarmPolicy.Message;
            existingPolicy.Status = systemAlarmPolicy.Status;
            existingPolicy.Parameters = systemAlarmPolicy.Parameters;
            PublishRefreshSystemAlarmPoliciesEvent(systemAlarmPolicies, systemAlarmPolicy.DeviceId);
        }

        public IEnumerable<SystemAlarmPolicy> GetAll()
        {
            return systemAlarmPolicies;
        }

        public IEnumerable<SystemAlarmPolicy> GetPoliciesForDevice(string deviceId)
        {
            return systemAlarmPolicies.Where(p => p.DeviceId == deviceId);
        }

        public void ModifyAlarmPolicyStatus(int policyId, bool status)
        {
            SystemAlarmPolicy systemAlarmPolicy = systemAlarmPolicies.First(p => p.Id == policyId);
            systemAlarmPolicy.Status = status;
            UpdateSystemAlarmPolicy(systemAlarmPolicy);
        }
    }
}
