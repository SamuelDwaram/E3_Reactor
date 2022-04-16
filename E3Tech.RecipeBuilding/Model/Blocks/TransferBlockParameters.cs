using Prism.Mvvm;
using System;

namespace E3Tech.RecipeBuilding.Model.Blocks
{
    public class TransferBlockParameters : BaseBlockParameters, ICloneable
    {
        public string Name
        {
            get => "Transfer";
        }

        private string destination;
        public override string Destination
        {
            get => destination;
            set
            {
                destination = value;
                RaisePropertyChanged();
                if(Destination == "MVA 25L" || Destination == "RV 50L" || Destination == "RV 25L")
                {
                    TransferMode = bool.TrueString;
                    IsLevelBasedVisible = false;
                }
                else
                {
                    IsLevelBasedVisible = true;
                }
            }
        }

        private string _uiLabel;
        public string UiLabel
        {
            get => _uiLabel ?? "Transfer";
            set
            {
                _uiLabel = value;
                RaisePropertyChanged();
            }
        }

        private string _volume;
        public string Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                RaisePropertyChanged();
            }
        }

        private bool  isLevelBasedVisible = true;
        public bool IsLevelBasedVisible
        {
            get
            {
                return isLevelBasedVisible;
            }
            set
            {
                isLevelBasedVisible = value;
                RaisePropertyChanged();
            }
        }


        

        private string transferMode;
        public string TransferMode
        {
            get => transferMode ?? bool.FalseString;
            set
            {
                transferMode = value;
                OnPropertyChanged();     
            }
        }

        private string transferModeString;
        public string TransferModeString
        {
            get => this.TransferMode == bool.FalseString ? "Level" : "Time";
            set { SetProperty(ref transferModeString, value); }
        }

       
        public object Clone()
        {
            return new TransferBlockParameters()
            {
                UiLabel = this.UiLabel?.Clone().ToString(),
                Started = this.Started?.Clone().ToString(),
                StartedTime = this.StartedTime?.Clone().ToString(),
                Ended = this.Ended?.Clone().ToString(),
                EndedTime = this.EndedTime?.Clone().ToString(),
                Enabled = this.Enabled?.Clone().ToString(),
                RemainingTime = this.RemainingTime?.Clone().ToString(),

                Source = this.Source?.Clone().ToString(),
                Destination = this.Destination?.Clone().ToString(),
                Volume = this.Volume?.Clone().ToString(),
                TransferMode = this.TransferMode?.Clone().ToString(),
                TransferModeString = this.TransferModeString?.Clone().ToString(),
                TimeInterval = this.TimeInterval?.Clone().ToString(),
                IntervalType = this.IntervalType?.Clone().ToString(),
            };
        }
    }
}
