using E3Tech.RecipeBuilding.Model.Blocks;
using System;
using System.Collections.Generic;

namespace E3Tech.RecipeBuilding.Model.RecipeExecutionInfoProvider
{
    public class RecipeExecutionInfoProvider : IRecipeExecutionInfoProvider
    {
        IRecipeExecutionInfoHandler recipeExecutionInfoHandler;

        public RecipeExecutionInfoProvider(IRecipeExecutionInfoHandler recipeExecutionInfoHandler)
        {
            this.recipeExecutionInfoHandler = recipeExecutionInfoHandler;
        }

        public void SaveRecipeExecutionInfo(IList<RecipeStep> recipeSteps, string deviceId)
        {
            foreach (RecipeStep step in recipeSteps)
            {
                if (step.BlockOne != null)
                {
                    AddRecipeBlockExecutionInfo(step.BlockOne, deviceId);
                }

            }
        }

        #region Add Recipe Block Execution Info
        private void AddRecipeBlockExecutionInfo(IRecipeBlock recipeBlock, string deviceId)
        {
            switch (recipeBlock.Name)
            {
                case "Start":
                    AddStartBlockExecutionInfo(recipeBlock as ParameterizedRecipeBlock<StartBlockParameters>, deviceId);
                    break;
                //case "HeatCool":
                //    AddHeatCoolBlockExecutionInfo(recipeBlock as ParameterizedRecipeBlock<HeatCoolBlockParameters>, deviceId);
                //    break;
                case "Stirrer":
                    AddStirrerBlockExecutionInfo(recipeBlock as ParameterizedRecipeBlock<StirrerBlockParameters>, deviceId);
                    break;
                case "Wait":
                    AddWaitBlockExecutionInfo(recipeBlock as ParameterizedRecipeBlock<WaitBlockParameters>, deviceId);
                    break;
                case "Transfer":
                    AddTransferBlockExecutionInfo(recipeBlock as ParameterizedRecipeBlock<TransferBlockParameters>, deviceId);
                    break;
                case "End":
                    AddEndBlockExecutionInfo(recipeBlock as ParameterizedRecipeBlock<EndBlockParameters>, deviceId);
                    break;
                default:
                    break;
            }
        }

        private void AddEndBlockExecutionInfo(ParameterizedRecipeBlock<EndBlockParameters> endRecipeBlock, string deviceId)
        {
            string message = "Recipe Ended at " + endRecipeBlock.Parameters.EndedTime;
            recipeExecutionInfoHandler.AddRecipeExecutionInfo(deviceId, string.Empty, string.Empty, string.Empty, message);
        }

        private void AddTransferBlockExecutionInfo(ParameterizedRecipeBlock<TransferBlockParameters> transferRecipeBlock, string deviceId)
        {
            string message = transferRecipeBlock.UiLabel + " started with set point " + transferRecipeBlock.Parameters.Volume + "mL";
            string duration = DateTime.Parse(transferRecipeBlock.Parameters.EndedTime).Subtract(DateTime.Parse(transferRecipeBlock.Parameters.StartedTime)).TotalMinutes.ToString();
            recipeExecutionInfoHandler
                .AddRecipeExecutionInfo(deviceId, transferRecipeBlock.Parameters.StartedTime, transferRecipeBlock.Parameters.EndedTime, duration, message);
        }

        private void AddWaitBlockExecutionInfo(ParameterizedRecipeBlock<WaitBlockParameters> waitRecipeBlock, string deviceId)
        {
            string message = "Wait started for duration " + waitRecipeBlock.Parameters.TimeInterval + waitRecipeBlock.Parameters.IntervalType;
            string duration = DateTime.Parse(waitRecipeBlock.Parameters.EndedTime).Subtract(DateTime.Parse(waitRecipeBlock.Parameters.StartedTime)).TotalMinutes.ToString();
            recipeExecutionInfoHandler
                .AddRecipeExecutionInfo(deviceId, waitRecipeBlock.Parameters.StartedTime, waitRecipeBlock.Parameters.EndedTime, duration, message);
        }

        private void AddStirrerBlockExecutionInfo(ParameterizedRecipeBlock<StirrerBlockParameters> stirrerRecipeBlock, string deviceId)
        {
            string message = "Stirrer started with SetPoint " + stirrerRecipeBlock.Parameters.SetPoint;
            string duration = DateTime.Parse(stirrerRecipeBlock.Parameters.EndedTime).Subtract(DateTime.Parse(stirrerRecipeBlock.Parameters.StartedTime)).TotalMinutes.ToString();
            recipeExecutionInfoHandler
                .AddRecipeExecutionInfo(deviceId, stirrerRecipeBlock.Parameters.StartedTime, stirrerRecipeBlock.Parameters.EndedTime, duration, message);
        }

        private void AddHeatCoolBlockExecutionInfo(ParameterizedRecipeBlock<HeatCoolBlockParameters> heatCoolRecipeBlock, string deviceId)
        {
            string message = "HC started with SetPoint " + heatCoolRecipeBlock.Parameters.SetPoint;
            string duration = DateTime.Parse(heatCoolRecipeBlock.Parameters.EndedTime).Subtract(DateTime.Parse(heatCoolRecipeBlock.Parameters.StartedTime)).TotalMinutes.ToString();
            recipeExecutionInfoHandler
                .AddRecipeExecutionInfo(deviceId, heatCoolRecipeBlock.Parameters.StartedTime, heatCoolRecipeBlock.Parameters.EndedTime, duration, message);
        }

        private void AddStartBlockExecutionInfo(ParameterizedRecipeBlock<StartBlockParameters> startRecipeBlock, string deviceId)
        {
            string message = "Recipe Started in " + (startRecipeBlock.Parameters.HeatCoolModeSelection == bool.TrueString ? "Process Mode" : "Jacket Mode");
            recipeExecutionInfoHandler
                .AddRecipeExecutionInfo(deviceId, startRecipeBlock.Parameters.StartedTime, string.Empty, string.Empty, message);
        }
        #endregion
    }
}
