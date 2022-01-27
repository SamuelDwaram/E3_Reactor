using Microsoft.Win32;
using System;
using System.IO;

namespace E3Tech.IO.FileAccess
{
    public class DefaultFileBrowser : IFileBrowser
    {
        public string OpenFile(string allowedExtentions)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false
            };
            dialog.Filter = allowedExtentions.Remove(0,1) + " files (*" + allowedExtentions + ")|*" + allowedExtentions;
            if (dialog.ShowDialog() != true)
                return null;
            return dialog.FileName;
        }

        public string SaveFile(string fileName, string extention)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                FileName = fileName,
                DefaultExt = extention,
                AddExtension = true,
            };
            bool? dialogresult = dialog.ShowDialog();
            if (dialogresult != true)
            {
                return null;
            }
            return dialog.FileName;
        }

        public string GetDefaultReceipeFileDirectiory()
        {
           string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if(Directory.Exists(path))
            {
                path = path + @"/E3Tech/Recipes/";
                if(Directory.Exists(path) == false)
                {
                    Directory.CreateDirectory(path);
                }
            }
            return path ;
        }
    }
}
