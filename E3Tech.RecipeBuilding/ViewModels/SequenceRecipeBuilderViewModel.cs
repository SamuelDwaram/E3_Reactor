using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;
using E3Tech.RecipeBuilding.Model;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Unity;

namespace E3Tech.RecipeBuilding.ViewModels
{
    public class SequenceRecipeBuilderViewModel : BindableBase
    {
        private readonly IRecipeBuilder recipeBuilder;
        private readonly IRecipeReloader recipeReloader;
        private readonly IRecipeExecutor recipeExecutor;
        private readonly IUnityContainer containerProvider;
        private RecipeBuilderViewModel SelectedRecipeBuilder;
        private readonly IFieldDevicesCommunicator fieldDevicesCommunicator;
        private readonly IRegionManager regionManager;

        

        public SequenceRecipeBuilderViewModel(IUnityContainer containerProvider,IRegionManager regionManager, IRecipeExecutor recipeExecutor, IFieldDevicesCommunicator fieldDevicesCommunicator, IRecipeBuilder recipeBuilder, IRecipeReloader recipeReloader)
        {
            this.regionManager = regionManager;
            this.recipeBuilder = recipeBuilder;
            this.recipeReloader = recipeReloader;
            this.containerProvider = containerProvider;
            this.fieldDevicesCommunicator = fieldDevicesCommunicator;
            this.recipeExecutor = recipeExecutor;
            SeqRecipeModels = new ObservableCollection<SeqRecipeModel>();
        }

        private ObservableCollection<SeqRecipeModel> seqRecipeModels;

        public ObservableCollection<SeqRecipeModel> SeqRecipeModels
        {
            get { return seqRecipeModels; }
            set { seqRecipeModels = value; }
        }

        private ICommand mouseDoubleClickCommand;
        public ICommand MouseDoubleClickCommand
        {
            get => mouseDoubleClickCommand ?? (mouseDoubleClickCommand = new DelegateCommand(new Action(MouseDoubleClickExecute)));
            set => SetProperty(ref mouseDoubleClickCommand, value);
        }

        private void MouseDoubleClickExecute()
        {
           SeqRecipeModel seqRecipeModel = recipeBuilder.ImportWithFile();
            SeqRecipeModels.Add(seqRecipeModel);
        }

        public ICommand Navigate => new DelegateCommand<string>(page => regionManager.RequestNavigate("SelectedViewPane", page));

    }
}
