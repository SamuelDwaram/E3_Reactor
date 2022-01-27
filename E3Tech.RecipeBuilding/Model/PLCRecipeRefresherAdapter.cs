using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer.Data;
using System;
using System.Collections.Generic;

namespace E3Tech.RecipeBuilding.Model
{
    public class PLCRecipeRefresherAdapter
    {
        private readonly IRecipeExecutor recipeExecutor;
        private readonly IRecipeRefresher recipeRefresher;
        private readonly IFieldDevicesCommunicator fieldDevicesCommunicator;

        public PLCRecipeRefresherAdapter(IFieldDevicesCommunicator fieldDevicesCommunicator, IRecipeExecutor recipeExecutor, IRecipeRefresher recipeRefresher)
        {
            this.recipeExecutor = recipeExecutor;
            this.recipeExecutor.UpdateRecipe += RecipeExecutor_UpdateRecipe;
            this.recipeRefresher = recipeRefresher;
            this.fieldDevicesCommunicator = fieldDevicesCommunicator;
            this.fieldDevicesCommunicator.FieldPointDataReceived += CommunicatorInstance_FieldPointDataReceived;
        }

        private void RecipeExecutor_UpdateRecipe(string deviceId, int stepIndex, RecipeStep recipeStep)
        {
            UpdateRecipeBlock(deviceId, stepIndex, recipeStep.BlockOne);
        }

        private void UpdateRecipeBlock(string deviceId, int stepIndex, IRecipeBlock recipeBlock)
        {
            Dictionary<string, string> toBeUpdatedParameters = new Dictionary<string, string>();
            toBeUpdatedParameters.Add("Started", recipeBlock.GetParameterValue("Started"));
            toBeUpdatedParameters.Add("Ended", recipeBlock.GetParameterValue("Ended"));
            toBeUpdatedParameters.Add("StartedTime", recipeBlock.GetParameterValue("StartedTime"));
            toBeUpdatedParameters.Add("EndedTime", recipeBlock.GetParameterValue("EndedTime"));
            toBeUpdatedParameters.Add("Enabled", bool.TrueString);

            foreach (KeyValuePair<string, string> kv in toBeUpdatedParameters)
            {
                recipeRefresher.RefreshBlock(deviceId, stepIndex, recipeBlock.Name, kv.Key, kv.Value);
            }
        }

        private void CommunicatorInstance_FieldPointDataReceived(object sender, FieldPointDataReceivedArgs args)
        {
            if (recipeRefresher != null)
            {
                try
                {
                    string blocParameterName = args.FieldPointDescription;
                    if (blocParameterName.Contains("Recipe"))
                    {
                        blocParameterName = blocParameterName.Substring(blocParameterName.IndexOf("_") + 1);
                        if (blocParameterName.IndexOf("_") > 0)
                        {
                            string blockName = blocParameterName.Substring(0, blocParameterName.IndexOf("_"));
                            string parameterNameWithStepNumber = blocParameterName.Substring(blocParameterName.IndexOf("_") + 1);
                            if (parameterNameWithStepNumber.IndexOf("_") > 0)
                            {
                                string parameterName = parameterNameWithStepNumber.Substring(0, parameterNameWithStepNumber.IndexOf("_"));
                                int stepIndex = Convert.ToInt32(parameterNameWithStepNumber.Substring(parameterNameWithStepNumber.IndexOf("_") + 1));
                                if (!string.IsNullOrEmpty(blockName))
                                {
                                    recipeRefresher.RefreshBlock(args.FieldDeviceIdentifier, stepIndex, blockName, parameterName, args.NewFieldPointData);
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {

                }
            }
        }
    }
}
