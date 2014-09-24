using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzusaTMS
{
    static class Inquiry
    {
        static public void Inquire(Concept[] collection)
        {
            List<Concept> UnknownConcepts = new List<Concept>();
            foreach (Concept c in collection)
            {
                if (c._type == "UNKNOWN")
                {
                    UnknownConcepts.Add(c);
                }
            }

            if (UnknownConcepts.Count > 0)
            {
                //Clarify concepts
                Console.WriteLine("");
            }
            else
            {
                //Clarify rule
            }

        }
    }
}
