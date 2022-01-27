using E3Tech.RecipeBuilding.Model;
using System.Collections.Generic;

namespace E3Tech.RecipeBuilding
{
    public class RecipesManager : IRecipesManager
    {
        private static RecipesManager instance;

        private readonly List<string> devicesRunningRecipe;

        public RecipesManager()
        {
            devicesRunningRecipe = new List<string>();
        }

        public void AddRecipe(string deviceId)
        {
            if (devicesRunningRecipe.Contains(deviceId))
            {
                /* DeviceId is already added to the Recipes */
            }
            else
            {
                devicesRunningRecipe.Add(deviceId);
            }
        }

        public void RemoveRecipe(string deviceId)
        {
            devicesRunningRecipe.RemoveAt(devicesRunningRecipe.IndexOf(deviceId));
        }

        public static RecipesManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new RecipesManager();
                return instance;
            }
        }

        public string[] DevicesRunningRecipe => devicesRunningRecipe.ToArray();
    }
}
