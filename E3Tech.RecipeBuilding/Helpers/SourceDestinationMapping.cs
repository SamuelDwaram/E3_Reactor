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
            { "AA1", new List<string>{ "MVA 25L", "RV 50L" } },
            { "AA2", new List<string>{ "MVA 25L", "RV 50L" } },
            { "Fill In", new List<string> { "R1", "R2", "R3", "R4","R5", "AA1", "AA2", "RV 50L", "DCM" } },
            { "Flash", new List<string>{  "R1", "R2", "R3", "R4","R5", "AA1", "AA2", "RV 50L", "DCM", "MVA 25L" } },
            { "MVA", new List<string>{ "RV 50L" } },
            { "R1", new List<string>{ "R1", "R4", "R5", "AA1", "AA2", "MVB", "MVA 25L", "RV 50L" } },
            { "R2", new List<string>{ "MVA 25L"} },
            { "R3", new List<string>{ "MVA 25L"} },
            { "R4", new List<string>{ "MVA 25L"} },
            { "R5", new List<string>{ "MVB 25L"} },
            { "MVB", new List<string>{ } },
            { "DCM",  new List<string>{ }}

        };
    }
}
