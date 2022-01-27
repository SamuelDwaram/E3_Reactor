using System.ComponentModel;
using Unity;

namespace E3Tech.RecipeBuilding.Model
{
    public interface IRecipeBlock : INotifyPropertyChanged
    {
        string Name { get; set; }

        string UiLabel { get; set; }

        string GuidID { get; set; }

        void Configure(IUnityContainer unityContainer);

        void UpdateParameterValue(string parameterName, string paremeterValue);

        string GetParameterValue(string parameterName);
    }

}
