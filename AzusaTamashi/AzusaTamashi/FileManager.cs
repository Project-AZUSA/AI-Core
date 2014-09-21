using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzusaTMS
{
    static class FileManager
    {
        static  List<string> DataFiles = new List<string>();

        static public void AddPath(string path){
            if (!DataFiles.Exists(o => o == path))
            {
                DataFiles.Add(path);
            }
        }

        static public string[] GetPath()
        {
            return DataFiles.ToArray();
        }
    }
}
