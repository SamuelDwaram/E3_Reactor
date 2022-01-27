using System.Collections.Generic;

namespace E3.ReactorManager.Interfaces.HardwareAbstractionLayer
{
    /// <summary>
    /// Class of Parameter Data
    /// </summary>
    public class SensorsDataSet
    {
        /// <summary>
        /// Parameter Name
        /// </summary>
        public string Label
        {
            set; get;
        }

        /// <summary>
        /// Parameter Unit
        /// </summary>
        public string DataUnit
        {
            set; get;
        }

        /// <summary>
        /// Parameter Identifier
        /// </summary>
        public string Identifier
        {
            set; get;
        }

        /// <summary>
        /// Sensor Set
        /// </summary>
        public IList<FieldPoint> SensorsFieldPoints
        {
            set; get;
        }
    }
}