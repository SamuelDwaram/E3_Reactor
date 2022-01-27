namespace E3.ReactorManager.Interfaces.DataAbstractionLayer
{
    /// <summary>
    /// Event Type
    /// </summary>
    public enum EventType
    {
        DeviceDataRead,
        LiveTrendsAccessed,
        ReportsAccessed,
        TrendsAccessed,
        AlarmsAccessed,
        AlarmAcknowledged,
        AlarmDismissed,
        AlarmsRaised,
        SavedAlarmLimits,
        ReadAlarmDetails,
        SaveReactorImage,
        CreatedUser,
        AllUsersDetailsAccessed,
        ModifyPassword,
        ModifyAccessLevel,
        EnabledUser,
        DisabledUser,
        ReadUserData,
        CheckUserStatus,
        CheckSameUsername,
        Login,
        InvalidLogin,
        Logout,
        StartedRamp,
        EndedRamp,
        SwitchedOn,
        SwitchedOff,
        ChangedSetPoint,
        EmergencyRaised,
        NotRecognized,
        SaveActionComments,
        UserModified,
        CreatedBatch,
        EndedBatch,
        BatchDataAccessed,
        ListAllRunningBatches,  //lists only running batches
        ListAllBatches,         //lists all the batches both completed and running
        RunningBatchesAccessed,
        ActionCommentsAccessed,
        LatestActionCommentsAccessed,
        ReactorImagesAccessed,
        AuditLog,
        AuditReportAccessed,
        AuditTrailAccessed,
        AutoLogout,             //If the Application has logged out automatically
        SystemShutDown,         //If the Application is shut down
    }
}