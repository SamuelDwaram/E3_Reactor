using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E3Tech.RecipeBuilding.Model.Blocks
{
    public class PhaseBlockParameters : ICloneable
    {
        public string StartCondition { get; set; }

        public string Param1 { get; set; }

        public string Param1Condition { get; set; }

        public string Param2 { get; set; }

        public string Param2Condition { get; set; }

        public string AbortCondition { get; set; }

        public object Clone()
        {
            return new PhaseBlockParameters()
            {
                 AbortCondition = this.AbortCondition?.Clone().ToString(),
                 Param1 = this.Param1?.Clone().ToString(),
                 Param1Condition = this.Param1Condition?.Clone().ToString(),
                 Param2 = this.Param2?.Clone().ToString(),
                 Param2Condition = this.Param2Condition?.Clone().ToString(),
                 StartCondition = this.StartCondition?.Clone().ToString(),
            };
        }
    }
}
