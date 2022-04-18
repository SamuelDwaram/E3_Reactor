using E3.SystemHealthManager.Models;
using System.Collections.Generic;

namespace E3.SystemHealthManager.Services
{
    public interface ISystemFailurePoliciesManager
    {
        void AddSystemFailurePolicy(SystemFailurePolicy systemFailurePolicy);

        void DeleteSystemFailurePolicy(int policyId);

        void UpdateSystemFailurePolicy(SystemFailurePolicy systemFailurePolicy);

        SystemFailurePolicy GetSystemFailurePolicy(int policyId);

        void ModifyFailurePolicyStatus(int policyId, bool status);

        IEnumerable<SystemFailurePolicy> GetAll();

        IEnumerable<SystemFailurePolicy> GetPoliciesForDevice(string deviceId);

        event RefreshSystemFailurePoliciesEventHandler RefreshSystemFailurePolicies;
    }

    public delegate void RefreshSystemFailurePoliciesEventHandler(IList<SystemFailurePolicy> systemFailurePolicies, string deviceId = null);
}
