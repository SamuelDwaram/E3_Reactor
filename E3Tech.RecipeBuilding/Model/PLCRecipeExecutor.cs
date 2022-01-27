using E3.ReactorManager.Interfaces.HardwareAbstractionLayer;
using E3Tech.RecipeBuilding.Model;
using E3Tech.RecipeBuilding.Model.Blocks;
using E3.ReactorManager.Interfaces.HardwareAbstractionLayer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using E3.ReactorManager.Recipe.PLCIntegrator.Models;
using System.Threading.Tasks;
using System.Threading;
using Timer = System.Timers.Timer;
using System.Collections.Concurrent;

namespace E3Tech.RecipeBuilding
{
    public class PLCRecipeExecutor : IRecipeExecutor
    {
        private readonly IFieldDevicesCommunicator fieldDevicesCommunicator;
        private readonly IRecipesManager recipesManager;
        private readonly Timer pollRecipeTimer = new Timer(500);
        private bool pollingInProgress;
        private readonly ConcurrentDictionary<string, int> plcVarHandles = new ConcurrentDictionary<string, int>();
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public event UpdateRecipe UpdateRecipe;
        public object monitor = new object();

        public PLCRecipeExecutor(IFieldDevicesCommunicator fieldDevicesCommunicator, IRecipesManager recipesManager)
        {
            this.fieldDevicesCommunicator = fieldDevicesCommunicator;
            this.recipesManager = recipesManager;
            Initialize();
            this.fieldDevicesCommunicator.FieldPointDataReceived += FieldDevicesCommunicator_FieldPointDataReceived;


        }
        private void FieldDevicesCommunicator_FieldPointDataReceived(object sender, FieldPointDataReceivedArgs args)
        {
            Sample();
        }
        private void Initialize()
        {
            pollRecipeTimer.Elapsed += (sender, args) => {
                if (pollingInProgress)
                {
                    // Wait till current poll cycle is completed.
                }
                else
                {
                    pollingInProgress = true;
                    Task.Factory.StartNew(PollRecipe, cancellationTokenSource.Token);
                }
            };
            pollRecipeTimer.Start();
        }

        private void Sample()
        {
            foreach(string deviceId in recipesManager.DevicesRunningRecipe)
            {
                int plcHandle = plcVarHandles.ContainsKey(deviceId) ? plcVarHandles[deviceId] : default;
                if (plcHandle == default)
                {
                    lock (monitor)
                    {
                        plcHandle = fieldDevicesCommunicator.CreateVariableHandles(deviceId, new List<string> { "RecipeTags.Recipe" }).First();
                        plcVarHandles.TryAdd(deviceId, plcHandle);
                    }
                }
                else
                {
                    Recipe recipe = fieldDevicesCommunicator.ReadAny<Recipe>(deviceId, plcHandle);
                    foreach (Block block in recipe.Blocks)
                    {
                        if (block.Name == string.Empty)
                        {
                            // This is an empty block => Recipe ended with previous block.
                            // No need to read further steps. It will be time consuming operation.
                            break;
                        }
                        IRecipeBlock recipeBlock = GetRecipeBlock(block);
                        UpdateRecipe?.BeginInvoke(deviceId, block.StepNo, new RecipeStep { BlockOne = recipeBlock }, null, null);
                    }
                }
            }
        }

        private async void PollRecipe()
        {
            foreach (string deviceId in recipesManager.DevicesRunningRecipe)
            {
                int plcHandle = plcVarHandles.ContainsKey(deviceId) ? plcVarHandles[deviceId] : default;
                if (plcHandle == default)
                {
                    lock (monitor)
                    {
                        plcHandle = fieldDevicesCommunicator.CreateVariableHandles(deviceId, new List<string> { "RecipeTags.Recipe" }).First();
                        plcVarHandles.TryAdd(deviceId, plcHandle);
                    }
                }
                else
                {
                    Recipe recipe = fieldDevicesCommunicator.ReadAny<Recipe>(deviceId, plcHandle);
                    foreach (Block block in recipe.Blocks)
                    {
                        if (block.Name == string.Empty)
                        {
                            // This is an empty block => Recipe ended with previous block.
                            // No need to read further steps. It will be time consuming operation.
                            break;
                        }
                        IRecipeBlock recipeBlock = GetRecipeBlock(block);
                        UpdateRecipe?.BeginInvoke(deviceId, block.StepNo, new RecipeStep { BlockOne = recipeBlock }, null, null);
                    }
                }
            }

            await Task.Delay(100);
            pollingInProgress = false;
        }

