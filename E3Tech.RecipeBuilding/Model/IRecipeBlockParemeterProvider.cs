namespace E3Tech.RecipeBuilding.Model
{
    public interface IRecipeBlockParemeterProvider<T>
    {
        bool PopulateParameters();

        T Parameters { get; }
    }
}
