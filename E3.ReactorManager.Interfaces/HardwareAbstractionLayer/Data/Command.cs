using System;

namespace E3.ReactorManager.Interfaces.HardwareAbstractionLayer
{
    /// <summary>
    ///Comamnd Status
    /// </summary>
    public class Command
    {
        /// <summary>
        /// Command Sent Time
        /// </summary>
        public DateTime SentTime { set; get; }

        /// <summary>
        /// Data Type of 
        ///     Command Point(Field Point)
        /// </summary>
        public string DataType { set; get; }

        /// <summary>
        /// Value to be written to PLC
        /// </summary>
        public string WriteValue { set; get; }

        /// <summary>
        /// Field Device Identifier
        /// </summary>
        public string FieldDeviceIdentifier { set; get; }

        /// <summary>
        /// Field Point Identifier
        /// </summary>
        public string FieldPointIdentifier { set; get; }

        /// <summary>
        /// Command In Progress
        /// </summary>
        public bool CommandInProgress { set; get; }
    }
}