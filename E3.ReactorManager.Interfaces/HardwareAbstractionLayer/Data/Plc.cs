using System;
using System.Threading.Tasks;
using TwinCAT.Ads;

namespace E3.ReactorManager.Interfaces.HardwareAbstractionLayer.Data
{
    public class Plc
    {
        /// <summary>
        /// Specify the TwinCAT client object
        ///     used to connect to PLC
        /// </summary>
        public TcAdsClient TwinCATClient { set; get; }

        /// <summary>
        /// Plc Uniqe Identifier
        /// </summary>
        public string Identifier { set; get; }

        /// <summary>
        /// Label of PLC (Beckhoff)
        /// </summary>
        public string Label
        {
            get;set;
        }

        /// <summary>
        /// AMS Net id of PLC
        /// </summary>
        public string Address
        {
            get;set;
        }

        /// <summary>
        /// Port Number 
        /// </summary>
        public int PortNumber
        {
            get; set;
        }

        /// <summary>
        /// Connect to PLC
        /// </summary>
        public void Connect()
        {
            //Check if the plc is already connected
            if (TwinCATClient.IsConnected)
            {
                return;
            }
            TwinCATClient.Connect(Address, PortNumber);
        }

        /// <summary>
        /// Create Variable Handle using FieldPoint's MemoryAddress
        /// </summary>
        /// <param name="MemoryAddress"></param>
        public int CreateVariableHandle(string MemoryAddress) => TwinCATClient.CreateVariableHandle(MemoryAddress);

        public void DeleteVariableHandle(int variableHandle) => TwinCATClient.DeleteVariableHandle(variableHandle);

        /// <summary>
        /// Read From the Plc
        /// </summary>
        public string Read(int PlcHandle, Type DataType)
        {
            string toBeReturnedValue = string.Empty;

            //Check if TwinCATClient is Connected and Not Disposed
            if (!TwinCATClient.Disposed && TwinCATClient.IsConnected)
            {
                try
                {
                    toBeReturnedValue = TwinCATClient.ReadAny(PlcHandle, DataType).ToString();
                }
                catch (AdsErrorException adsErrorException)
                {
                    Console.WriteLine(adsErrorException.Message + " " + DateTime.Now.ToString());
                    return string.Empty;
                }
            }

            return toBeReturnedValue;
        }

        /// <summary>
        /// Read from string variables of plc
        /// </summary>
        /// <param name="PlcHandle"></param>
        public string ReadString(int PlcHandle)
        {
            AdsStream adsStream = new AdsStream(30);
            AdsBinaryReader reader = new AdsBinaryReader(adsStream);

            string toBeReturnedValue = string.Empty;

            //Check if TwinCatClient is Connected and Not Disposed
            if (!TwinCATClient.Disposed && TwinCATClient.IsConnected)
            {
                try
                {
                    toBeReturnedValue = reader.ReadPlcAnsiString(TwinCATClient.Read(PlcHandle, adsStream));
                }
                catch (AdsErrorException adsErrorException)
                {
                    Console.WriteLine(adsErrorException.Message + " " + DateTime.Now.ToString());
                    return string.Empty;
                }   
            }

            return toBeReturnedValue;
        }

        /// <summary>
        /// Write From the Plc
        /// </summary>
        public bool Write(int PLcHandle, object Data)
        {
            try
            {
                if (TwinCATClient.IsConnected)
                {
                    TwinCATClient.WriteAny(PLcHandle, Data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Writing values to PLC");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                return false;
            }
            return true;
        }

        /// <summary>
        /// Write to string variable of PLC
        /// </summary>
        /// <param name="PlcHandle"></param>
        /// <param name="writeStringValue"></param>
        public bool WriteString(int PlcHandle, string writeStringValue)
        {
            try
            {
                TwinCATClient.WriteAny(PlcHandle, writeStringValue, new int[] { writeStringValue.Length });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Writing string to PLC");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                return false;
            }
            return true;
        }

        /// <summary>
        /// Dispose all variable handles and
        /// Disconnect from plc
        /// </summary>
        public async void Disconnect()
        {
            try
            {
                /*
                 * Check if the connection exists and then disconnect
                 */
                if (TwinCATClient.IsConnected)
                {
                    TwinCATClient.Disconnect();
                    TwinCATClient.Dispose();

                    Console.WriteLine("Closed connections successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in disposing plc TwinCAT client object");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            await Task.Yield();
        }
    }
}
