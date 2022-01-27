using Prism.Mvvm;

namespace E3Tech.RecipeBuilding.ViewModels
{
    public class BlockParameterProviderViewModel : BindableBase
    {
        public BlockParameterProviderViewModel()
        {

        }

        private object _parameters;
        public object Parameters 
        {
            get => _parameters;
            set => SetProperty(ref _parameters, value);
        }
    }
}
