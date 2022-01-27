using System.ComponentModel;

namespace E3Tech.RecipeBuilding.Model
{
    public interface IParameterizedRecipeBlock<T> : IRecipeBlock, INotifyPropertyChanged
    {

        T Parameters { get; }
    }
}
