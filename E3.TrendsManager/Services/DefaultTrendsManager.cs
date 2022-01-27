using E3.ReactorManager.Interfaces.DataAbstractionLayer;
using E3.TrendsManager.Models;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Geared;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Unity;

namespace E3.TrendsManager.Services
{
    public class DefaultTrendsManager : ITrendsManager
    {
        private readonly IDatabaseReader databaseReader;
        private readonly TaskScheduler taskScheduler;

        public DefaultTrendsManager(IList<TrendDevice> trendDevices, IDatabaseReader databaseReader, IUnityContainer unityContainer)
        {
            TrendDevices = trendDevices;
            this.databaseReader = databaseReader;
            taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            Task.Factory.StartNew(new Func<object>(() =>
                //resolve LiveTrendsManager. so it will start its work
                unityContainer.Resolve<ILiveTrendManager>()
            ), CancellationToken.None, TaskCreationOptions.None, taskScheduler);
        }

        public SeriesCollection GetTrendsCollection(TrendDevice trendDevice, IList<string> parametersFromUI, DateTime startTime, DateTime endTime)
        {
            Dictionary<string, IList<string>> trendTitleCollection = new Dictionary<string, IList<string>>();
            SeriesCollection totalSeriesCollection = new SeriesCollection();
            IList<TrendParameter> selectedParameters = new List<TrendParameter>();
            parametersFromUI.ToList().ForEach(p => {
                trendDevice.Parameters.Where(tp => tp.Label == p).ToList().ForEach(tp => selectedParameters.Add(tp));
            });
            string query = "select ";
            selectedParameters.ToList().ForEach(tp => {
                query += $"{tp.FieldPointId}, ";

                //Add the title to the collection
                if (tp.ParameterType == TrendParameterType.Group)
                {
                    if (trendTitleCollection.ContainsKey(tp.Label))
                    {
                        trendTitleCollection[tp.Label].Add(tp.FieldPointId);
                    }
                    else
                    {
                        trendTitleCollection.Add(tp.Label, new List<string> { tp.FieldPointId });
                    }
                }
                else
                {
                    trendTitleCollection.Add(tp.Label, new List<string> { tp.FieldPointId });
                }
            });

            query += $"TimeStamp from {trendDevice.DeviceId} where TimeStamp between '{startTime:yyyy-MM-dd HH:mm:ss}' and '{endTime:yyyy-MM-dd HH:mm:ss}'";
            //fetch data from db using query
            databaseReader.ExecuteReadCommand(query, CommandType.Text).AsEnumerable().ToList()
                .ForEach(dataRecord => {
                    int seriesIndex = 0;
                    foreach (KeyValuePair<string, IList<string>> keyValuePair in trendTitleCollection)
                    {
                        TrendParameter trendParameter = selectedParameters.First(tp => tp.Label == keyValuePair.Key);
                        if (keyValuePair.Value.Count > 1)
                        {
                            //Grouped parameter type
                            double sumOfParametersInGroup = Queryable.Sum(keyValuePair.Value.ToList().Select(fieldPointId => Convert.ToInt32(dataRecord.Field<double>(fieldPointId))).AsQueryable());
                            double averageOfParametersInGroup = Math.Round(sumOfParametersInGroup / keyValuePair.Value.Count, 1);
                            AddToSeriesCollection(ref totalSeriesCollection, trendParameter.Label, trendParameter.Units, averageOfParametersInGroup, ref seriesIndex);
                        }
                        else
                        {
                            //Individual parameter type
                            AddToSeriesCollection(ref totalSeriesCollection, trendParameter.Label, trendParameter.Units, dataRecord.Field<double>(trendParameter.FieldPointId), ref seriesIndex);
                        }
                    }

                    //AddTimeStamp record
                    AddToSeriesCollection(ref totalSeriesCollection, "TimeStamp", "t/min", dataRecord.Field<DateTime>("TimeStamp"), ref seriesIndex);
                });

            return totalSeriesCollection;
        }

        private void AddToSeriesCollection<T>(ref SeriesCollection totalSeriesCollection, string paramLabel, string paramUnits, T value, ref int seriesIndex)
        {
            if (totalSeriesCollection.Any(s => s.Title.Contains(paramLabel)))
            {
                totalSeriesCollection.First(s => s.Title.Contains(paramLabel)).Values.Add(value);
            }
            else
            {
                totalSeriesCollection.Add(new GLineSeries
                {
                    Title = paramUnits.Length > 0 ? $"{paramLabel}({paramUnits})" : $"{paramLabel}",
                    Values = new GearedValues<T> { value },
                    Fill = Brushes.Transparent,
                    PointGeometrySize = 5,
                    ScalesYAt = seriesIndex
                });
                seriesIndex++;
            }
        }

