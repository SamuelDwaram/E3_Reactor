using E3Tech.RecipeBuilding.Model;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace E3Tech.RecipeBuilding.ViewModels
{
    public class RecipesViewModel : BindableBase
    {
        private readonly IRecipesManager recipesManager;
        private readonly IRegionManager regionManager;
        private readonly TaskScheduler taskScheduler;

        public RecipesViewModel(IRecipesManager recipesManager, IRegionManager regionManager)
        {
            taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            this.recipesManager = recipesManager;
            this.regionManager = regionManager;
        }

        public ICommand Loaded => new DelegateCommand(() => {
            RecipesCount = recipesManager.DevicesRunningRecipe.Count();
            Task.Factory.StartNew(() => RaisePropertyChanged(nameof(RecipesCount)), CancellationToken.None, TaskCreationOptions.None, taskScheduler);
        });

        public ICommand Navigate => new DelegateCommand<string>(page => regionManager.RequestNavigate("SelectedViewPane", page));

        public int RecipesCount { get; private set; }
    }
}
