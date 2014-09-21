using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzusaTMS
{
    static class Utils
    {
        public static T[][] AdjacentSets<T>(T[] seq)
        {
            List<T[]> lresult = new List<T[]>();
            for (int n = 1; n <= seq.Length; n++)
            {                
                for (int offset = 0; offset <= seq.Length - n; offset++)
                {
                    var cur = new T[n];
                    for (int i = offset; i < offset + n; i++)
                    {
                        cur[i-offset]=seq[i];
                    }
                    lresult.Add(cur);
                }
            }
            return lresult.ToArray();
        }

        public static int[][] AdjacentSetsID<T>(T[] seq)
        {
            List<int[]> lresult = new List<int[]>();
            for (int n = 1; n <= seq.Length; n++)
            {
                for (int offset = 0; offset <= seq.Length - n; offset++)
                {
                    var cur = new int[n];
                    for (int i = offset; i < offset + n; i++)
                    {
                        cur[i - offset] = i;
                    }
                    lresult.Add(cur);
                }
            }
            return lresult.ToArray();
        }

        public static string ReplaceOnce(string source, string target, string dest)
        {
            int index = source.IndexOf(target);

            return source.Insert(index, dest).Remove(index + dest.Length, target.Length);

        }

        public static string RSplit(string content, string remove)
        {
            int index = content.ToLower().IndexOf(remove.ToLower());
            return content.Substring(index + remove.Length).Trim();
        }

        public static string LSplit(string content, string remove)
        {
            int index = content.ToLower().IndexOf(remove.ToLower());
            return content.Substring(0, index).Trim();
        }

        public static bool Identical(List<Concept> lhs, List<Concept> rhs)
        {
            if (lhs.Count == rhs.Count)
            {
                for (int i = 0; i < lhs.Count; i++)
                {
                    if (lhs[i] != rhs[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public static string[] GetRuleSets(Concept[] pool)
        {
            string type="";
            foreach (Concept c in pool)
            {
                type += c._type + "+";
            }
            type = type.Trim('+');

            return StrGetRuleSets(type);
        }

        public static string[] StrGetRuleSets(string pool)
        {
            List<string> results = new List<string>();

            string[] reactants = pool.Split('+');
            string line;

            //number of elem
            for (int n =1 ; n <= reactants.Length; n++)
            {
                for (int offset = 0; offset <= reactants.Length - n; offset++)
                {
                    line = "";
                    for (int i = offset; i < offset+n; i++)
                    {
                        line += reactants[i] + "+";
                    }
                    line=line.Trim('+');
                    results.Add(line);
                }
            }
            return results.ToArray();
        }

    }
}