        private IRecipeBlock GetRecipeBlock(Block block)
        {
            switch (block.Name)
            {
                case "Start":
                    return GetStartBlock(block);
                case "Stirrer":
                    return GetStirrerBlock(block);
                case "Wait":
                    return GetWaitBlock(block);
                case "Transfer":
                    return GetTransferBlock(block);
                case "Flush":
                    return GetFlushBlock(block);
                case "Drain":
                    return GetDrainBlock(block);
                case "N2Purge":
                    return GetN2PurgeBlock(block);
                case "End":
                    return GetEndBlock(block);
                default:
                    return Activator.CreateInstance<RecipeBlock>();
            }
        }

        #region PlcObjects to RecipeBlock conversions
        private IRecipeBlock GetStartBlock(Block block)
        {
            ParameterizedRecipeBlock<StartBlockParameters> startBlock = Activator.CreateInstance<ParameterizedRecipeBlock<StartBlockParameters>>();
            startBlock.Parameters.Started = block.Properties.bBlockStarted.ToString();
            startBlock.Parameters.Ended = block.Properties.bBlockEnded.ToString();
            startBlock.Parameters.StartedTime = block.Properties.sStartedTime;
            startBlock.Parameters.EndedTime = block.Properties.sEndedTime;
            startBlock.Parameters.HeatCoolModeSelection = block.Properties.bModeSelection.ToString();
            return startBlock;
        }
        private IRecipeBlock GetWaitBlock(Block block)
        {
            ParameterizedRecipeBlock<WaitBlockParameters> waitBlock = Activator.CreateInstance<ParameterizedRecipeBlock<WaitBlockParameters>>();
            waitBlock.Parameters.Started = block.Properties.bBlockStarted.ToString();
            //waitBlock.Parameters.Started = "True";
            //waitBlock.Parameters.Ended = "True";
            waitBlock.Parameters.Ended = block.Properties.bBlockEnded.ToString();
            waitBlock.Parameters.StartedTime = block.Properties.sStartedTime;
            //waitBlock.Parameters.StartedTime = "00:05";
            waitBlock.Parameters.EndedTime = block.Properties.sEndedTime;
            waitBlock.Parameters.TimeInterval = block.Properties.nInterval.ToString();
            waitBlock.Parameters.IntervalType = block.Properties.sIntervalType;
            //waitBlock.Parameters.TimeInterval = block.Properties.nInterval.ToString();
            return waitBlock;
        }

        private IRecipeBlock GetStirrerBlock(Block block)
        {
            ParameterizedRecipeBlock<StirrerBlockParameters> stirBlock = Activator.CreateInstance<ParameterizedRecipeBlock<StirrerBlockParameters>>();
            stirBlock.Parameters.Enabled = bool.TrueString;
            stirBlock.Parameters.Started = block.Properties.bBlockStarted.ToString();
            stirBlock.Parameters.Ended = block.Properties.bBlockEnded.ToString();
            stirBlock.Parameters.StartedTime = block.Properties.sStartedTime;
            stirBlock.Parameters.EndedTime = block.Properties.sEndedTime;
            stirBlock.Parameters.SetPoint = block.Properties.lrSetPoint.ToString();
            stirBlock.Parameters.Destination = block.Properties.sDestination;
            return stirBlock;
        }

