using E3.Mediator.Models;
using E3.Mediator.Services;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;
using E3.TrendsManager.Models;
using E3.TrendsManager.ViewModels;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Geared;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace E3.TrendsManager.Services
{
    public class LiveTrendManager : ILiveTrendManager
    {
        private readonly ITrendsManager trendsManager;
        private readonly IFieldDevicesCommunicator fieldDevicesCommunicator;
        private readonly LiveTrendsViewModel liveTrendsViewModel;

        public LiveTrendManager(ITrendsManager trendsManager, MediatorService mediatorService, LiveTrendsViewModel liveTrendsViewModel, IFieldDevicesCommunicator fieldDevicesCommunicator)
        {
            this.trendsManager = trendsManager;
            this.fieldDevicesCommunicator = fieldDevicesCommunicator;
            this.liveTrendsViewModel = liveTrendsViewModel;
            SubscribeToMediatorServiceEvents(mediatorService);
            Task.Run(ReadLiveTrendParametersData);
        }

        private void ReadLiveTrendParametersData()
        {
            foreach (TrendDevice trendDevice in MonitoredTrendDevices.Where(td => td.Parameters.Any(tp => tp.IsLiveTrendParameter)))
            {
                IList<DateTimePoint> liveTrendValues = new List<DateTimePoint>();
                LiveTrendParametersDataRecord liveDataRecord = new LiveTrendParametersDataRecord();
                foreach (TrendParameter trendParameter in trendDevice.Parameters)
                {
                    double liveData = fieldDevicesCommunicator.ReadFieldPointValue<double>(trendDevice.DeviceId, trendParameter.FieldPointId);
                    liveDataRecord.Data.Add(trendParameter, liveData);
                    liveTrendValues.Add(new DateTimePoint { 
                        Value = liveData,
                        DateTime = DateTime.Now
                    });
                }
                //update TimeStamp of LiveDataRecord
                liveDataRecord.TimeStamp = DateTime.Now;
                //add the live data to the container
                if (TrendDevicesData.ContainsKey(trendDevice))
                {
                    //Retain data for only 6 hours
                    if (TrendDevicesData[trendDevice].Count > 21600)
                    {
                        TrendDevicesData[trendDevice].RemoveAt(0);
                    }
                    TrendDevicesData[trendDevice].Add(liveDataRecord);
                }
                else
                {
                    TrendDevicesData.Add(trendDevice, new List<LiveTrendParametersDataRecord> { liveDataRecord });
                }

                UpdateLiveTrendsInUI?.BeginInvoke(trendDevice, liveTrendValues, liveDataRecord.TimeStamp, null, null);
            }

            //sleep for 1 second and read the parameters data again
            Task.Factory.StartNew(new Action(() => Thread.Sleep(1000)))
                .ContinueWith(t => ReadLiveTrendParametersData());
        }

        private void SubscribeToMediatorServiceEvents(MediatorService mediatorService)
        {
            mediatorService.Register(InMemoryMediatorMessageContainer.UpdateSelectedDeviceId, obj => {
                Device device = obj as Device;
                TrendDevice trendDevice = MonitoredTrendDevices.First(td => td.DeviceId == device.Id);
                SeriesCollection seriesCollection = GetSeriesCollection(trendDevice, TrendDevicesData.First(keyValuePair => keyValuePair.Key == trendDevice).Value);
                liveTrendsViewModel.SetParameters(trendDevice, GetAxesCollection(seriesCollection, trendDevice), seriesCollection);
            });
        }

        public AxesCollection GetAxesCollection(SeriesCollection seriesCollection, TrendDevice trendDevice)
        {
            AxesCollection axesCollection = new AxesCollection();
            seriesCollection.ToList().ForEach(series =>
            {
                if (series.Title.Contains("TimeStamp"))
                {
                    // Skip.
                }
                else
                {
                    TrendParameter trendParameter = trendDevice.Parameters.First(p => series.Title.Contains(p.Label));
                    axesCollection.Add(new Axis
                    {
                        Title = series.Title,
                        Foreground = Brushes.White,
                        MinValue = Convert.ToDouble(trendParameter.Limits.Contains('|') ? trendParameter.Limits.Split('|')[0] : "0"),
                        MaxValue = Convert.ToDouble(trendParameter.Limits.Contains('|') ? trendParameter.Limits.Split('|')[1] : trendParameter.Limits),
                        Separator = new Separator
                        {
                            StrokeThickness = 0
                        },
                        LabelFormatter = val => Math.Round(val, 1).ToString()
                    });
                }
            });

            return axesCollection.Count > 0 ? axesCollection : new AxesCollection()
            {
                new Axis {
                    Title = "",
                    MinValue = 0,
                    MaxValue = 100,
                    LabelFormatter = val => Math.Round(val, 1).ToString()
                }
            };
        }

        private SeriesCollection GetSeriesCollection(TrendDevice trendDevice, IList<LiveTrendParametersDataRecord> liveTrendParametersDataRecords)
        {
            SeriesCollection totalSeriesCollection = new SeriesCollection();
            int seriesIndex = 0;
            trendDevice.Parameters.ToList().ForEach(tp => {
                if (tp.IsLiveTrendParameter)
                {
                    IEnumerable<DateTimePoint> valuesList = liveTrendParametersDataRecords.Select(dataRecord => new DateTimePoint { 
                        Value = dataRecord.Data.First(keyValuePair => keyValuePair.Key == tp).Value,
                        DateTime = dataRecord.TimeStamp
                    });
                    totalSeriesCollection.Add(new GLineSeries
                    {
                        Title = tp.Units.Length > 0 ? $"{tp.Label}({tp.Units})" : $"{tp.Label}",
                        Values = new GearedValues<DateTimePoint>(valuesList),
                    });
                    seriesIndex++;
                }
            });
            
            return totalSeriesCollection;
        }

        public event UpdateLiveTrendsDataRecord UpdateLiveTrendsInUI;
        public IList<TrendDevice> MonitoredTrendDevices => trendsManager.TrendDevices;
        public Dictionary<TrendDevice, IList<LiveTrendParametersDataRecord>> TrendDevicesData { get; set; } = new Dictionary<TrendDevice, IList<LiveTrendParametersDataRecord>>();
    }
}
