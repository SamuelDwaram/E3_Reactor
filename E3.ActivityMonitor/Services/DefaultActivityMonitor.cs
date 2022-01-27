using System;

namespace E3.ActivityMonitor.Services
{
    public class DefaultActivityMonitor : IActivityMonitor
    {
        public event EventHandler ApplicationIsIdle;

        public void InvokeApplicationIsIdle()
        {
            ApplicationIsIdle?.Invoke(this, null);
        }
    }
}
