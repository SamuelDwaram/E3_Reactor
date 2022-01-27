using System;
using System.Reflection;

namespace E3.ReactorManager.Interfaces.HardwareAbstractionLayer.Data
{
    public class LiveDataEventArgs : EventArgs
    {
        public string PropertyInfoIdentifier { get; set; }

        public PropertyInfo PropertyInfo { get; set; }

        public string LiveData { get; set; }
    }
}
