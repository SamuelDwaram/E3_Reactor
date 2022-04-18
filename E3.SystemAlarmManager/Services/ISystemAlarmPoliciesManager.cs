using System.Collections.Generic;
using E3.SystemAlarmManager.Models;

namespace E3.SystemAlarmManager.Services
{
    public interface ISystemAlarmPoliciesManager
    {
        void AddSystemAlarmPolicy(SystemAlarmPolicy systemAlarmPolicy);

        void DeleteSystemAlarmPolicy(int policyId);

        void UpdateSystemAlarmPolicy(SystemAlarmPolicy systemAlarmPolicy);

        void ModifyAlarmPolicyStatus(int policyId, bool status);

        IEnumerable<SystemAlarmPolicy> GetAll();

        IEnumerable<SystemAlarmPolicy> GetPoliciesForDevice(string deviceId);

        event RefreshSystemAlarmPoliciesEventHandler RefreshSystemAlarmPolicies;
    }

    public delegate void RefreshSystemAlarmPoliciesEventHandler(IList<SystemAlarmPolicy> systemAlarmPolicies, string deviceId = null);
}
