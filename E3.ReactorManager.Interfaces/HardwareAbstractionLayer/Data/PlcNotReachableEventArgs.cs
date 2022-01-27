using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E3.ReactorManager.Interfaces.HardwareAbstractionLayer.Data
{
    /// <summary>
    /// FieldDevice Not Reachable Event Args
    /// </summary>
    public class PlcNotReachableEventArgs : EventArgs
    {
        /// <summary>
        /// Label of PLC
        /// </summary>
        public string Label
        {
            set; get;
        }

        /// <summary>
        /// PLC Identifier (AMS Net id )
        /// </summary>
        public string PLCIdentifier
        {
            set; get;
        }
    }
}
