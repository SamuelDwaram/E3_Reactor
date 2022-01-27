using System;

namespace E3.ReactorManager.Interfaces.Framework.Logging
{
    /// <summary>
    /// Contract to log Application Events
    /// </summary>
    public interface ILogger
    {
        void Log(LogType logType, string message, Exception exception = null, params object[] args);
    }

    /// <summary>
    /// Log Type
    /// </summary>
    public enum LogType
    {
        Information,
        Debug,
        Fatal,
        Trace,
        Error,
        Warning,
        MethodEntry,
        MethodExit,
        Exception,
        QueueCall,
        DatabaseCall,
        HardwareDataReceived,
        HardwareCommandSent
    }
}
