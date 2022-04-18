using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace E3.SystemHealthManager.Models
{
    public class SystemFailurePolicy : ICloneable
    {
        public int Id { get; set; }

        public string DeviceId { get; set; }

        public string DeviceLabel { get; set; }

        /// <summary>
        /// Field Point label
        /// </summary>
        public string FailedResourceLabel { get; set; }

        public string TargetValue { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string TroubleShootMessage { get; set; } = string.Empty;

        public FailureResourceType FailureResourceType { get; set; }

        public IEnumerable<FailureResourceType> FailureResourceTypeValues { get; set; }
            = Enum.GetValues(typeof(FailureResourceType)).Cast<FailureResourceType>();

        public bool Status { get; set; }

        public DateTime CreatedTimeStamp { get; set; }

        public object Clone()
        {
            return new SystemFailurePolicy
            {
                Id = this.Id,
                DeviceId = this.DeviceId,
                DeviceLabel = this.DeviceLabel,
                FailedResourceLabel = this.FailedResourceLabel,
                Title = this.Title,
                Message = this.Message,
                TroubleShootMessage = this.TroubleShootMessage,
                FailureResourceType = this.FailureResourceType,
                Status = this.Status,
                CreatedTimeStamp = this.CreatedTimeStamp
            };
        }
    }

    public enum FailureResourceType
    {
        Device,
        Controller
    }
}
