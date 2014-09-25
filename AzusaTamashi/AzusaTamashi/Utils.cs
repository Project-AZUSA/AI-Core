using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzusaTMS
{
    static class Utils
    {
        //e.g. FindDifference( "3+4","1+2+3+UNKNOWN" ) = ["4"]
        public static string FindDifference(string _pattern, string _in, out int index)
        {
            string[] spattern = _pattern.Split('+');
            string[] sin = _in.Split('+');
            List<string> diff = new List<string>();
            List<int> ind = new List<int>();
            int tmp=0;

            
            bool differencefound = false;

            for(int begin =0; begin<=sin.Length-spattern.Length;begin++){

                for (int site = 0; site < spattern.Length; site++)
                {
                    if (sin[begin + site] != spattern[site])
                    {
                        if (differencefound)
                        {
                            tmp = begin + site;
                            break;
                        }
                        differencefound = true;
                    }
                    if (site == spattern.Length-1 && differencefound)
                    {
                        index = tmp;
                        return spattern[site];                        
                    }                   
                }
            }

            index = -1;
            return null;
        }

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
                        cur[i - offset] = seq[i];
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

        public static string[] GetRuleCombinations(Concept[] pool)
        {
            string type = "";
            foreach (Concept c in pool)
            {
                type += c._type + "+";
            }
            type = type.Trim('+');

            return StrGetSubsets(type);
        }

        public static string[] StrGetSubsets(string pool)
        {
            List<string> results = new List<string>();

            string[] reactants = pool.Split('+');
            string line;

            //number of elem
            for (int n = reactants.Length; n >= 1; n--)
            {
                for (int offset = 0; offset <= reactants.Length - n; offset++)
                {
                    line = "";
                    for (int i = offset; i < offset + n; i++)
                    {
                        line += reactants[i] + "+";
                    }
                    line = line.Trim('+');
                    if (!results.Contains(line))
                    {
                        results.Add(line);
                    }
                }
            }
            return results.ToArray();
        }

        static public IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });

            return GetPermutations(list, length - 1)
                .SelectMany(t => list.Where(e => !t.Contains(e)),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }

    }
}
