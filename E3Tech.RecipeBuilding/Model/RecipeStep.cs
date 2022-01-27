using System.ComponentModel;

namespace E3Tech.RecipeBuilding.Model
{
    public class RecipeStep : INotifyPropertyChanged
    {
        

        public override string ToString()
        {
            return this.GetType().Name;
        }

        private IRecipeBlock blockOne;
        public IRecipeBlock BlockOne
        {
            get => blockOne;
            set
            {
                blockOne = value;
                RaisePropertyChanged(nameof(BlockOne));
            }
        }

        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                RaisePropertyChanged("Name");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
