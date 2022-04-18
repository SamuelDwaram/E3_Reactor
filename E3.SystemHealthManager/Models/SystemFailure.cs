using System;
using System.Collections.Generic;
using System.Linq;

namespace E3.SystemHealthManager.Models
{
    public class SystemFailure
    {
        public int Id { get; set; }

        public int SystemFailurePolicyId { get; set; }

        public string DeviceId { get; set; }

        public string DeviceLabel { get; set; }

        public string FailedResourceLabel { get; set; }

        public string Title { get; set; }
        public string TroubleShootMessage { get; set; }

        public FailureState FailureState { get; set; }

        public IEnumerable<FailureState> FailureStateValues
        {
            get => Enum.GetValues(typeof(FailureState)).Cast<FailureState>();
        }

        public IEnumerable<FailureType> FailureTypeValues
        {
            get => Enum.GetValues(typeof(FailureType)).Cast<FailureType>();
        }

        public FailureType FailureType { get; set; }

        public DateTime TimeStamp { get; set; }

        public DateTime RaisedTimeStamp { get; set; }

        public DateTime AcknowledgedTimeStamp { get; set; }
    }

    public enum FailureState
    {
        Raised,
        Acknowledged,
        Resolved
    }

    public enum FailureType
    {
        System,
        Hardware,
    }
}
