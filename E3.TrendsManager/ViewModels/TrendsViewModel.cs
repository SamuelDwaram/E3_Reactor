using E3.TrendsManager.Helpers;
using E3.TrendsManager.Models;
using E3.TrendsManager.Services;
using LiveCharts;
using LiveCharts.Geared;
using LiveCharts.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace E3.TrendsManager.ViewModels
{
    public class TrendsViewModel : BindableBase
    {
        private readonly ITrendsManager trendsManager;
        private readonly TaskScheduler taskScheduler;

        public TrendsViewModel(ITrendsManager trendsManager)
        {
            this.trendsManager = trendsManager;
            taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
        }

        #region Touch Device Identifiers
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);

        public static bool IsTouchEnabled()
        {
            int MAXTOUCHES_INDEX = 0x95;
            return GetSystemMetrics(MAXTOUCHES_INDEX) > 0;
        }
        #endregion

        #region Tooltip Handlers
        private TooltipContent GetTooltipContent(Point point)
        {
            int closestPointIndex = point.X > 0 ? Convert.ToInt32(Math.Truncate(point.X)) : 0;
            TooltipContent newTooltipContent = new TooltipContent();
            SeriesCollection trendsCollection = (SeriesCollection)GetType().GetProperty("TrendsCollection").GetValue(this, null);
            if (trendsCollection.Count > 0)
            {
                foreach (string selectedParameter in SelectedTrendParameters)
                {
                    AddToToolTipContent<double>(ref newTooltipContent, selectedParameter, trendsCollection, closestPointIndex);
                }

                //Add TimeStamp
                AddToToolTipContent<DateTime>(ref newTooltipContent, "TimeStamp", new SeriesCollection { TimeStampSeries }, closestPointIndex);
            }

            return newTooltipContent;
        }

        private void AddToToolTipContent<T>(ref TooltipContent tooltipContent, string paramName, SeriesCollection seriesCollection, int closestPointIndex)
        {
            GLineSeries parameterValuesCollection = (GLineSeries)seriesCollection.FirstOrDefault(series => series.Title.Contains(paramName));
            if (parameterValuesCollection == null)
            {
                // Skip. No trends data
            }
            else
            {
                TooltipParameterAndValue tooltipParameterAndValue = ReturnTooltipParameterAndValueObject(paramName, (GearedValues<T>)parameterValuesCollection.Values, closestPointIndex);
                if (tooltipParameterAndValue == null)
                {
                    // Skip. No trends data
                }
                else
                {
                    tooltipContent.Content.Add(tooltipParameterAndValue);
                }
            }
        }

        TooltipParameterAndValue ReturnTooltipParameterAndValueObject<T>(string parameterName, GearedValues<T> valuesCollection, int closestValueIndex)
        {
            if (valuesCollection != null && valuesCollection.Count > 0 && closestValueIndex < valuesCollection.Count)
            {
                return new TooltipParameterAndValue { ParameterName = parameterName, ParameterValue = valuesCollection[closestValueIndex].ToString() };
            }

            return null;
        }

        private void UpdateUI(TooltipContent result)
        {
            TooltipContent = result;
        }

        public void UpdateMouseMoveTooltipContent(MouseMoveCommandParameters commandParameters)
        {
            var chart = commandParameters.Sender;
            try
            {
                //lets get where the mouse is at our chart
                var mouseCoordinate = commandParameters.args.GetPosition(chart);
                var point = chart.ConvertToChartValues(mouseCoordinate);

                XPointer = point.X;

                UpdateUI(GetTooltipContent(point));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " At UpdateMouseMoveTooltipContent");
            }
        }

        public void UpdateTouchMoveToolTipContent(TouchMoveCommandParameters commandParameters)
        {
            var chart = commandParameters.Sender;
            try
            {
                //lets get where the mouse is at our chart
                var mouseCoordinate = commandParameters.args.GetTouchPoint(chart);
                var point = chart.ConvertToChartValues(mouseCoordinate.Position);

                XPointer = point.X;

                UpdateUI(GetTooltipContent(point));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " At UpdateTouchMoveToolTipContent");
            }
        }
        #endregion

        public AxesCollection GetAxesCollection(SeriesCollection seriesCollection)
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
                    TrendParameter trendParameter = AvailableDevices.First(d => d.DeviceId == SelectedDevice).Parameters.First(p => series.Title.Contains(p.Label));
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

            return axesCollection.Count > 0 ? axesCollection : new AxesCollection { new Axis {
                Title = "",
                MinValue = 0,
                MaxValue = 100,
                LabelFormatter = val => Math.Round(val, 1).ToString()
            }};
        }

        #region Commands
        public ICommand GetLastWeekTrendsCommand
        {
            get => new DelegateCommand(() => {
                DateTime start = DateTime.Now.Subtract(TimeSpan.FromDays(7));
                Start.Date = start.Date;
                Start.Hour = start.Hour;
                Start.Minute = start.Minute;

                DateTime end = DateTime.Now;
                End.Date = end.Date;
                End.Hour = end.Hour;
                End.Minute = end.Minute;
                GenerateTrendsCommand.Execute(default);
            });
        }

        public ICommand Get24HoursTrendsCommand
        {
            get => new DelegateCommand(() => {
                DateTime start = DateTime.Now.Subtract(TimeSpan.FromDays(1));
                Start.Date = start.Date;
                Start.Hour = start.Hour;
                Start.Minute = start.Minute;

                DateTime end = DateTime.Now;
                End.Date = end.Date;
                End.Hour = end.Hour;
                End.Minute = end.Minute;
                GenerateTrendsCommand.Execute(default);
            });
        }

        public ICommand GenerateTrendsCommand
        {
            get => new DelegateCommand(() =>
            {
                //Generate SelectedStartDate and SelectedEndDate from Start and End Objects
                SelectedStartDate = Start.Date.AddHours(Start.Hour).AddMinutes(Start.Minute);
                SelectedEndDate = End.Date.AddHours(End.Hour).AddMinutes(End.Minute);

                Task.Factory.StartNew(new Func<SeriesCollection>(() => trendsManager.GetTrendsCollection(AvailableDevices.First(d => d.DeviceId == SelectedDevice), SelectedTrendParameters, SelectedStartDate, SelectedEndDate)), CancellationToken.None, TaskCreationOptions.None, taskScheduler)
                    .ContinueWith(new Func<Task<SeriesCollection>, SeriesCollection>(t =>
                    {
                        MaxValueAxisX = t.Result.Count > 0 ? t.Result.Max(series => series.Values.Count) : 0;
                        if (t.Result.Count >= 0)
                        {
                            AxisY = GetAxesCollection(t.Result);
                            RaisePropertyChanged(nameof(AxisY));
                        }
                        return t.Result;
                    }), taskScheduler).ContinueWith(new Action<Task<SeriesCollection>>(t =>
                    {
                        if (t.Result.Count == 0)
                        {
                            //No Data found.
                            MessageBox.Show("No Data found in the System for the selected interval. Please select valid Start and End Times.", "No Data", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        foreach (GLineSeries series in t.Result)
                        {
                            if (series.Title.Contains("TimeStamp"))
                            {
                                TimeStampSeries = series;
                                //No need to add TimeStamp collection to TrendsCollection.
                                //It is used only for adding content to ToolTip
                                continue;
                            }
                            TrendsCollection.Add(series);
                        }
                        RaisePropertyChanged(nameof(TrendsCollection));
                    }), taskScheduler);
            });
        }

        public ICommand UpdateAvailableTrendParametersCommand
        {
            get => new DelegateCommand<string>(deviceId =>
            {
                SelectedTrendParameters.Clear();
                AvailableTrendParameters.Clear();

                AvailableDevices.First(td => td.DeviceId == deviceId).Parameters.ToList()
                .ForEach(p => {
                    if (AvailableTrendParameters.Contains(p.Label))
                    {
                        // Skip.
                    }
                    else
                    {
                        AvailableTrendParameters.Add(p.Label);
                    }
                });
                RaisePropertyChanged(nameof(AvailableTrendParameters));
            });
        }

        public ICommand AddToSelectedParametersCommand
        {
            get => new DelegateCommand<string>(tp =>
            {
                if (SelectedTrendParameters.Any(p => p == tp))
                {
                    // Skip.
                }
                else
                {
                    SelectedTrendParameters.Add(tp);
                }
            });
        }

        public ICommand RemoveFromSelectedParametersCommand
        {
            get => new DelegateCommand<string>(tp =>
            {
                SelectedTrendParameters.Remove(tp);
            });
        }

        public ICommand ClearTrendsCollectionCommand
        {
            get => new DelegateCommand(() =>
            {
                TooltipContent = new TooltipContent();
                TrendsCollection = new SeriesCollection();
                RaisePropertyChanged(nameof(TrendsCollection));
            });
        }

        public ICommand UpdateMouseMoveToolTipCommand
        {
            get => new DelegateCommand<MouseMoveCommandParameters>(new Action<MouseMoveCommandParameters>(UpdateMouseMoveTooltipContent));
        }

        public ICommand UpdateTouchMoveToolTipCommand
        {
            get => new DelegateCommand<TouchMoveCommandParameters>(new Action<TouchMoveCommandParameters>(UpdateTouchMoveToolTipContent));
        }
        #endregion

        #region Properties
        public bool IsTouchDevice
        {
            get => IsTouchEnabled();
        }

        public IList<TrendDevice> AvailableDevices
        {
            get => trendsManager.TrendDevices;
        }

        public IList<string> AvailableTrendParameters { get; set; } = new List<string>();
        public IList<string> SelectedTrendParameters { get; set; } = new List<string>();
        public string SelectedDevice { get; set; }
        public DateTime SelectedStartDate { get; set; } = DateTime.Now;
        public DateTime SelectedEndDate { get; set; } = DateTime.Now;
        public SeriesCollection TrendsCollection { get; set; } = new SeriesCollection();
        public GLineSeries TimeStampSeries { get; set; } = new GLineSeries();

        public AxesCollection AxisY { get; set; } = new AxesCollection { new Axis {
                Title = "",
                MinValue = 0,
                MaxValue = 100,
                LabelFormatter = val => Math.Round(val, 1).ToString()
            }};

        private double _maxValueAxisX = 60;
        public double MaxValueAxisX
        {
            get => _maxValueAxisX;
            set
            {
                _maxValueAxisX = value;
                RaisePropertyChanged();
            }
        }

        private double _xPointer;
        public double XPointer
        {
            get => _xPointer;
            set
            {
                _xPointer = value;
                RaisePropertyChanged();
            }
        }

        private TooltipContent _tooltipContent;
        public TooltipContent TooltipContent
        {
            get => _tooltipContent ??= new TooltipContent();
            set
            {
                _tooltipContent = value;
                RaisePropertyChanged();
            }
        }

        public Time Start { get; set; } = new Time();
        public Time End { get; set; } = new Time();
        #endregion
    }

    public class Time : BindableBase
    {
        private DateTime _date = DateTime.Now.Date;
        public DateTime Date
        {
            get { return _date; }
            set { SetProperty(ref _date, value); }
        }

        private int _hour = DateTime.Now.Hour;
        public int Hour
        {
            get => _hour;
            set
            {
                if (value > 23 || value < 0)
                {
                    // Don't allow.
                    SetProperty(ref _hour, 0);
                }
                else
                {
                    DateTime userInput = Date;
                    userInput = userInput.AddHours(value).AddMinutes(_minute);
                    if (userInput > DateTime.Now)
                    {
                        SetProperty(ref _hour, 0);
                    }
                    else
                    {
                        SetProperty(ref _hour, value);
                    }
                }
            }
        }

        private int _minute = DateTime.Now.Minute;
        public int Minute
        {
            get => _minute;
            set
            {
                if (value > 59 || value < 0)
                {
                    // Don't allow.
                    SetProperty(ref _minute, 0);
                }
                else
                {
                    DateTime userInput = Date;
                    userInput = userInput.AddHours(_hour).AddMinutes(value);
                    if (userInput > DateTime.Now)
                    {
                        SetProperty(ref _minute, 0);
                    }
                    else
                    {
                        SetProperty(ref _minute, value);
                    }
                }
            }
        }
    }
}