        public string PrepareTrendsImageForGivenData(string deviceId, DataTable dataTable, IEnumerable<string> parameterList)
        {
            Dictionary<string, string> parametersInfo = new Dictionary<string, string>();
            parameterList.ToList().ForEach(p => {
                TrendParameter tp = TrendDevices.First(td => td.DeviceId == deviceId).Parameters.FirstOrDefault(tp => tp.Label == p);
                if (tp == null)
                {
                    // skip.
                }
                else
                {
                    parametersInfo.Add($"{p}({tp.Units})", tp.Limits);
                }
            });

            if (parametersInfo.Count > 0)
            {
                SeriesCollection totalSeriesCollection = new SeriesCollection();
                AxesCollection axisY = new AxesCollection();
                foreach (KeyValuePair<string, string> parameter in parametersInfo)
                {
                    if (dataTable.Columns.Contains(parameter.Key))
                    {
                        Axis axis = new Axis
                        {
                            Title = parameter.Key,
                            Foreground = Brushes.Black,
                            MinValue = GetMinValue(parameter.Value),
                            MaxValue = GetMaxValue(parameter.Value),
                            Separator = new LiveCharts.Wpf.Separator
                            {
                                StrokeThickness = 0
                            },
                            LabelFormatter = val => Math.Round(val, 1).ToString()
                        };
                        axisY.Add(axis);
                        int index = totalSeriesCollection.Count;
                        totalSeriesCollection.Add(new GLineSeries
                        {
                            Title = parameter.Key,
                            Values = new GearedValues<DateTimePoint>(),
                            ScalesYAt = index,
                            PointGeometrySize = 0
                        });
                        totalSeriesCollection[index].Values.AddRange(new GearedValues<DateTimePoint>(dataTable.AsEnumerable().Select(row => new DateTimePoint
                        {
                            Value = row.Field<double>(parameter.Key),
                            DateTime = row.Field<DateTime>("TimeStamp")
                        })));
                    }
                }
                string tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

                CartesianChart cartesianChart = new CartesianChart
                {
                    DisableAnimations = true,
                    LegendLocation = LegendLocation.Top,
                    Height = 480,
                    Width = 650,
                    AxisY = axisY,
                    AxisX = new AxesCollection
                    {
                        new Axis
                        {
                            Title = "t/min",
                            FontSize = 12,
                            Foreground = Brushes.Black,
                            LabelFormatter = val => new DateTime((long)val).ToString("dd/MMM HH:mm:ss"),
                            Separator = new LiveCharts.Wpf.Separator
                            {
                                StrokeThickness = 0
                            }
                        }
                    },
                    Series = totalSeriesCollection
                };

                Viewbox viewBox = new Viewbox
                {
                    Child = cartesianChart
                };
                viewBox.Measure(cartesianChart.RenderSize);
                viewBox.Arrange(new Rect(new Point(0, 0), cartesianChart.RenderSize));
                cartesianChart.Update(true, true);
                viewBox.UpdateLayout();

                SaveToPng(cartesianChart, tempFilePath);
                return tempFilePath;
            }
            else
            {
                //no trends available for given parameters
                return string.Empty;
            }
        }

        private double GetMaxValue(string value)
        {
            return Convert.ToDouble(value.Contains('|') ? value.Split('|')[1] : value);
        }

        private double GetMinValue(string value)
        {
            return Convert.ToDouble(value.Contains('|') ? value.Split('|')[0] : "0");
        }

        private void SaveToPng(FrameworkElement visual, string fileName)
        {
            EncodeVisual(visual, fileName, new PngBitmapEncoder());
        }

        private static void EncodeVisual(FrameworkElement visual, string fileName, BitmapEncoder encoder)
        {
            var bitmap = new RenderTargetBitmap((int)visual.ActualWidth, (int)visual.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            var frame = BitmapFrame.Create(bitmap);
            encoder.Frames.Add(frame);
            using var stream = File.Create(fileName);
            encoder.Save(stream);
        }

        public IList<TrendDevice> TrendDevices { get; } = new List<TrendDevice>();
    }
}
