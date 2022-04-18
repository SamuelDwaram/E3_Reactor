using E3.ActionComments.Model.Data;
using System;
using System.Collections.Generic;

namespace E3.ActionComments.Model
{
    public interface IActionCommentsHandler
    {
        event EventHandler UpdateActionCommentsView;

        void LogActionComments(string fieldDeviceIdentifier, string comments, string user);

        IList<ActionCommentsInfo> GetActionComments(string fieldDeviceIdentifier, DateTime? startTime = null, DateTime? endTime = null);
    }
}
