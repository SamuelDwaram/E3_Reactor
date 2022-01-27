using E3Tech.RecipeBuilding.Model;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows.Input;
using Unity;

namespace E3Tech.RecipeBuilding.ViewModels
{
    public class RecipeStepViewModel : BindableBase
    {
        private readonly IUnityContainer containerProvider;

        public RecipeStepViewModel(IUnityContainer containerProvider)
        {
            this.containerProvider = containerProvider;            
        }

        //private void ConfigureBlock(IRecipeBlock recipeBlock)
        //{
        //    if (recipeBlock == null)
        //    {
        //        return;
        //    }

        //    recipeBlock.Configure(containerProvider);
        //}

        //private ICommand configureBlockCommand;
        //public ICommand ConfigureBlockCommand
        //{
        //    get => configureBlockCommand ?? (configureBlockCommand = new DelegateCommand<IRecipeBlock>(new Action<IRecipeBlock>(ConfigureBlock)));
        //    private set => SetProperty(ref configureBlockCommand, value);
        //}

        public RecipeStep RecipeStep { get; set; }
    }
}
