using System.Text;
using System;
using System.Collections.Generic;
using System.IO;

namespace LinkTable
{
    partial class Program
    {
        static string RSplit(string content, string remove)
        {
            int index = content.ToLower().IndexOf(remove.ToLower());
            return content.Substring(index+remove.Length).Trim();
        }

        static string LSplit(string content, string remove)
        {
            int index = content.ToLower().IndexOf(remove.ToLower());
            return content.Substring(0,index).Trim();
        }
    }
}