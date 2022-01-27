using System;

namespace E3Tech.RecipeBuilding.Model
{
    public interface IRecipeRefresher
    {
        
        event RefreshBlockEventHandler RefreshBlockRecieved;

        void RefreshBlock(string id, int stepIndex, string blockName, string parameterName, string parameterValue);
    }

    public delegate void RefreshBlockEventHandler(object sender, RefreshBlockEventArgs e);

    public class RefreshBlockEventArgs : EventArgs
    {
        public string Id { get; internal set; }
        public string ParameterValue { get; internal set; }
        public string ParameterName { get; internal set; }
        public string BlockName { get; internal set; }
        public int StepIndex { get; internal set; }        
    }
}
