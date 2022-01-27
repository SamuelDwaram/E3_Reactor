using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E3.ReactorManager.Interfaces.HardwareAbstractionLayer.Data
{
    public class FieldPointDataReceivedArgs : EventArgs
    {
        /// <summary>
        /// Field Device Identifier
        /// </summary>
        public string FieldDeviceIdentifier
        {
            get;set;
        }

        /// <summary>
        /// Label of the fieldpoint that is changed
        /// </summary>
        public string FieldPointIdentifier
        {
            get;set;
        }

        public string SensorDataSetIdentifier
        {
            get;set;
        }

        /// <summary>
        /// Data type of the field point that is changed
        /// </summary>
        public string FieldPointDataType
        {
            get;set;
        }

        /// <summary>
        /// Describes whether this field point
        /// is alarm field point or (sensor/command)fieldPoint
        /// </summary>
        public string FieldPointType
        {
            get;set;
        }

        /// <summary>
        /// Contains description about field Point
        /// </summary>
        public string FieldPointDescription
        {
            get; set;
        }

        /// <summary>
        /// Current value of the field point which is changed
        /// </summary>
        public string NewFieldPointData
        {
            get;set;
        }
    }
}
