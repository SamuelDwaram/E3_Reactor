using System;

namespace E3.ReactorManager.Interfaces.FieldDeviceManager
{
    /// <summary>
    /// DEvice Command Raised Event Args
    /// </summary>
    public class DeviceCommandRaisedEventArgs : EventArgs
    {
        /// <summary>
        /// Device Identifier
        /// </summary>
        public string DeviceIdentifier { get; set; }

        /// <summary>
        /// Field Point Identifier
        /// </summary>
        public string FieldPointIdentifier { set; get; }
    }
}