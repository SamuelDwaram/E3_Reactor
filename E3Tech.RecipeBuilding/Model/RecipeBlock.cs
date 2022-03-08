using System.ComponentModel;
using Unity;

namespace E3Tech.RecipeBuilding.Model
{
    public class RecipeBlock : IRecipeBlock
    {
        private string name;
        private string uiLabel;
        private string guidId;
        private int index;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return this.Name;
        }

        public void UpdateParameterValue(string parameterName, string paremeterValue)
        {

        }

        public bool Configure(IUnityContainer unityContainer)
        {
            return true;
        }

        public string GetParameterValue(string parameterName)
        {
            return string.Empty;
        }

        public string Name { get => name; set => name = value; }

        public string UiLabel { get => uiLabel; set => uiLabel = value; }

        public string GuidID { get => guidId; set => guidId = value; }

        public int Index { get => index; set => index = value; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
