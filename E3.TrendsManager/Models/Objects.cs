using System;
using System.Collections.Generic;

namespace E3.TrendsManager.Models
{
    public class LiveTrendParametersDataRecord
    {
        public Dictionary<TrendParameter, double> Data { get; set; } = new Dictionary<TrendParameter, double>();
        public DateTime TimeStamp { get; set; }
    }

    public class TrendParameter
    {
        public string Label { get; set; }
        public string FieldPointId { get; set; }
        public string SensorDataSetId { get; set; }

        /// <summary>
        /// Will be of the form => "<Min-Limit>|<Max-Limit>"
        /// If there is not '|' character present string 
        /// then we will take Min-Limit=0 and Max-limit=(value in the string)
        /// </summary>
        public string Limits { get; set; } = string.Empty;

        public string Units { get; set; } = string.Empty;

        public TrendParameterType ParameterType { get; set; } = TrendParameterType.Individual;
        public bool IsLiveTrendParameter { get; set; }
    }

    public class TrendDevice
    {
        public string DeviceId { get; set; }
        public string DeviceLabel { get; set; }
        public IList<TrendParameter> Parameters { get; set; } = new List<TrendParameter>();
    }

    public enum TrendParameterType
    {
        Individual,
        Group
    }
}
