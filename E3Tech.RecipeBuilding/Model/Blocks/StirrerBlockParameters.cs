using Prism.Mvvm;
using System;

namespace E3Tech.RecipeBuilding.Model.Blocks
{
    public class StirrerBlockParameters : BaseBlockParameters, ICloneable
    {
        public string Name
        {
            get => "Stirrer";
        }

        private string _uiLabel;
        public string UiLabel
        {
            get => _uiLabel ?? "Stir";
            set
            {
                _uiLabel = value;
                RaisePropertyChanged();
            }
        }

        

        private string _setPoint;
        public string SetPoint
        {
            get { return _setPoint; }
            set
            {
                _setPoint = value;
                RaisePropertyChanged();
            }
        }

        public object Clone()
        {
            return new StirrerBlockParameters()
            {
                UiLabel = this.UiLabel?.Clone().ToString(),
                Started = this.Started?.Clone().ToString(),
                StartedTime = this.StartedTime?.Clone().ToString(),
                Ended = this.Ended?.Clone().ToString(),
                EndedTime = this.EndedTime?.Clone().ToString(),
                Enabled = this.Enabled?.Clone().ToString(),
                RemainingTime = this.RemainingTime?.Clone().ToString(),

                SetPoint = this.SetPoint?.Clone().ToString(),
                Destination = this.Destination?.Clone().ToString()
            };
        }
    }
}
