using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AzusaTMS
{
   
    class Rule
    {
        public string[] _reactType;
        public string _prodType;
        public string _pattern;

        public Rule(string[] reactantType, string productType, string pattern)
        {
            _reactType = reactantType;
            _prodType = productType;
            _pattern = pattern;
        }

      

        //Apply to rule onto a pool of reactants
        public Concept GetProduct(Concept[] reactants)
        {
            string content = _pattern;
            string name = "[";

            foreach (Concept elem in reactants)
            {
                name += elem._name + ",";
                content = Utils.ReplaceOnce(content, elem._type, elem._content);
            }
            name = name.Trim(',') + "]";
            Concept item = new Concept(name, _prodType, content);
            item._fromFile = reactants[0]._fromFile;

            return item;
        }

        public Concept[] ActOn(Concept[] reactants)
        {
            var product = reactants.ToList();

            bool hit = false;
            bool match = false;
            int index = -1;
            int current_site = 0;

            for (int pos = 0; pos < reactants.Length; pos++)
            {
                if (reactants[pos]._type == _reactType[current_site])
                {
                    hit = true;
                    if (current_site == _reactType.Length - 1)
                    {
                        match = true;
                        index = pos - (_reactType.Length - 1);
                        break;
                    }
                    current_site++;
                }
                else
                {
                    hit = false;
                    current_site = 0;
                }
            }

            product.Insert(index,GetProduct(product.GetRange(index, _reactType.Length).ToArray()));
            product.RemoveRange(index + 1, _reactType.Length);

            return product.ToArray();

        }
    }

    static class Assembler
    {

        static List<Rule> Rules = new List<Rule>();

        
        static public List<Concept> Combine(Concept[] elements)
        {
            List<Concept[]> current_level = new List<Concept[]>();
            current_level.Add(elements);
            List<Concept[]> next_level = new List<Concept[]>();           
            List<Rule> rules;

            while (true)
            {
                foreach (Concept[] set in current_level)
                {
                    rules = GetApplicableRules(set);
                    if (rules.Count == 0)
                    {
                        continue;
                    }

                    foreach (Rule r in rules)
                    {
                        next_level.Add(r.ActOn(set));
                    }                    
                }

                if (next_level.Count == 0)
                {
                    break;
                }

                current_level = new List<Concept[]>(next_level);
                next_level.Clear();
            }


            current_level.Sort(delegate(Concept[] x, Concept[] y)
            {
                return Math.Sign(x.Length - y.Length);
            });

            return current_level[0].ToList();
        }

        static List<Rule> GetApplicableRules(Concept[] elements)
        {
            List<Rule> rules = new List<Rule>();
            Rule r;
            foreach (string possibility in Utils.GetRuleCombinations(elements))
            {
                r=SearchForRule(possibility);
                if ( r != null)
                {
                    rules.Add(r);
                }
            }

            return rules;
        }

        static public void LoadRules(string path)
        {
            string[] rawList = File.ReadAllLines(path);
            string[] reactants;
            string product, pattern;

            foreach (string line in rawList)
            {
                if (line.Contains(','))
                {
                    reactants = line.Split(',')[0].Split('+');
                    product = line.Split(',')[1];
                    pattern = line.Replace(line.Split(',')[0] + "," + product + ",", "");
                    Rules.Add(new Rule(reactants, product.Trim(), pattern.Trim()));
                }
            }



        }

        static public void SaveRules(string path)
        {
            List<string> lines = new List<string>();
            string line;
            foreach (Rule item in Rules)
            {
                line = "";
                foreach (string site in item._reactType)
                {
                    line += site + "+";
                }
                line = line.TrimEnd('+');
                line += "," + item._prodType + "," + item._pattern;
                lines.Add(line);
            }

            File.WriteAllLines(path, lines.ToArray(), Encoding.UTF8);

        }

        static public Rule SearchForRule(string reactants)
        {
            string type;

            foreach (Rule r in Rules)
            {
                type = "";
                foreach (string site in r._reactType)
                {
                    type += site + "+";
                }
                type = type.TrimEnd('+');

                if (type == reactants)
                {
                    return r;
                }
            }

            return null;
        }

        static public void UpdateRule(string line)
        {
            string[] reactants;            
            string product, pattern;


            if (line.Contains(','))
            {
                reactants = line.Split(',')[0].Split('+');
                product = line.Split(',')[1];
                pattern = line.Replace(line.Split(',')[0] + "," + product + ",", "");


                if (Rules.Exists(r => { return r._reactType == reactants; }))
                {
                    if (product != "")
                    {
                        Rules.Find(r => { return r._reactType == reactants; })._prodType = product;
                        Rules.Find(r => { return r._reactType == reactants; })._pattern = pattern;
                    }
                    else
                    {
                        Rules.Remove(Rules.Find(r => { return r._reactType == reactants; }));
                    }
                }
                else
                {
                    Rules.Add(new Rule(reactants, product.Trim(), pattern.Trim()));
                }
            }

            
        }
    }
}
