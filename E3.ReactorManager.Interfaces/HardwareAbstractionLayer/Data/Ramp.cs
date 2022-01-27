using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E3.ReactorManager.Interfaces.HardwareAbstractionLayer.Data
{
    public class Ramp
    {
        /// <summary>
        /// to which field device the field point 
        /// is connected
        /// </summary>
        public string FieldDeviceIdentifier { get; set; }

        /// <summary>
        /// to which field point the ramp is being set
        /// </summary>
        public string FieldPointIdentifier { get; set; }

        /// <summary>
        /// RampData for the field point
        /// set of pairs of syntax (timetoStart,toBeAttainedSetPoint)
        /// </summary>
        public List<RampData> RampDataSet { get; set; } = new List<RampData>();
    }
}