        private IRecipeBlock GetTransferBlock(Block block)
        {
            ParameterizedRecipeBlock<TransferBlockParameters> transferBlock = Activator.CreateInstance<ParameterizedRecipeBlock<TransferBlockParameters>>();
            transferBlock.Parameters.Enabled = bool.TrueString;
            transferBlock.Parameters.Started = block.Properties.bBlockStarted.ToString();
            transferBlock.Parameters.Ended = block.Properties.bBlockEnded.ToString();
            transferBlock.Parameters.StartedTime = block.Properties.sStartedTime;
            transferBlock.Parameters.EndedTime = block.Properties.sEndedTime;
            transferBlock.Parameters.Source = block.Properties.sSource;
            transferBlock.Parameters.Destination = block.Properties.sDestination;
            transferBlock.Parameters.IntervalType = block.Properties.sIntervalType;
            transferBlock.Parameters.TimeInterval = block.Properties.nInterval.ToString();
            transferBlock.Parameters.TransferMode = block.Properties.bModeSelection.ToString();
            return transferBlock;
        }

        private IRecipeBlock GetN2PurgeBlock(Block block)
        {
            ParameterizedRecipeBlock<N2PurgeBlockParameters> n2PurgeBlock = Activator.CreateInstance<ParameterizedRecipeBlock<N2PurgeBlockParameters>>();
            n2PurgeBlock.Parameters.Enabled = bool.TrueString;
            n2PurgeBlock.Parameters.Started = block.Properties.bBlockStarted.ToString();
            n2PurgeBlock.Parameters.Ended = block.Properties.bBlockEnded.ToString();
            n2PurgeBlock.Parameters.StartedTime = block.Properties.sStartedTime;
            n2PurgeBlock.Parameters.EndedTime = block.Properties.sEndedTime;
            n2PurgeBlock.Parameters.Source = block.Properties.sSource;
            n2PurgeBlock.Parameters.IntervalType = block.Properties.sIntervalType;
            n2PurgeBlock.Parameters.TimeInterval = block.Properties.nInterval.ToString();
            return n2PurgeBlock;
        }

        private IRecipeBlock GetDrainBlock(Block block)
        {
            ParameterizedRecipeBlock<DrainBlockParameters> drainBlock = Activator.CreateInstance<ParameterizedRecipeBlock<DrainBlockParameters>>();
            drainBlock.Parameters.Enabled = bool.TrueString;
            drainBlock.Parameters.Started = block.Properties.bBlockStarted.ToString();
            drainBlock.Parameters.Ended = block.Properties.bBlockEnded.ToString();
            drainBlock.Parameters.StartedTime = block.Properties.sStartedTime;
            drainBlock.Parameters.EndedTime = block.Properties.sEndedTime;
            drainBlock.Parameters.Source = block.Properties.sSource;
            drainBlock.Parameters.IntervalType = block.Properties.sIntervalType;
            drainBlock.Parameters.TimeInterval = block.Properties.nInterval.ToString();
            return drainBlock;
        }

        private IRecipeBlock GetFlushBlock(Block block)
        {
            ParameterizedRecipeBlock<FlushBlockParameters> flushBlock = Activator.CreateInstance<ParameterizedRecipeBlock<FlushBlockParameters>>();
            flushBlock.Parameters.Enabled = bool.TrueString;
            flushBlock.Parameters.Started = block.Properties.bBlockStarted.ToString();
            flushBlock.Parameters.Ended = block.Properties.bBlockEnded.ToString();
            flushBlock.Parameters.StartedTime = block.Properties.sStartedTime;
            flushBlock.Parameters.EndedTime = block.Properties.sEndedTime;
            flushBlock.Parameters.Source = block.Properties.sSource;
            flushBlock.Parameters.IntervalType = block.Properties.sIntervalType;
            flushBlock.Parameters.TimeInterval = block.Properties.nInterval.ToString();
            return flushBlock;
        }

