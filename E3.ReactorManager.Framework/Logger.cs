using System;
using E3.ReactorManager.Interfaces.Framework.Logging;
using NLog;

namespace E3.ReactorManager.Framework
{
    /// <summary>
    /// Logger for Application
    /// </summary>
    public class Logger : Interfaces.Framework.Logging.ILogger
    {
        private static readonly NLog.Logger NLogger = LogManager.GetCurrentClassLogger();

        public void Log(LogType logType,string message, Exception exception, params object[] args)
        {
            switch (logType)
            {
                case LogType.Information:
                    NLogger.Info(message);
                    break;
                case LogType.Debug:
                    NLogger.Debug(message);
                    break;
                case LogType.Fatal:
                    NLogger.Fatal(exception, message);
                    break;
                case LogType.Trace:
                    NLogger.Trace(exception, message);
                    break;
                case LogType.Error:
                    NLogger.Error(exception, message, args);
                    break;
                case LogType.Warning:
                    NLogger.Warn(exception, message);
                    break;
                case LogType.MethodEntry:
                    break;
                case LogType.MethodExit:
                    break;
                case LogType.Exception:
                    NLogger.Error(exception, message);
                    break;
                case LogType.QueueCall:
                    break;
                case LogType.DatabaseCall:
                    NLogger.Info(message);
                    break;
                case LogType.HardwareDataReceived:
                    break;
                case LogType.HardwareCommandSent:
                    break;
                default:
                    break;
            }
        }
    }
}
