using E3.ReactorManager.Interfaces.HardwareAbstractionLayer.Data;
using System.Collections.Generic;

namespace E3.ReactorManager.Interfaces.HardwareAbstractionLayer
{
    /// <summary>
    /// Field Device Data
    /// </summary>
    public class FieldDevice
    {
        /// <summary>
        /// Device Name
        /// </summary>
        public string Label
        {
            get; set;
        }

        /// <summary>
        /// Device Type
        /// </summary>
        public string Type
        {
            get; set;
        }

        /// <summary>
        /// Device Identifier/Tag
        /// </summary>
        public string Identifier
        {
            get; set;
        }

        /// <summary>
        /// Plc Identifier
        /// </summary>
        public Plc RelatedPlc { set; get; }

        /// <summary>
        /// Parameters Data 
        /// </summary>
        public IList<SensorsDataSet> SensorsData
        {
            set; get;
        }

        /// <summary>
        /// Command Data 
        /// </summary>
        public IList<FieldPoint> CommandPoints
        {
            set; get;
        } = new List<FieldPoint>();

        /// <summary>
        /// Get Status of field Points
        /// </summary>
        public IList<FieldPoint> FieldPointsStatus
        {
            set; get;
        }
    }

    public class Device
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public string Type { get; set; }
    }
}
