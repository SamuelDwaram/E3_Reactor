using System;

namespace E3.SystemAlarmManager.Models
{
    public class SystemAlarmPolicy : ICloneable
    {
        public int Id { get; set; }

        public string DeviceId { get; set; }

        public string DeviceLabel { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public SystemAlarmParameters Parameters { get; set; }

        public PolicyType PolicyType { get; set; }

        public bool Status { get; set; }

        public DateTime CreatedTimeStamp { get; set; }

        public object Clone()
        {
            return new SystemAlarmPolicy
            {
                Id = this.Id,
                DeviceId = this.DeviceId,
                DeviceLabel = this.DeviceLabel,
                Title = this.Title,
                Message = this.Message,
                Parameters = this.Parameters,
                PolicyType = this.PolicyType,
                Status = this.Status,
                CreatedTimeStamp = this.CreatedTimeStamp
            };
        }
    }

    public enum PolicyType
    {
        Individual,
        Group,
    }
}
