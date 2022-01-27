using E3.TrendsManager.Models;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.Data;

namespace E3.TrendsManager.Services
{
    public interface ITrendsManager
    {
        IList<TrendDevice> TrendDevices { get; }

        SeriesCollection GetTrendsCollection(TrendDevice trendDevice, IList<string> selectedParameters, DateTime startTime, DateTime endTime);

        string PrepareTrendsImageForGivenData(string deviceId, DataTable dataTable, IEnumerable<string> parameterList);
    }
}
