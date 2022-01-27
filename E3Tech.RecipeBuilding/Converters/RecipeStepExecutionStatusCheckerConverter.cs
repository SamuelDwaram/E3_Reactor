using E3Tech.RecipeBuilding.Model;
using E3Tech.RecipeBuilding.Model.Blocks;
using E3Tech.RecipeBuilding.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;

namespace E3Tech.RecipeBuilding.Converters
{
    public class RecipeStepExecutionStatusCheckerConverter : IValueConverter
    {
        /* Checks whether the Recipe step has completed the Execution of all the blocks in it */
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var recipeStep = (value as RecipeStepViewModel).RecipeStep;

                if (recipeStep.BlockOne != null)
                {
                    if (!GetBlockEndedStatus(recipeStep.BlockOne))
                    {
                        return false;
                    }
                }

            //    if (recipeStep.BlockTwo != null)
            //    {
            //        if (!GetBlockEndedStatus(recipeStep.BlockTwo))
            //        {
            //            return false;
            //        }
            //    }

            //    if (recipeStep.BlockThree != null)
            //    {
            //        if (!GetBlockEndedStatus(recipeStep.BlockThree))
            //        {
            //            return false;
            //        }
            //    }

            //    if (recipeStep.BlockFour != null)
            //    {
            //        if (!GetBlockEndedStatus(recipeStep.BlockFour))
            //        {
            //            return false;
            //        }
            //    }
            }

            return true;
        }

        public bool GetBlockEndedStatus(IRecipeBlock block)
        {
            string blockEndedStatus;

            switch (block.Name)
            {
                case "Start":
                    blockEndedStatus = (block as ParameterizedRecipeBlock<StartBlockParameters>).Parameters.Ended;
                    break;
                case "HeatCool":
                    blockEndedStatus = (block as ParameterizedRecipeBlock<HeatCoolBlockParameters>).Parameters.Ended;
                    break;
                case "Stirrer":
                    blockEndedStatus = (block as ParameterizedRecipeBlock<StirrerBlockParameters>).Parameters.Ended;
                    break;
                case "Wait":
                    blockEndedStatus = (block as ParameterizedRecipeBlock<WaitBlockParameters>).Parameters.Ended;
                    break;
                default:
                    blockEndedStatus = string.Empty;
                    break;
            }

            if (string.IsNullOrWhiteSpace(blockEndedStatus))
            {
                return false;
            }
            else
            {
                return bool.Parse(blockEndedStatus);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }
    }
}
