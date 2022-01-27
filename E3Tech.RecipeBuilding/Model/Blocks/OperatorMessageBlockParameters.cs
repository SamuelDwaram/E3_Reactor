using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E3Tech.RecipeBuilding.Model.Blocks
{
    public class OperatorMessageBlockParameters : ICloneable
    {
        public string Message { get; set; }

        public string PlayAudibleAlert { get; set; }

        public string SelectMp3 { get; set; }

        public string RepeatAlert { get; set; }


        public object Clone()
        {
            return new OperatorMessageBlockParameters()
            {
                Message = this.Message?.Clone().ToString(),
                PlayAudibleAlert = this.PlayAudibleAlert?.Clone().ToString(),
                SelectMp3 = this.SelectMp3?.Clone().ToString(),
                RepeatAlert = this.RepeatAlert?.Clone().ToString(),
            };
        }
    }
}
