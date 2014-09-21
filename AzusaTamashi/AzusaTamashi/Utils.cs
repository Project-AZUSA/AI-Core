using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzusaTMS
{
    static class Utils
    {
        public static T[][] FastPowerSet<T>(T[] seq)
        {
            var powerSet = new T[1 << seq.Length][];
            powerSet[0] = new T[0]; // starting only with empty set
            for (int i = 0; i < seq.Length; i++)
            {
                var cur = seq[i];
                int count = 1 << i; // doubling list each time
                for (int j = 0; j < count; j++)
                {
                    var source = powerSet[j];
                    var destination = powerSet[count + j] = new T[source.Length + 1];
                    for (int q = 0; q < source.Length; q++)
                        destination[q] = source[q];
                    destination[source.Length] = cur;
                }
            }
            return powerSet;
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

        

    }
}
