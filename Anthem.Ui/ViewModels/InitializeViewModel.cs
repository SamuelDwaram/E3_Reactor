using E3.ReactorManager.Interfaces.DataAbstractionLayer;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;
using E3Tech.RecipeBuilding.Model;
using Prism.Regions;
using System.Data;
using System.Threading.Tasks;
using Unity;
using System.Linq;
using System.Collections.Generic;
using E3.TrendsManager.Models;
using E3.TrendsManager.Services;
using Unity.Resolution;

namespace Anathem.Ui.ViewModels
{
    public class InitializeViewModel
    {
        private readonly IRegionManager regionManager;
        private readonly IRecipesManager recipesManager;
        private readonly IDatabaseReader databaseReader;
        private readonly TaskScheduler taskScheduler;
        private readonly IUnityContainer unityContainer;

        public InitializeViewModel(IUnityContainer unityContainer, IRegionManager regionManager, IRecipesManager recipesManager, IDatabaseReader databaseReader)
        {
            this.regionManager = regionManager;
            this.recipesManager = recipesManager;
            this.databaseReader = databaseReader;
            this.unityContainer = unityContainer;
            taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Task.Factory.StartNew(() => unityContainer.Resolve<IFieldDevicesCommunicator>().StartCyclicPollingOfFieldDevices(CallBack, taskScheduler));
        }

        private void CallBack(Task task)
        {
            //Add the recipes for all field devices since there is no design experiment in this project
            databaseReader.ExecuteReadCommand("select Identifier from dbo.FieldDevices", CommandType.Text).AsEnumerable().ToList()
                .ForEach(dr => recipesManager.AddRecipe(dr.Field<string>(0)));
            regionManager.RequestNavigate("SelectedViewPane", "Login");
            InitializeTrendsModule();
        }

        private void InitializeTrendsModule()
        {
            unityContainer.Resolve<ITrendsManager>(new ParameterOverride[] {
                new ParameterOverride("trendDevices", new List<TrendDevice>(GetTrendDevices()))
            });
        }
        private IEnumerable<TrendDevice> GetTrendDevices()
        {
            return (from DataRow row in databaseReader.ExecuteReadCommand("select Identifier, Label from dbo.FieldDevices", CommandType.Text).AsEnumerable()
                    select new TrendDevice
                    {
                        DeviceId = row.Field<string>("Identifier"),
                        DeviceLabel = row.Field<string>("Label"),
                        Parameters = GetTrendParameters(row.Field<string>("Identifier").Split('_')[1])
                    }).ToList();
        }

        private IList<TrendParameter> GetTrendParameters(string deviceCount)
        {
            return new List<TrendParameter>
            {

                new TrendParameter
                {
                    Label = "Reac Temp",
                    Limits = "-90|200",
                    FieldPointId = "ReactorMassTemperature",
                    SensorDataSetId = "sensorDataSet_" + deviceCount,
                    Units = "°C",
                    IsLiveTrendParameter = true,
                },
                new TrendParameter
                {
                    Label = "Vapour Temp",
                    Limits = "0|200",
                    FieldPointId = "VapourTemperature",
                    SensorDataSetId = "sensorDataSet_" + deviceCount,
                    Units = "°C",
                    IsLiveTrendParameter = true,
                },

                 new TrendParameter
                {
                    Label = "HC Temp SetPoint",
                    Limits = "-90|200",
                    FieldPointId = "HeatCoolSetPoint",
                    SensorDataSetId = "sensorDataSet_" + deviceCount,
                    Units = "°C",
                    IsLiveTrendParameter = true,
                },
                new TrendParameter
                {
                    Label = "Stirrer RPM",
                    Limits = "0|200",
                    FieldPointId = "StirrerCurrentSpeed",
                    SensorDataSetId = "sensorDataSet_" + deviceCount,
                    Units = "RPM",
                    IsLiveTrendParameter = true,
                },
                new TrendParameter
                {
                    Label = "Vent Temp",
                    Limits = "0|200",
                    FieldPointId = "VentTemperature",
                    SensorDataSetId = "sensorDataSet_" + deviceCount,
                    Units = "°C",

                },
                new TrendParameter
                {
                    Label = "Pressure",
                    Limits = "-1|3",
                    FieldPointId = "ReactorPressure",
                    SensorDataSetId = "sensorDataSet_" + deviceCount,
                    Units = "Bar",
                },
                 new TrendParameter
                {
                    Label = "Jacket Temp",
                    Limits = "-90|200",
                    FieldPointId = "JacketTemperature",
                    SensorDataSetId = "sensorDataSet_" + deviceCount,
                    Units = "°C",
                },

                new TrendParameter
                {
                    Label = "Scrubber RPM",
                    Limits = "0|200",
                    FieldPointId = "ScrubberCurrentSpeed",
                    SensorDataSetId = "sensorDataSet_" + deviceCount,
                    Units = "RPM",
                },
                //new TrendParameter
                //{
                //    Label = "ReactorpH",
                //    Limits = "0|14",
                //    FieldPointId = "ReactorpH",
                //    SensorDataSetId = "sensorDataSet_" + deviceCount,
                //    Units = "",
                //},
                //new TrendParameter
                //{
                //    Label = "Stirrer SetPoint",
                //    Limits = "0|200",
                //    FieldPointId = "StirrerSpeedSetPoint",
                //    SensorDataSetId = "sensorDataSet_" + deviceCount,
                //    Units = "RPM",
                //},
            };
        }

    }
}
