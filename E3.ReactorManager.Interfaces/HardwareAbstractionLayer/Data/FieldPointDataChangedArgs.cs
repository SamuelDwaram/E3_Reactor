using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E3.ReactorManager.Interfaces.HardwareAbstractionLayer.Data
{
    public class FieldPointDataChangedArgs
    {
        public string FieldDeviceIdentifier;
        public string FieldPointIdentifier;
        public string FieldPointDataType;
        public string NewFieldPointData;
    }
}
