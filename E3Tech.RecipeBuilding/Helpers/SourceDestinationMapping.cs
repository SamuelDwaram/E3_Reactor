using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E3Tech.RecipeBuilding.Helpers
{
    public class SourceDestinationMapping
    {
        public static Dictionary<string, List<string>> SourceDestinationPair = new Dictionary<string, List<string>>()
        {
            { "DCM", new List<string>{ "MVA", "RV 50L", "RV 25L" } },
            { "MVB", new List<string>{  "RV 50L", "RV 25L" } },
            { "AA1", new List<string>{ "MVA", "RV 50L", "RV 25L" } },
            { "AA2", new List<string>{ "MVA", "RV 50L", "RV 25L" } },
            { "Fill In", new List<string> { "R1", "R2", "R3", "R4", "R5", "AA1", "AA2", "DCM" } },
            { "Flash", new List<string>{  "MVA", "RV 50L", "RV 25L" } },
            { "MVA", new List<string>{ "RV 50L", "RV 25L" } },
            { "R1", new List<string>{  "R3", "R4", "R5", "AA1", "AA2", "MVB", "MVA", "RV 50L", "RV 25L" } },
            { "R2", new List<string>{ "MVA"} },
            { "R3", new List<string>{ "MVA"} },
            { "R4", new List<string>{ "MVA"} },
            { "R5", new List<string>{ "MVB"} }

        };
    }
}
