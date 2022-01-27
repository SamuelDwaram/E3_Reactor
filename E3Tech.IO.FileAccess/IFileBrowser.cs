using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E3Tech.IO.FileAccess
{
   public interface IFileBrowser
    {
        string SaveFile(string fileName, string extention);

        string GetDefaultReceipeFileDirectiory();

        string OpenFile(string allowedExtentions);
    }
}
