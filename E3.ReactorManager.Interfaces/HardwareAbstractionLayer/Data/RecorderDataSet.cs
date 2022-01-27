using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E3.ReactorManager.Interfaces.HardwareAbstractionLayer.Data
{
    public class RecorderDataSet
    {
        /// <summary>
        /// Current Experiment Batch Number
        /// </summary>
        public string BatchNumber
        {
            get;set;
        }

        /// <summary>
        /// Current Experiment Batch Name
        /// </summary>
        public string BatchName
        {
            get;set;
        }

        /// <summary>
        /// Scientist Name of Curretn Experiment
        /// </summary>
        public string Scientist
        {
            get;set;
        }

        /// <summary>
        /// Last Product made on this Reactor
        /// </summary>
        public string LastProductMade
        {
            get;set;
        }

        /// <summary>
        /// Solvent Being Used in the Current Experiment
        /// </summary>
        public string Solvent
        {
            get;set;
        }
    }
}
