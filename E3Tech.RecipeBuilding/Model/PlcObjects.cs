using System.Runtime.InteropServices;

namespace E3.ReactorManager.Recipe.PLCIntegrator.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Recipe
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
        public Block[] Blocks;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Block
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
        public string Recipe;
        public int StepNo;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
        public string Name;
        public Properties Properties;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Properties
    {
        [MarshalAs(UnmanagedType.I1)]
        public bool bStartCommand;
        [MarshalAs(UnmanagedType.I1)]
        public bool bStopCommand;
        [MarshalAs(UnmanagedType.I1)]
        public bool bBlockStarted;
        [MarshalAs(UnmanagedType.I1)]
        public bool bBlockEnded;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string sStartedTime;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string sEndedTime;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
        public string sSource;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
        public string sDestination;
        public double lrSetPoint;
        [MarshalAs(UnmanagedType.I1)]
        public bool bModeSelection;
        public int nInterval;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string sIntervalType;
    }
}