        private IRecipeBlock GetEndBlock(Block block)
        {
            ParameterizedRecipeBlock<EndBlockParameters> endBlock = Activator.CreateInstance<ParameterizedRecipeBlock<EndBlockParameters>>();
            //endBlock.Parameters.Started = block.Properties.bBlockStarted.ToString();
            //endBlock.Parameters.Started = "True";
            endBlock.Parameters.Ended = block.Properties.bBlockEnded.ToString();
            //endBlock.Parameters.Ended = "True";
            endBlock.Parameters.StartedTime = block.Properties.sStartedTime;
            endBlock.Parameters.EndedTime = block.Properties.sEndedTime;

            return endBlock;
        }
        #endregion

        public void ClearRecipe(string deviceId) => fieldDevicesCommunicator.SendCommandToDevice(deviceId, "ClearRecipeStatus", "bool", bool.TrueString);

        public void Execute(string deviceId, IList<RecipeStep> recipeSteps)
        {
            // stop the polling timer and dispose the polling task
            pollRecipeTimer.Stop();
            pollingInProgress = false;
            cancellationTokenSource.Cancel();

            Recipe recipe = new Recipe();
            IList<Block> blocks = new List<Block>();
            foreach ((RecipeStep recipeStep, int stepIndex) in recipeSteps.Select((recipeStep, stepIndex) => (recipeStep, stepIndex)))
            {
                blocks.Add(GetBlockPlcObject(recipeStep.BlockOne, stepIndex));
            }

            for (int index = blocks.Count; index < 700; index++)
            {
                blocks.Add(new Block());
            }

            recipe.Blocks = blocks.ToArray();
            
            fieldDevicesCommunicator.WriteAny<Recipe>(deviceId, plcVarHandles[deviceId], recipe);
            
            SendRecipeTriggers(recipeSteps.Count - 1, deviceId);

            //Start the timer again after writing the recipe blocks to Plc
            pollRecipeTimer.Start();
        }

        private Block GetBlockPlcObject(IRecipeBlock blockOne, int stepIndex)
        {
            Block block = new Block
            {
                StepNo = stepIndex,
                Name = blockOne?.Name
            };
            switch (blockOne?.Name)
            {
                case "Start":
                    block.Properties.bModeSelection = Convert.ToBoolean(blockOne.GetParameterValue(nameof(StartBlockParameters.HeatCoolModeSelection)));
                    break;
                case "Wait":
                    ParameterizedRecipeBlock<WaitBlockParameters> waitBlock = blockOne as ParameterizedRecipeBlock<WaitBlockParameters>;
                    block.Properties.nInterval = Convert.ToInt32(waitBlock.Parameters.TimeInterval);
                    block.Properties.sIntervalType = waitBlock.Parameters.IntervalType;
                    break;
                case "Stirrer":
                    ParameterizedRecipeBlock<StirrerBlockParameters> stirBlock = blockOne as ParameterizedRecipeBlock<StirrerBlockParameters>;
                    block.Properties.lrSetPoint = Convert.ToDouble(stirBlock.Parameters.SetPoint);
                    block.Properties.sDestination = stirBlock.Parameters.Destination;
                    break;
                case "Transfer":
                    ParameterizedRecipeBlock<TransferBlockParameters> transferBlock = blockOne as ParameterizedRecipeBlock<TransferBlockParameters>;
                    block.Properties.sSource = transferBlock.Parameters.Source;
                    block.Properties.sDestination = transferBlock.Parameters.Destination;
                    block.Properties.nInterval = Convert.ToInt32(transferBlock.Parameters.TimeInterval);
                    block.Properties.sIntervalType = transferBlock.Parameters.IntervalType;
                    block.Properties.bModeSelection = Convert.ToBoolean(transferBlock.Parameters.TransferMode);
                    break;
                case "Flush":
                    ParameterizedRecipeBlock<FlushBlockParameters> flushBlock = blockOne as ParameterizedRecipeBlock<FlushBlockParameters>;
                    block.Properties.sSource = flushBlock.Parameters.Source;
                    block.Properties.nInterval = Convert.ToInt32(flushBlock.Parameters.TimeInterval);
                    block.Properties.sIntervalType = flushBlock.Parameters.IntervalType;
                    break;
                case "Drain":
                    ParameterizedRecipeBlock<DrainBlockParameters> drainBlock = blockOne as ParameterizedRecipeBlock<DrainBlockParameters>;
                    block.Properties.sSource = drainBlock.Parameters.Source;
                    block.Properties.nInterval = Convert.ToInt32(drainBlock.Parameters.TimeInterval);
                    block.Properties.sIntervalType = drainBlock.Parameters.IntervalType;
                    break;
                case "N2Purge":
                    ParameterizedRecipeBlock<N2PurgeBlockParameters> n2PurgeBlock = blockOne as ParameterizedRecipeBlock<N2PurgeBlockParameters>;
                    block.Properties.sSource = n2PurgeBlock.Parameters.Source;
                    block.Properties.nInterval = Convert.ToInt32(n2PurgeBlock.Parameters.TimeInterval);
                    block.Properties.sIntervalType = n2PurgeBlock.Parameters.IntervalType;
                    break;
                case "End":
                    break;
                default:
                    break;
            }
            return block;    
        }

