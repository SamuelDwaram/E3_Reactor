using E3.ReactorManager.Interfaces.HardwareAbstractionLayer.Data;

namespace E3.ReactorManager.Interfaces.HardwareAbstractionLayer
{
    public interface IRampHandler
    {
        /// <summary>
        /// Sends ramp to PLC
        /// </summary>
        /// <param name="ramp"></param>
        void SendRampToPLC(Ramp ramp);
        /// <summary>
        /// Sends the corresponding changed ramp item to PLC
        /// </summary>
        /// <param name="fieldDeviceIdentifier"></param>
        /// <param name="fieldPointIdentifier"></param>
        /// <param name="rampDataCount"></param>
        /// <param name="editedTimeToMaintain"></param>
        /// <param name="editedToBeMaintainedSetPoint"></param>
        void SendChangedRampItemToPLC(string fieldDeviceIdentifier,
                                      string fieldPointIdentifier,
                                      string rampDataCount,
                                      string editedTimeToMaintain,
                                      string editedToBeMaintainedSetPoint);
    }
}
