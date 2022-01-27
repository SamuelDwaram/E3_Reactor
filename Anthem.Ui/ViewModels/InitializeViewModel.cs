using E3.ReactorManager.Interfaces.DataAbstractionLayer;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;
using E3Tech.RecipeBuilding.Model;
using Prism.Regions;
using System.Data;
using System.Threading.Tasks;
using Unity;
using System.Linq;

namespace Anathem.Ui.ViewModels
{
    public class InitializeViewModel
    {
        private readonly IRegionManager regionManager;
        private readonly IRecipesManager recipesManager;
        private readonly IDatabaseReader databaseReader;
        private readonly TaskScheduler taskScheduler;

        public InitializeViewModel(IUnityContainer unityContainer, IRegionManager regionManager, IRecipesManager recipesManager, IDatabaseReader databaseReader)
        {
            this.regionManager = regionManager;
            this.recipesManager = recipesManager;
            this.databaseReader = databaseReader;
            taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Task.Factory.StartNew(() => unityContainer.Resolve<IFieldDevicesCommunicator>().StartCyclicPollingOfFieldDevices(CallBack, taskScheduler));
        }

        private void CallBack(Task task)
        {
            //Add the recipes for all field devices since there is no design experiment in this project
            databaseReader.ExecuteReadCommand("select Identifier from dbo.FieldDevices", CommandType.Text).AsEnumerable().ToList()
                .ForEach(dr => recipesManager.AddRecipe(dr.Field<string>(0)));
            regionManager.RequestNavigate("SelectedViewPane", "Login");
        }
    }
}
