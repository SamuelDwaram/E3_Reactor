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
                    Label = "MVA Temperature",
                    Limits = "-90|200",
                    FieldPointId = "MVA_Temperature",
                    SensorDataSetId = "sensorDataSet_" + deviceCount,
                    Units = "°C",
                },
                new TrendParameter
                {
                    Label = "RV-50L Temperature",
                    Limits = "-90|200",
                    FieldPointId = "RV50L_Temperature",
                    SensorDataSetId = "sensorDataSet_" + deviceCount,
                    Units = "°C",
                },
                new TrendParameter
                {
                    Label = "RV-25L Temperature",
                    Limits = "-90|200",
                    FieldPointId = "RV25L_Temperature",
                    SensorDataSetId = "sensorDataSet_" + deviceCount,
                    Units = "°C",
                },
                new TrendParameter
                {
                    Label = "MVA Pressure",
                    Limits = "0|9",
                    FieldPointId = "MVA_Pressure",
                    SensorDataSetId = "sensorDataSet_" + deviceCount,
                    Units = "Bar",
                },
                new TrendParameter
                {
                    Label = "RV-50L Pressure",
                    Limits = "0|9",
                    FieldPointId = "RV50L_Pressure",
                    SensorDataSetId = "sensorDataSet_" + deviceCount,
                    Units = "Bar",
                },
                new TrendParameter
                {
                    Label = "RV-25L Pressure",
                    Limits = "0|9",
                    FieldPointId = "RV25L_Pressure",
                    SensorDataSetId = "sensorDataSet_" + deviceCount,
                    Units = "Bar",
                },
                new TrendParameter
                {
                    Label = "MVA Level",
                    Limits = "0|100",
                    FieldPointId = "MVA_Level",
                    SensorDataSetId = "sensorDataSet_" + deviceCount,
                    Units = "%",
                },
                new TrendParameter
                {
                    Label = "RV-50L Level",
                    Limits = "0|100",
                    FieldPointId = "RV50L_Level",
                    SensorDataSetId = "sensorDataSet_" + deviceCount,
                    Units = "%",
                },
                new TrendParameter
                {
                    Label = "RV-25L Level",
                    Limits = "0|100",
                    FieldPointId = "RV25L_Level",
                    SensorDataSetId = "sensorDataSet_" + deviceCount,
                    Units = "%",
                },
                new TrendParameter
                {
                    Label = "Chiller Temperature",
                    Limits = "0|50",
                    FieldPointId = "ChillerTemperature",
                    SensorDataSetId = "sensorDataSet_" + deviceCount,
                    Units = "°C",
                },
                new TrendParameter
                {
                    Label = "Chiller Temperature Setpoint",
                    Limits = "0|100",
                    FieldPointId = "ChillerSetpoint",
                    SensorDataSetId = "sensorDataSet_" + deviceCount,
                    Units = "°C",
                },
                

            };
        }

    }
}
