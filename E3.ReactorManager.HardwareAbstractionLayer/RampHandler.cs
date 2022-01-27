using E3.ReactorManager.Framework;
using E3.ReactorManager.Interfaces.Framework.Logging;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer.Data;
using System;
using System.Threading.Tasks;

namespace E3.ReactorManager.HardwareAbstractionLayer
{
    public class RampHandler : IRampHandler
    {
        #region Fields
        ILogger logger = new Logger();
        #endregion

        #region Functions
        /// <summary>
        /// Sends the corresponding changed ramp item to PLC
        /// </summary>
        /// <param name="fieldDeviceIdentifier"></param>
        /// <param name="fieldPointIdentifier"></param>
        /// <param name="rampDataCount"></param>
        /// <param name="editedTimeToMaintain"></param>
        /// <param name="editedToBeMaintainedSetPoint"></param>
        public async void SendChangedRampItemToPLC(string fieldDeviceIdentifier,
                                             string fieldPointIdentifier,
                                             string rampDataCount,
                                             string editedTimeToMaintain,
                                             string editedToBeMaintainedSetPoint)
        {
            try
            {
                //send command to PLC
                FieldDevicesCommunicator.CommunicatorInstance
                    .SendCommandToDevice(fieldDeviceIdentifier,
                                              fieldPointIdentifier + "RampTime_" + rampDataCount,
                                              "int",
                                              int.Parse(Math.Floor(Convert.ToDouble(editedTimeToMaintain)).ToString()).ToString());
                //send command to PLC
                FieldDevicesCommunicator.CommunicatorInstance
                    .SendCommandToDevice(fieldDeviceIdentifier,
                                              fieldPointIdentifier + "RampSetPoint_" + rampDataCount,
                                              "int",
                                              int.Parse(Math.Floor(Convert.ToDouble(editedToBeMaintainedSetPoint)).ToString()).ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in sending edited ramp item to PLC");
                Console.WriteLine(ex.Message);

                logger.Log(LogType.Exception, "Error in sending edited ramp item to PLC", ex);
            }
            await Task.Yield();
        }
        /// <summary>
        /// Sends ramp to PLC
        /// </summary>
        /// <param name="ramp"></param>
        public async void SendRampToPLC(Ramp ramp)
        {
            //Send ramp To PLC
            for (int rampDataCount = 1; rampDataCount <= ramp.RampDataSet.Count; rampDataCount++)
            {
                try
                {
                    FieldDevicesCommunicator.CommunicatorInstance
                        .SendCommandToDevice(ramp.FieldDeviceIdentifier,
                                                 ramp.FieldPointIdentifier + "RampTime_" + rampDataCount,
                                                 "int",
                                                 int.Parse(Math.Floor(Convert.ToDouble(ramp.RampDataSet[rampDataCount - 1].TimeToMaintain)).ToString()).ToString());
                    FieldDevicesCommunicator.CommunicatorInstance
                        .SendCommandToDevice(ramp.FieldDeviceIdentifier,
                                                 ramp.FieldPointIdentifier + "RampSetPoint_" + rampDataCount,
                                                 "int",
                                                 int.Parse(Math.Floor(Convert.ToDouble(ramp.RampDataSet[rampDataCount - 1].ToBeMaintainedSetPoint)).ToString()).ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in sending total ramp item to PLC");
                    Console.WriteLine("Error in message : " + ex.Message);
                    Console.WriteLine("Error StackTrace : " + ex.StackTrace);

                    logger.Log(LogType.Exception, "Error in sending total ramp item to PLC", ex);
                }
            }
            //send Number of steps in Ramp to PLC
            try
            {
                FieldDevicesCommunicator.CommunicatorInstance
                    .SendCommandToDevice(ramp.FieldDeviceIdentifier,
                                              ramp.FieldPointIdentifier + "NumberOfStepsInRamp",
                                              "int",
                                              ramp.RampDataSet.Count.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Sending Number of Steps in Ramp to PLC");
                Console.WriteLine(ex.Message);

                logger.Log(LogType.Exception, "Error in Sending Number of Steps in Ramp to PLC", ex);
            }
            await Task.Yield();
        }
        #endregion
    }
}
