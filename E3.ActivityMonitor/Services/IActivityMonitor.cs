using System;

namespace E3.ActivityMonitor.Services
{
    public interface IActivityMonitor
    {
        /// <summary>
        /// Invokes when the Application is Idle
        /// for the time input in the App.config file
        /// </summary>
        event EventHandler ApplicationIsIdle;

        void InvokeApplicationIsIdle();
    }

    public enum ActivityState
    {
        Active,
        Warned,
        Idle
    }
}
