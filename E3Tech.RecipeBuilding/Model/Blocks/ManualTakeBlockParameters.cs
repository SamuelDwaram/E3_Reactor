using System;

namespace E3Tech.RecipeBuilding.Model.Blocks
{
    public class ManualTakeBlockParameters : ICloneable
    {
        public string Substance1 { get; set; }

        public string Substance1Amout { get; set; }

        public string Substance2 { get; set; }

        public string Substance2Amout { get; set; }

        public object Clone()
        {
            return new ManualTakeBlockParameters()
            {
                Substance1 = this.Substance1?.Clone().ToString(),
                Substance1Amout = this.Substance1Amout?.Clone().ToString(),
                Substance2 = this.Substance2?.Clone().ToString(),
                Substance2Amout = this.Substance2Amout?.Clone().ToString(),
            };
        }
    }
}
