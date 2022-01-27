namespace E3Tech.RecipeBuilding.Model
{
    public class RecipeRefresher : IRecipeRefresher
    {
        public string Id { get; set; }

        public event RefreshBlockEventHandler RefreshBlockRecieved;

        public void RefreshBlock(string id, int stepIndex, string blockName, string parameterName, string parameterValue)
        {
            RefreshBlockRecieved?.Invoke(this, new RefreshBlockEventArgs()
            {
                Id = id,
                StepIndex = stepIndex,
                BlockName = blockName,
                ParameterName = parameterName,
                ParameterValue = parameterValue
            });
        }
    }
}
