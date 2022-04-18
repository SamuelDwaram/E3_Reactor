using E3.SystemHealthManager.Models;
using System.Collections.Generic;

namespace E3.SystemHealthManager.Services
{
    public interface ISystemFailuresManager
    {
        void Acknowledge(int FailureId);

        IEnumerable<SystemFailure> GetSystemFailuresForDevice(string deviceId);

        IEnumerable<SystemFailure> GetAll();

        event RefreshSystemFailuresEventHandler RefreshSystemFailures;
    }

    public delegate void RefreshSystemFailuresEventHandler(IEnumerable<SystemFailure> systemFailures);
}
