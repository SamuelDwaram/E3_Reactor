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

        private SlaveRecipe GetRecipeSteps(string deviceId)
        {
            var plcHandle = fieldDevicesCommunicator.CreateVariableHandles(deviceId, new List<string> { "RecipeTags.SlaveRecipe" }).First();
            return fieldDevicesCommunicator.ReadAny<SlaveRecipe>(deviceId, plcHandle);

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
                //case "N2Purge":
                //    return GetN2PurgeBlockInstance(deviceId, block);
                case "Drain":
                    return GetDrainBlockInstance(deviceId, block);
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
            transferRecipeBlock.UpdateParameterValue("Source", block.Properties.sSource);
            transferRecipeBlock.UpdateParameterValue("Destination", block.Properties.sDestination);
            transferRecipeBlock.UpdateParameterValue("TimeInterval", Convert.ToString(block.Properties.nInterval));
            transferRecipeBlock.UpdateParameterValue("TransferMode", Convert.ToString(block.Properties.bModeSelection));
            transferRecipeBlock.UpdateParameterValue("IntervalType", Convert.ToString(block.Properties.sIntervalType));


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
            stirrerRecipeBlock.UpdateParameterValue("Source", block.Properties.sSource);
            stirrerRecipeBlock.UpdateParameterValue("SetPoint", Convert.ToString(block.Properties.lrSetPoint));
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
            waitRecipeBlock.UpdateParameterValue("RemainingTime", block.Properties.sRemainingTime);
            waitRecipeBlock.UpdateParameterValue("TimeInterval", block.Properties.nInterval.ToString());

            //waitRecipeBlock.UpdateParameterValue("Duration", fieldDevicesCommunicator.ReadFieldPointValue<string>(deviceId, "WaitDuration_" + block.StepNo));
            //waitRecipeBlock.UpdateParameterValue("RemainingTime", fieldDevicesCommunicator.ReadFieldPointValue<string>(deviceId, "WaitRemainingTime_" + block.StepNo));

            return waitRecipeBlock;
        }

        //private IRecipeBlock GetN2PurgeBlockInstance(string deviceId, Block block)
        //{
        //    IRecipeBlock n2PurgeRecipeBlock = new ParameterizedRecipeBlock<N2PurgeBlockParameters>();

        //    n2PurgeRecipeBlock.UpdateParameterValue("Started", block.Properties.bBlockStarted.ToString());
        //    n2PurgeRecipeBlock.UpdateParameterValue("StartedTime", block.Properties.sStartedTime);
        //    n2PurgeRecipeBlock.UpdateParameterValue("Ended", block.Properties.bBlockEnded.ToString());
        //    n2PurgeRecipeBlock.UpdateParameterValue("EndedTime", block.Properties.sEndedTime);
        //    n2PurgeRecipeBlock.UpdateParameterValue("Source", block.Properties.sSource);
        //    n2PurgeRecipeBlock.UpdateParameterValue("TimeInterval", Convert.ToString(block.Properties.nInterval));
        //    n2PurgeRecipeBlock.UpdateParameterValue("IntervalType", Convert.ToString(block.Properties.sIntervalType));
        //    n2PurgeRecipeBlock.UpdateParameterValue("Duration", fieldDevicesCommunicator.ReadFieldPointValue<string>(deviceId, "WaitDuration_" + block.StepNo));
        //    n2PurgeRecipeBlock.UpdateParameterValue("RemainingTime", fieldDevicesCommunicator.ReadFieldPointValue<string>(deviceId, "WaitRemainingTime_" + block.StepNo));

        //    return n2PurgeRecipeBlock;
        //}

        private IRecipeBlock GetDrainBlockInstance(string deviceId, Block block)
        {
            IRecipeBlock drainRecipeBlock = new ParameterizedRecipeBlock<DrainBlockParameters>();

            drainRecipeBlock.UpdateParameterValue("Started", block.Properties.bBlockStarted.ToString());
            drainRecipeBlock.UpdateParameterValue("StartedTime", block.Properties.sStartedTime);
            drainRecipeBlock.UpdateParameterValue("Ended", block.Properties.bBlockEnded.ToString());
            drainRecipeBlock.UpdateParameterValue("EndedTime", block.Properties.sEndedTime);
            //drainRecipeBlock.UpdateParameterValue("TimeInterval", Convert.ToString(block.Properties.nInterval));
            //drainRecipeBlock.UpdateParameterValue("IntervalType", Convert.ToString(block.Properties.sIntervalType));
            drainRecipeBlock.UpdateParameterValue("Source", block.Properties.sSource);
            drainRecipeBlock.UpdateParameterValue("Duration", fieldDevicesCommunicator.ReadFieldPointValue<string>(deviceId, "WaitDuration_" + block.StepNo));
            drainRecipeBlock.UpdateParameterValue("RemainingTime", fieldDevicesCommunicator.ReadFieldPointValue<string>(deviceId, "WaitRemainingTime_" + block.StepNo));

            return drainRecipeBlock;
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
            SlaveRecipe recipe = GetRecipeSteps(deviceId);
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
