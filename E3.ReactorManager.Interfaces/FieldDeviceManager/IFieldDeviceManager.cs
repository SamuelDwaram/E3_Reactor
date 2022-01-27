using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;

namespace E3.ReactorManager.Interfaces.FieldDeviceManager
{
    /// <summary>
    /// Field Device Manager
    /// </summary>
    public interface IFieldDeviceManager
    {
        /// <summary>
        /// Event to be raised when Field Device button pressed
        /// </summary>

        event EventHandler<DeviceCommandRaisedEventArgs> DeviceCommandRaised;

        /// <summary>
        /// Add Field Device to Persistance
        /// like Data Base
        /// </summary>
        /// <param name="device"></param>
        void AddFieldDevice(FieldDevice device);

    }
}