        private void SendTransferBlock(ParameterizedRecipeBlock<TransferBlockParameters> transferBlock, int stepIndex, string deviceId)
        {
            string volume = transferBlock.Parameters.Volume;
            string sourceItemIndex = transferBlock.Parameters.Source;
            string targetItemIndex = transferBlock.Parameters.Destination;

            fieldDevicesCommunicator
                .SendCommandToDevice(deviceId,
                                          "TransferEnabled_" + stepIndex,
                                          "bool",
                                          bool.TrueString);

            fieldDevicesCommunicator
                .SendCommandToDevice(deviceId,
                                          "TransferSourceItemIndex_" + stepIndex,
                                          "int",
                                          sourceItemIndex);

            fieldDevicesCommunicator
                .SendCommandToDevice(deviceId,
                                          "TransferTargetItemIndex_" + stepIndex,
                                          "int",
                                          targetItemIndex);

            fieldDevicesCommunicator
                .SendCommandToDevice(deviceId,
                                          "TransferVolume_" + stepIndex,
                                          "int",
                                          volume != null ? (int.Parse(Math.Floor(Convert.ToDouble(volume)).ToString())).ToString() : "0");
        }

        private void SendRecipeTriggers(int numberOfRecipeSteps, string deviceId)
        {
            /*
             * Send the Recipe Status and the Number of Recipe Steps to the plc
             * after sending all the recipe blocks data to the plc
             */
            fieldDevicesCommunicator
                .SendCommandToDevice(deviceId,
                                          "NumberOfRecipeSteps",
                                          "int",
                                          numberOfRecipeSteps.ToString());
            fieldDevicesCommunicator
                .SendCommandToDevice(deviceId,
                                          "RecipeStatus",
                                          "bool",
                                          bool.TrueString);
        }

        public void UpdateBlock(int stepIndex, IRecipeBlock block, string deviceId)
        {
            
        }

        public void AbortBlockExecution(int stepIndex, IRecipeBlock block, string deviceId)
        {
            
        }

        public void EditBlockExecution(string deviceId)
        {
            fieldDevicesCommunicator.SendCommandToDevice(deviceId, "EditInProgress",
                "bool",bool.TrueString);
        }

        public void SaveUpdatedBlockExecution(string deviceId)
        {
            fieldDevicesCommunicator.SendCommandToDevice(deviceId, "EditInProgress",
                "bool", bool.FalseString);
        }

        public bool GetRecipeStatus(string deviceId)
        {
            var recipeStatus
                = fieldDevicesCommunicator.ReadFieldPointValue<bool>(deviceId, "RecipeStatus");

            if (recipeStatus)
            {
                return recipeStatus;
            }
            else
            {
                return false;
            }
        }

        public void AbortRecipeExecution(string deviceId)
        {
            fieldDevicesCommunicator
                .SendCommandToDevice(deviceId,
                                          "AbortRecipeStatus",
                                          "bool",
                                          bool.TrueString);
        }
    }
}
