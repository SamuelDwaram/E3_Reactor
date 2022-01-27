using E3.TrendsManager.Models;
using E3.TrendsManager.Services;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Geared;
using LiveCharts.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Unity;

namespace E3.TrendsManager.ViewModels
{
    public class LiveTrendsViewModel : BindableBase
    {
        private readonly IUnityContainer unityContainer;
        private readonly TaskScheduler taskScheduler;

        public LiveTrendsViewModel(IUnityContainer unityContainer)
        {
            taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            this.unityContainer = unityContainer;
        }

        private void LiveTrendsViewModel_UpdateLiveTrendsInUI(TrendDevice trendDevice, IList<DateTimePoint> parameterLiveValues, DateTime timeStamp)
        {
            if (TrendDevice.DeviceId == trendDevice.DeviceId)
            {
                for (int index = 0; index < TrendsCollection.Count; index++)
                {
                    //Retain data for only 6 hours
                    if (TrendsCollection[index].Values.Count > 21600)
                    {
                        TrendsCollection[index].Values.RemoveAt(0);
                    }
                    TrendsCollection[index].Values.Add(parameterLiveValues[index]);
                }
                Task.Factory.StartNew(new Action(() => RaisePropertyChanged(nameof(TrendsCollection))), CancellationToken.None, TaskCreationOptions.None, taskScheduler);
            }
        }

        public void SetParameters(TrendDevice trendDevice, AxesCollection axisY, SeriesCollection seriesCollection)
        {
            //Subscribe to the UpdateLiveTrendsInUI event in ILiveTrendsManager
            unityContainer.Resolve<ILiveTrendManager>().UpdateLiveTrendsInUI -= LiveTrendsViewModel_UpdateLiveTrendsInUI;
            unityContainer.Resolve<ILiveTrendManager>().UpdateLiveTrendsInUI += LiveTrendsViewModel_UpdateLiveTrendsInUI;

            //Update trend device
            TrendDevice = trendDevice;
            AxisY = axisY;
            RaisePropertyChanged(nameof(AxisY));

            if (TrendsCollection.Count > 0 || TrendsCollection.Chart != null)
            {
                TrendsCollection.Clear();
            }
            int seriesIndex = 0;
            foreach (GLineSeries series in seriesCollection)
            {
                TrendsCollection.Add(new GLineSeries {
                    PointGeometrySize = 0,
                    Title = series.Title,
                    Values = new GearedValues<DateTimePoint>(),
                    Fill = Brushes.Transparent,
                    ScalesYAt = series.ScalesYAt
                });

                //Add all the DateTimePoint's separately to the newly added Series since direct assignment is not working
                foreach (DateTimePoint dateTimePoint in series.Values)
                {
                    TrendsCollection[seriesIndex].Values.Add(dateTimePoint);
                }
                ++seriesIndex;
            }
            RaisePropertyChanged(nameof(TrendsCollection));
        }

        #region Commands
        public ICommand ResetZoomCommand
        {
            get => new DelegateCommand<Axis[]>(axes => {
                foreach (var axis in axes)
                {
                    axis.MinValue = double.NaN;
                    axis.MaxValue = double.NaN;
                }
            });
        }
        #endregion

        #region Properties
        public Func<double, string> YFormatter { get; } = val => Math.Round(val, 2).ToString();
        public Func<double, string> XFormatter { get; } = val => new DateTime((long)val).ToString("dd-MM-yy HH:mm:ss");
        private static readonly AxesCollection defaultAxisCollection = new AxesCollection()
        {
            new Axis {
                Title = "",
                MinValue = 0,
                MaxValue = 100,
                LabelFormatter = val => Math.Round(val, 1).ToString()
            }
        };
        public AxesCollection AxisY { get; set; } = defaultAxisCollection;
        public SeriesCollection TrendsCollection { get; set; } = new SeriesCollection();
        public TrendDevice TrendDevice { get; set; } = new TrendDevice();
        #endregion
    }
}
