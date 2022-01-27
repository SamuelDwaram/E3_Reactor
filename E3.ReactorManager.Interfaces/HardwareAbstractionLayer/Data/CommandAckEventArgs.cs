using System;

namespace E3.ReactorManager.Interfaces.HardwareAbstractionLayer
{
    /// <summary>
    /// COmmand Acknowledgement Received 
    /// Event Args
    /// </summary>
    public class CommandAckEventArgs : EventArgs
    {
        /// <summary>
        /// Execution Status of Command
        ///     True(executed successfully) or False(execution failed)
        /// </summary>
        public bool ExecutionStatus { get; set; }
        
        /// <summary>
        /// Value written in the Command
        /// </summary>
        public string WriteValue { get; set; }

        /// <summary>
        /// Field Device Identifier
        /// </summary>
        public string FieldDeviceIdentifier { get; set; }
        
        /// <summary>
        /// Field Point Identifier
        /// </summary>
        public string FieldPointIdentifier { get; set; }
        
    }
}