using System.Collections.Generic;

namespace E3.ReactorManager.Interfaces.HardwareAbstractionLayer.Data
{
    public class DataReadFromController
    {
        /// <summary>
        /// Identifier of Controller in the System
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Label of Controller in the System
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Address of Controller in the System
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Provider Name of Controller
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Data read from Controller from its Used MemoryAddresses in the following form
        /// <MemoryAddress, DataReadFromThatAddress>
        /// </summary>
        public Dictionary<string, ushort> Data { get; set; }

        /// <summary>
        /// Indicates Read Operation successful or failure
        /// </summary>
        public bool ReadOperationStatus { get; set; }
    }
}
