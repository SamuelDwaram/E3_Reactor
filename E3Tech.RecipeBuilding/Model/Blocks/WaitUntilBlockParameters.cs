using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E3Tech.RecipeBuilding.Model.Blocks
{
    class WaitUntilBlockParameters : ICloneable
    {
        public string Param1 { get; set; }

        public string Param2 { get; set; }

        public string Condition { get; set; }        

        public object Clone()
        {
            return new WaitUntilBlockParameters()
            {
                Param1 = this.Param1?.Clone().ToString(),
                Param2 = this.Param2?.Clone().ToString(),
                Condition = this.Condition?.Clone().ToString(),
            };
        }
    }
}
