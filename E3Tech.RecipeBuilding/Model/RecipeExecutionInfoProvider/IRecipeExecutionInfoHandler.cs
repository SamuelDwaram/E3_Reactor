using System;
using System.Collections.Generic;

namespace E3Tech.RecipeBuilding.Model.RecipeExecutionInfoProvider
{
    public interface IRecipeExecutionInfoHandler
    {
        IList<RecipeBlockExecutionInfo> GetRecipeExecutionInfo(string deviceId, DateTime startTime, DateTime endTime);

        void AddRecipeExecutionInfo(string deviceId, string startTime, string endTime, string duration, string executionMessage);
    }
}
