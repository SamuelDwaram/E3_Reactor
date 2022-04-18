using System;
using System.Collections.Generic;
using System.Linq;

namespace E3.SystemAlarmManager.Models
{
    public class SystemAlarmParameters
    {
        public int SystemAlarmPolicyId { get; set; }

        public string Name { get; set; }

        public AlarmParametersType ParametersType { get; set; } = AlarmParametersType.RatedValueVariations;

        public IEnumerable<AlarmParametersType> ParametersTypeValues { get; set; }
            = Enum.GetValues(typeof(AlarmParametersType)).Cast<AlarmParametersType>();

        public float RatedValue { get; set; }

        public float VariationPercentage { get; set; }

        public AlarmParametersVariationType VariationType { get; set; } = AlarmParametersVariationType.Both;

        public IEnumerable<AlarmParametersVariationType> VariationTypeValues { get; set; }
            = Enum.GetValues(typeof(AlarmParametersVariationType)).Cast<AlarmParametersVariationType>();

        public float UpperLimit { get; set; }

        public float LowerLimit { get; set; }
    }

    public enum AlarmParametersType
    {
        Limits,
        RatedValueVariations,
    }

    public enum AlarmParametersVariationType
    {
        Above,
        Below,
        Both
    }
}
