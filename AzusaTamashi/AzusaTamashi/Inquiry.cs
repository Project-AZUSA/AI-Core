using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzusaTMS
{
    static class Inquiry
    {

        //If first parse failed, try different ways to get a good parse
        static public Concept Inquire(Concept[] collection)
        {
            
            //first separate known and unknown concepts
            List<Concept> KnownConcepts = new List<Concept>();
            List<Concept> UnknownConcepts = new List<Concept>();
            
            foreach (Concept c in collection)
            {
                if (c._type != "UNKNOWN")
                {
                    KnownConcepts.Add(c);
                }
                else
                {
                    UnknownConcepts.Add(c);
                }
            }

            //if there is some unknown concepts then first try dropping them
            if (UnknownConcepts.Count != 0)
            {
                Concept result = Parser.Parse(KnownConcepts.ToArray());
                if (result._type != "UNKNOWN")
                {
                    return result;
                }

                //if this doesn't work, try deduce type of the UNKNOWN and add them as new concepts

                //Form a type string 
                string type = "";                
                foreach (Concept c in collection)
                {
                    type += c._type + "+";
                }
                type = type.TrimEnd('+');

                //Form power sets of the type string using a starred type string
                // find corresponding rules
                string starredtype = type.Replace("UNKNOWN", ".*");
                List<Rule> matches = new List<Rule>();
                Rule r;

                foreach (string s in Utils.StrGetSubsets(starredtype))
                {
                    r = Assembler.SearchForRule(s);
                    if (r != null)
                    {
                        matches.Add(r);
                    }
                }
                
                //Next compare the matching rules with the type string to deduce the type
                foreach (Rule rule in matches)
                {
                    int index;
                    string diff= Utils.FindDifference(rule.GetTypeStr(), type, out index);

                    //pattern matched
                    if (diff!=null)
                    {                        
                        //ConceptBase.Update( + "," + diff + "," + collection[index]._content,"data\\sandbox.txt");
                        Concept tmp = new Concept(collection[index]._name,diff,collection[index]._content);
                        tmp._fromFile="data\\sandbox.txt";

                        var ctmp = collection;
                        ctmp[index] = tmp;
                        return Parser.Parse(ctmp, true);
                    }
                }

            }
            //there is actually no unknown concepts... try re-ordering the concepts
            else {
                IEnumerable<IEnumerable<Concept>> permutations = Utils.GetPermutations<Concept>(collection, collection.Length);
                Concept result;
                List<Concept> candidates = new List<Concept>();
                foreach (IEnumerable<Concept> ic in permutations)
                {
                    result = Parser.Parse(ic.ToArray(),true);

                    if (result._type != "UNKNOWN")
                    {
                        candidates.Add(result);
                    }
                }

                if (candidates.Count > 0)
                {
                    return Parser.Disambiguate(candidates);
                }

                //if that doesn't work, some rules are missing (try to guess a rule?)

            }

            return null;
        }
    }
}
