using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;
using E3.ReactorManager.Recipe.PLCIntegrator.Models;
using E3Tech.RecipeBuilding.Model;
using E3Tech.RecipeBuilding.Model.Blocks;
using E3Tech.RecipeBuilding.ParameterProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity;

namespace E3Tech.RecipeBuilding
{
    public class PLCRecipeReloader : IRecipeReloader
    {
        IFieldDevicesCommunicator fieldDevicesCommunicator;
        IUnityContainer containerProvider;
        public IList<IRecipeBlock> AvailableBlocks { get; private set; }

        public PLCRecipeReloader(IUnityContainer containerProvider)
        {
            this.containerProvider = containerProvider;

            fieldDevicesCommunicator = containerProvider.Resolve<IFieldDevicesCommunicator>();

            LoadRegisteredBlocks();
        }

        public bool CheckIfRecipeStepContainsRecipeBlock(string deviceId, string blockName, int stepIndex)
        {
            if (blockName == "Start")
            {
                if (stepIndex == 0)
                {
                    return true;
                }

                return false;
            }

            if (blockName == "End")
            {
                return false;
            }

            if (stepIndex > 0)
            {
                return fieldDevicesCommunicator.ReadFieldPointValue<bool>(deviceId, blockName + "Enabled_" + stepIndex);
            }

            return false;
        }

        private Recipe GetRecipeSteps(string deviceId)
        {
            var plcHandle = fieldDevicesCommunicator.CreateVariableHandles(deviceId, new List<string> { "RecipeTags.Recipe" }).First();
            return fieldDevicesCommunicator.ReadAny<Recipe>(deviceId, plcHandle);

        }

        private IRecipeBlock GetRecipeBlockInstance(Block block, string deviceId )
        {
            switch (block.Name)
            {
                case "Start":
                    return GetStartBlockInstance(deviceId, block);
                case "HeatCool":
                    return GetHeatCoolBlockInstance(deviceId, block);
                case "Stirrer":
                    return GetStirrerBlockInstance(deviceId, block);
                case "Wait":
                    return GetWaitBlockInstance(deviceId, block);
                case "Transfer":
                    return GetTransferBlockInstance(deviceId, block);
                case "End":
                    return GetEndBlockInstance(deviceId, block);
                default:
                    return null;
            }
        }

        private IRecipeBlock GetEndBlockInstance(string deviceId, Block block)
        {
            IRecipeBlock EndRecipeBlock = new ParameterizedRecipeBlock<EndBlockParameters>();

            EndRecipeBlock.UpdateParameterValue("Started", block.Properties.bBlockStarted.ToString());
            EndRecipeBlock.UpdateParameterValue("StartedTime", block.Properties.sStartedTime);
            EndRecipeBlock.UpdateParameterValue("Ended", block.Properties.bBlockEnded.ToString());
            EndRecipeBlock.UpdateParameterValue("EndedTime", block.Properties.sEndedTime);
            return EndRecipeBlock;
        }

        #region Get Recipe Block Instances
        private IRecipeBlock GetTransferBlockInstance(string deviceId, Block block)
        {
            IRecipeBlock transferRecipeBlock = new ParameterizedRecipeBlock<TransferBlockParameters>();

            transferRecipeBlock.UpdateParameterValue("Started", block.Properties.bBlockStarted.ToString());
            transferRecipeBlock.UpdateParameterValue("StartedTime", block.Properties.sStartedTime);
            transferRecipeBlock.UpdateParameterValue("Ended", block.Properties.bBlockEnded.ToString());
            transferRecipeBlock.UpdateParameterValue("EndedTime", block.Properties.sEndedTime);
            transferRecipeBlock.UpdateParameterValue("TargetItemIndex", fieldDevicesCommunicator.ReadFieldPointValue<string>(deviceId, "TransferTargetItemIndex_" + block.StepNo));
            transferRecipeBlock.UpdateParameterValue("SourceItemIndex", fieldDevicesCommunicator.ReadFieldPointValue<string>(deviceId, "TransferSourceItemIndex_" + block.StepNo));
            transferRecipeBlock.UpdateParameterValue("Volume", fieldDevicesCommunicator.ReadFieldPointValue<string>(deviceId, "TransferVolume_" + block.StepNo));

            return transferRecipeBlock;
        }

        private IRecipeBlock GetStartBlockInstance(string deviceId, Block block)
        {
            IRecipeBlock startRecipeBlock = new ParameterizedRecipeBlock<StartBlockParameters>();

            startRecipeBlock.UpdateParameterValue("Started", block.Properties.bBlockStarted.ToString());
            startRecipeBlock.UpdateParameterValue("StartedTime", block.Properties.sStartedTime);
            startRecipeBlock.UpdateParameterValue("Ended", block.Properties.bBlockEnded.ToString());
            startRecipeBlock.UpdateParameterValue("EndedTime", block.Properties.sEndedTime);
            startRecipeBlock.UpdateParameterValue("HeatCoolModeSelection", fieldDevicesCommunicator.ReadFieldPointValue<string>(deviceId, "HeatCoolModeSelection_" + block.StepNo));
            startRecipeBlock.UpdateParameterValue("HeatCoolDeltaTemp", fieldDevicesCommunicator.ReadFieldPointValue<string>(deviceId, "HeatCoolDeltaTemp_" + block.StepNo));

            return startRecipeBlock;
        }

