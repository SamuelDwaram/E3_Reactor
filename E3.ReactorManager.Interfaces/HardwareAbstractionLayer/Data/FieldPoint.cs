namespace E3.ReactorManager.Interfaces.HardwareAbstractionLayer
{
    /// <summary>
    /// Individual Field Point
    /// </summary>
    public class FieldPoint
    {
        /// <summary>
        /// Identifier of Source Controller to which field point is connected
        /// </summary>
        public string SourceControllerIdentifier { get; set; }

        /// <summary>
        /// FieldPoint Label(Identifier)
        /// </summary>
        public string Label
        {
            get; set;
        }

        /// <summary>
        /// Name of the handle to memory address of field point
        /// </summary>
        public int PLCHandle { set; get; }

        /// <summary>
        /// Type of Address
        /// </summary>
        public string TypeOfAddress { set; get; }

        /// <summary>
        /// Variable name of fieldPoint 
        ///     in the PLC
        /// </summary>
        public string MemoryAddress
        {
            set; get;
        }

        /// <summary>
        /// Parameter Point Description
        /// </summary>
        public string Description
        {
            set; get;
        }

        /// <summary>
        /// Field Point Data Type
        /// </summary>
        public string FieldPointDataType { set; get; }

        /// <summary>
        /// Parameter Value
        /// </summary>
        public string Value
        {
            set; get;
        }

        /// <summary>
        /// Command Value
        /// </summary>
        public string CommandValue
        {
            get; set;
        }

        /// <summary>
        /// Maximum limit for the field point value
        /// </summary>
        public string MaxValue { get; set; }

        /// <summary>
        /// Offset by which field point value has to be moved
        /// </summary>
        public string Offset { get; set; }

        /// <summary>
        /// Factor by which field point value has to be multiplied
        /// </summary>
        public string Multiplier { get; set; }

        /// <summary>
        /// it will be true if the field point value is changed
        /// </summary>
        public bool ValueChanged;

        /// <summary>
        /// Connection Status Of FieldPoint
        ///     True(Responding) or False(Not Responding)
        /// </summary>
        public bool ConnectionStatus
        {
            get;set;
        }

        /// <summary>
        /// Used as a handle for getting Notifications from PLC
        /// if this value changes
        /// </summary>
        public int NotificationHandle
        {
            get;set;
        }

        /// <summary>
        /// Tells whether this field point requires
        /// Notification Service in the PLC
        /// </summary>
        public bool RequireNotificationService
        {
            get;set;
        }

        /// <summary>
        /// Tells whether this fieldPoint value 
        /// is to be logged in database 
        /// </summary>
        public bool ToBeLogged
        {
            get;set;
        }

        /// <summary>
        /// Id of the SensorsDataSet to which fieldPoint belongs to
        /// </summary>
        public string SensorDataSetIdentifier
        {
            get;set;
        }

        #region Failed Read Operation Status Handlers
        public int FailedReadOperationStatusCounter { get; set; }

        public void IncreaseFailedReadOperationStatusCounter()
        {
            FailedReadOperationStatusCounter += 1;
        }

        public bool AreMaximumFailedReadOperationsCompleted()
        {
            return FailedReadOperationStatusCounter >= 5;
        }

        public void ResetFailedReadOperationStatusCounter()
        {
            FailedReadOperationStatusCounter = 0;
        }
        #endregion
    }
}