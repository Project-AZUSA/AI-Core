using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzusaTMS
{
    static class Parser
    {
        static public Concept Parse(Concept[] from, bool DontInquire=false)
        {
            int minlen;
            List<Concept[]> candidates =new List<Concept[]>();

            //Apply rules to combine concepts
            List<Concept[]> combined = Assembler.Combine(from);

            //Get only the smallest sets
            minlen=combined[0].Length;

            foreach (Concept[] cs in combined)
            {
                if (cs.Length == minlen)
                {
                    candidates.Add(cs);
                }
                else
                {
                    break;
                }
            }

            //If concepts are fully combined, disambiguate and return
            if (minlen == 1)
            {
                List<Concept> results = new List<Concept>();
                foreach (Concept[] cs in candidates)
                {
                    results.Add(cs[0]);
                }
                return Disambiguate(results);
            }
            //If not fully combined, investigate further to try to get a good parse
            else if(!DontInquire)
            {
                Concept tmp;
                List<Concept> parsed = new List<Concept>();
                foreach (Concept[] cs in candidates)
                {
                    tmp = Inquiry.Inquire(cs);

                    if (tmp!=null)
                    {
                        parsed.Add(tmp);
                    }
                }


                if (parsed.Count > 0)
                {
                    return Disambiguate(parsed);
                }                                
            }

            //If all things failed, combine everything in a randomly picked concept list into a UNKNOWN
            string name = "[";
            string content = "";
            foreach (Concept c in candidates[0])
            {
                name += c._name + ",";
                content += c._content;
            }
            name = name.TrimEnd(',');
            name = name + "]";

            return new Concept(name, "UNKNOWN", content);
             
        }

        static public Concept Disambiguate(List<Concept> candidates)
        {
            if (candidates.Count == 1)
            {
                return candidates[0];
            }
            else
            {
                double min = Semantics.SemanticDistance(candidates[0]);
                double current;
                int index=0;
                for(int i= 1;i<candidates.Count;i++)
                {
                    current = Semantics.SemanticDistance(candidates[i]);
                    if (current < min)
                    {
                        min = current;
                        index = i;
                    }
                }
                return candidates[index];
            }
        }


    }
}
