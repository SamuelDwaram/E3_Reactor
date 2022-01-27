using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace E3Tech.RecipeBuilding.ViewModels
{
    public class DesignExperimentViewModel : BindableBase
    {
        private readonly IRegionManager regionManager;
        public DesignExperimentViewModel(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
        }
        public ICommand Navigate => new DelegateCommand<string>(page => regionManager.RequestNavigate("SelectedViewPane", page));

    }
}