        private IRecipeBlock GetHeatCoolBlockInstance(string deviceId, Block block)
        {
            IRecipeBlock heatCoolRecipeBlock = new ParameterizedRecipeBlock<HeatCoolBlockParameters>();

            heatCoolRecipeBlock.UpdateParameterValue("Started", block.Properties.bBlockStarted.ToString());
            heatCoolRecipeBlock.UpdateParameterValue("StartedTime", block.Properties.sStartedTime);
            heatCoolRecipeBlock.UpdateParameterValue("Ended", block.Properties.bBlockEnded.ToString());
            heatCoolRecipeBlock.UpdateParameterValue("EndedTime", block.Properties.sEndedTime);
            heatCoolRecipeBlock.UpdateParameterValue("OperatingMode", fieldDevicesCommunicator.ReadFieldPointValue<string>(deviceId, "HeatCoolOperatingMode_" + block.StepNo));
            heatCoolRecipeBlock.UpdateParameterValue("SetPoint", fieldDevicesCommunicator.ReadFieldPointValue<string>(deviceId, "HeatCoolSetPoint_" + block.StepNo));
            heatCoolRecipeBlock.UpdateParameterValue("Duration", fieldDevicesCommunicator.ReadFieldPointValue<string>(deviceId, "HeatCoolDuration_" + block.StepNo));

            return heatCoolRecipeBlock;
        }

        private IRecipeBlock GetStirrerBlockInstance(string deviceId, Block block)
        {
            IRecipeBlock stirrerRecipeBlock = new ParameterizedRecipeBlock<StirrerBlockParameters>();

            stirrerRecipeBlock.UpdateParameterValue("Started", block.Properties.bBlockStarted.ToString());
            stirrerRecipeBlock.UpdateParameterValue("StartedTime", block.Properties.sStartedTime);
            stirrerRecipeBlock.UpdateParameterValue("Ended", block.Properties.bBlockEnded.ToString());
            stirrerRecipeBlock.UpdateParameterValue("EndedTime", block.Properties.sEndedTime);
            stirrerRecipeBlock.UpdateParameterValue("SetPoint", fieldDevicesCommunicator.ReadFieldPointValue<string>(deviceId, "StirrerSetPoint_" + block.StepNo));

            return stirrerRecipeBlock;
        }

        private IRecipeBlock GetWaitBlockInstance(string deviceId, Block block)
        {
            IRecipeBlock waitRecipeBlock = new ParameterizedRecipeBlock<WaitBlockParameters>();

            waitRecipeBlock.UpdateParameterValue("Started", block.Properties.bBlockStarted.ToString());
            waitRecipeBlock.UpdateParameterValue("StartedTime", block.Properties.sStartedTime);
            waitRecipeBlock.UpdateParameterValue("Ended", block.Properties.bBlockEnded.ToString());
            waitRecipeBlock.UpdateParameterValue("EndedTime", block.Properties.sEndedTime);
            waitRecipeBlock.UpdateParameterValue("Duration", fieldDevicesCommunicator.ReadFieldPointValue<string>(deviceId, "WaitDuration_" + block.StepNo));
            waitRecipeBlock.UpdateParameterValue("RemainingTime", fieldDevicesCommunicator.ReadFieldPointValue<string>(deviceId, "WaitRemainingTime_" + block.StepNo));

            return waitRecipeBlock;
        }

        #endregion

        public bool GetRecipeStatus(string deviceId)
        {
            return fieldDevicesCommunicator.ReadFieldPointValue<bool>(deviceId, "RecipeStatus");
        }

        public bool GetRecipeEndedStatus(string deviceId)
        {
            return fieldDevicesCommunicator.ReadFieldPointValue<bool>(deviceId, "RecipeEnded");
        }

        private void LoadRegisteredBlocks()
        {
            AvailableBlocks = new List<IRecipeBlock>();
            IEnumerable<IRecipeBlock> blocks = containerProvider.ResolveAll<IRecipeBlock>().Where(i => !string.IsNullOrWhiteSpace(i.Name));

            if (blocks.Count() > 0)
            {
                /*
                 * Check if the recipe blocks are loaded in the UnityContainer
                 * and Update Available Blocks with the List returned from the UnityContainer
                 */
                AvailableBlocks = new List<IRecipeBlock>(blocks);
            }
        }

        public IList<RecipeStep> ReloadRecipe(object arg)
        {
            string deviceId = (string)arg;

            var recipeSteps = new List<RecipeStep>();
            Recipe recipe = GetRecipeSteps(deviceId);
            foreach (var block in recipe.Blocks)
            {
                if (string.IsNullOrEmpty(block.Name) == false)
                {
                    var step = new RecipeStep();
                    step.BlockOne = GetRecipeBlockInstance( block, deviceId );
                    recipeSteps.Add(step);
                }
            }
            return recipeSteps;
        }
    }
}
