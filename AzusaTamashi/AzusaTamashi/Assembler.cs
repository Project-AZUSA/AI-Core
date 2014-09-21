using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AzusaTMS
{
    class Site
    {
        Concept binder;
        string type;
        public bool occupied = false;

        public Site(string Type)
        {
            type = Type;
        }

        public void Clear()
        {
            binder = null;
            occupied = false;
        }

        public bool Bind(Concept source)
        {
            if (occupied)
            {
                return false;
            }

            if (source._type == type)
            {
                binder = source;
                occupied = true;
            }

            return occupied;
        }
    }

    class Rule
    {
        public Site[] _reactType;
        public string _prodType;
        public string _pattern;

        public Rule(Site[] reactantType, string productType, string pattern)
        {
            _reactType = reactantType;
            _prodType = productType;
            _pattern = pattern;
        }

        void ClearSites()
        {
            foreach (Site site in _reactType)
            {
                site.Clear();
            }
        }


        //Make combinations that matches the binding sites
        public Concept[] GetMatch(Concept[] elements)
        {
            bool canBind;
            bool isAMatch;
            List<Concept[]> results = new List<Concept[]>();

            Concept[][] Combinations = Utils.FastPowerSet<Concept>(elements);

            for (int i = 0; i < (1 << elements.Length); i++) //for each combination
            {
                isAMatch = true;

                //try to bind every element
                ClearSites();

                foreach (Concept elem in Combinations[i])
                {
                    canBind = false;
                    foreach (Site site in _reactType)
                    {
                        if (site.Bind(elem))
                        {
                            canBind = true;
                            break;
                        }
                    }
                    if (!canBind) { isAMatch = false; break; }
                }

                foreach (Site site in _reactType)
                {
                    if (!site.occupied)
                    {
                        isAMatch = false;
                        break;
                    }
                }

                if (isAMatch)
                {
                    return Combinations[i];
                }
            }

            return null;
        }

        //Make combinations that matches the binding sites
        public List<Concept[]> Matches(Concept[] elements)
        {
            Concept[][] Combinations = Utils.FastPowerSet<Concept>(elements);

            bool canBind;
            bool isAMatch;
            List<Concept[]> results = new List<Concept[]>();

            //i < 2^N
            for (int i = 0; i < (1 << elements.Length); i++) //for each combination
            {
                isAMatch = true;

                //try to bind every element
                ClearSites();

                foreach (Concept elem in Combinations[i])
                {
                    canBind = false;
                    foreach (Site site in _reactType)
                    {
                        if (site.Bind(elem))
                        {
                            canBind = true;
                            break;
                        }
                    }
                    if (!canBind) { isAMatch = false; break; }
                }

                foreach (Site site in _reactType)
                {
                    if (!site.occupied)
                    {
                        isAMatch = false;
                        break;
                    }
                }

                if (isAMatch)
                {
                    results.Add(Combinations[i]);
                }
            }

            return results;
        }

        //Apply the rule 
        public Concept Apply(Concept[] reactants)
        {
            string content = _pattern;
            string name = "";

            foreach (Concept elem in reactants)
            {
                name += elem._name;
                content = Utils.ReplaceOnce(content, elem._type, elem._content);
            }

            Concept item= new Concept(name, _prodType,content);
            item._fromFile = reactants[0]._fromFile;
            ConceptBase.Update(name + "," + _prodType + "," + content);

            return item;
        }

        //React from a pool
        public List<Concept> React(Concept[] pool)
        {
            List<Concept> lpool =pool.ToList();
            Concept[] match = null;
            int index;

            //foreach (Concept[] match in Matches(pool))
            do
            {
                index = 0;
                match = GetMatch(lpool.ToArray());
                if (match == null) break;
                foreach (Concept reactant in match)
                {
                    if (lpool.Contains(reactant))
                    {
                        index = lpool.IndexOf(reactant);
                        lpool.Remove(reactant);
                    }
                    else
                    {
                        continue;
                    }
                }
                lpool.Insert(index,Apply(match));
            } while (match != null);

            return lpool;
        }
    }

    static class Assembler
    {

        static List<Rule> Rules = new List<Rule>();

        static public void LoadRules(string path)
        {
            string[] rawList = File.ReadAllLines(path);
            string[] reactants;
            List<Site> sites = new List<Site>();
            string product, pattern;

            foreach (string line in rawList)
            {
                if (line.Contains(','))
                {
                    reactants = line.Split(',')[0].Split('+');
                    product = line.Split(',')[1];
                    pattern = line.Replace(line.Split(',')[0] + "," + product + ",", "");

                    foreach (string type in reactants)
                    {
                        sites.Add(new Site(type));
                    }

                    Rules.Add(new Rule(sites.ToArray(), product.Trim(), pattern.Trim()));
                }
            }

        }

        static public List<Concept> Combine(Concept[] elements)
        {
            List<Concept> pool = elements.ToList<Concept>();
            List<Concept> old_pool;

            do
            {
                old_pool = pool;
                foreach (Rule rule in Rules)
                {
                    pool = rule.React(pool.ToArray());
                }
            } while (!Utils.Identical(pool,old_pool));

            return pool;
        }

        static public void Update(string line)
        {
            string[] reactants;
            List<Site> sites = new List<Site>();
            string product, pattern;


            if (line.Contains(','))
            {
                reactants = line.Split(',')[0].Split('+');
                product = line.Split(',')[1];
                pattern = line.Replace(line.Split(',')[0] + "," + product + ",", "");

                foreach (string type in reactants)
                {
                    sites.Add(new Site(type));
                }

                if (Rules.Exists(r => { return r._reactType == sites.ToArray(); }))
                {
                    if (product != "")
                    {
                        Rules.Find(r => { return r._reactType == sites.ToArray(); })._prodType = product;
                        Rules.Find(r => { return r._reactType == sites.ToArray(); })._pattern = pattern;
                    }
                    else
                    {
                        Rules.Remove(Rules.Find(r => { return r._reactType == sites.ToArray(); }));
                    }
                }
                else
                {
                    Rules.Add(new Rule(sites.ToArray(), product.Trim(), pattern.Trim()));
                }
            }

        }
    }
}
