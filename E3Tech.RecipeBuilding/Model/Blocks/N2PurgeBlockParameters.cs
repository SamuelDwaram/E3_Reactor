using System;

namespace E3Tech.RecipeBuilding.Model.Blocks
{
    public class N2PurgeBlockParameters : BaseDrainBlockParameters, ICloneable
    {
        public string Name
        {
            get => "N2Purge";
        }

        private string _uiLabel;
        public string UiLabel
        {
            get => _uiLabel ?? "N2Purge";
            set
            {
                _uiLabel = value;
                RaisePropertyChanged();
            }
        }
        public object Clone()
        {
            return new N2PurgeBlockParameters()
            {
                UiLabel = this.UiLabel?.Clone().ToString(),
                Started = this.Started?.Clone().ToString(),
                StartedTime = this.StartedTime?.Clone().ToString(),
                Ended = this.Ended?.Clone().ToString(),
                EndedTime = this.EndedTime?.Clone().ToString(),
                Enabled = this.Enabled?.Clone().ToString(),

                Source = this.Source?.Clone().ToString(),
                TimeInterval = this.TimeInterval?.Clone().ToString(),
                IntervalType = this.IntervalType?.Clone().ToString(),
            };
        }
    }
}
