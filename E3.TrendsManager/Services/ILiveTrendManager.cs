using E3.TrendsManager.Models;
using LiveCharts.Defaults;
using System;
using System.Collections.Generic;

namespace E3.TrendsManager.Services
{
    public interface ILiveTrendManager
    {
        event UpdateLiveTrendsDataRecord UpdateLiveTrendsInUI;
        IList<TrendDevice> MonitoredTrendDevices { get; }

        Dictionary<TrendDevice, IList<LiveTrendParametersDataRecord>> TrendDevicesData { get; set; }
    }

    public delegate void UpdateLiveTrendsDataRecord(TrendDevice trendDevice, IList<DateTimePoint> parameterLiveValues, DateTime timeStamp);
}
