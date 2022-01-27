using E3.AuditTrailManager.Model.Data;
using E3.AuditTrailManager.Model.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace E3.AuditTrailManager.Model
{
    public interface IAuditTrailManager
    {
        event EventHandler UpdateAuditTrailView;

        /// <summary>
        /// Saves Audit Event
        /// </summary>
        /// <param name="eventMessage"></param>
        /// <param name="nameOfUser"></param>
        /// <param name="auditCategory"></param>
        void RecordEventAsync(string eventMessage, string nameOfUser, EventTypeEnum auditCategory);

        Task RecordEventSync(string eventMessage, string nameOfUser, EventTypeEnum auditCategory);

        IList<AuditEvent> GetAuditTrail(DateTime start, DateTime end);

        IList<AuditEvent> GetAuditTrail(bool prevSet = false, bool nextSet = false, DateTime dateTimePoint = default);
    }
}
