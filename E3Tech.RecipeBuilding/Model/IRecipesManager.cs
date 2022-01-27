using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E3Tech.RecipeBuilding.Model
{
    public interface IRecipesManager
    {
        void AddRecipe(string deviceId);

        void RemoveRecipe(string deviceId);

        string[] DevicesRunningRecipe { get; }
    }
}
