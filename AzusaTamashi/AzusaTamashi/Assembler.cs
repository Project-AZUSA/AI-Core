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

        public string Type()
        {
            return type;
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
        public Concept[] GetMatch(Concept[] elements, out int[] order, bool KeepOrder = true)
        {
            bool canBind;
            bool isAMatch;
            Concept[] result = null;
            int[] resultID = null;
            Concept[][] Combinations = null;
            int[][] IDCombinations = null;

            if (KeepOrder)
            {
                Combinations = Utils.AdjacentSets<Concept>(elements);
                IDCombinations = Utils.AdjacentSetsID<Concept>(elements);
            }
            else
            {
                Combinations = Utils.FastPowerSet<Concept>(elements);
            }

            for (int i = 0; i < Combinations.GetLength(0); i++) //for each combination
            {
                isAMatch = true;

                //try to bind every element
                ClearSites();


                if (Combinations[i].Length < _reactType.Length) { continue; }


                foreach (Concept elem in Combinations[i])
                {
                    canBind = false;
                    for (int index = 0; index < _reactType.Length; index++)
                    {
                        if (_reactType[index].Bind(elem))
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
                    result = Combinations[i];
                    if (KeepOrder) resultID = IDCombinations[i];
                }
            }
            order = resultID;
            return result;
        }

        public Concept Apply(Concept[] reactants)
        {
            string content = _pattern;
            string name = "";

            foreach (Concept elem in reactants)
            {
                name += elem._name;
                content = Utils.ReplaceOnce(content, elem._type, elem._content);
            }

            Concept item = new Concept(name, _prodType, content);
            item._fromFile = reactants[0]._fromFile;
            //ConceptBase.Update(name + "," + _prodType + "," + content);

            return item;
        }

        //React from a pool
        public List<Concept> React(Concept[] pool, bool KeepOrder = true)
        {
            List<Concept> lpool = pool.ToList();
            Concept[] match = null;
            int[] matchID = null;
            int index, counter;

            do
            {
                index = -1;
                match = GetMatch(lpool.ToArray(), out matchID, KeepOrder);
                if (match == null) break;
                if (KeepOrder)
                {
                    counter = 0;
                    foreach (int position in matchID)
                    {
                        if (index == -1) index = position;
                        lpool.RemoveAt(position-counter);
                        counter++;
                    }
                }
                else
                {
                    foreach (Concept reactant in match)
                    {
                        if (lpool.Contains(reactant))
                        {
                            if (index == -1) index = lpool.IndexOf(reactant);
                            lpool.Remove(reactant);
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                lpool.Insert(index, Apply(match));
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
                sites.Clear();
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

            Rules.Sort(delegate(Rule x, Rule y)
            {
                return Math.Sign(x._reactType.Count() - y._reactType.Count());
            });

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
            } while (!Utils.Identical(pool, old_pool));

            return pool;
        }

        static public List<Concept> FreeCombine(Concept[] elements)
        {
            List<Concept> pool = elements.ToList<Concept>();
            List<Concept> old_pool;

            do
            {
                old_pool = pool;
                foreach (Rule rule in Rules)
                {
                    pool = rule.React(pool.ToArray(), false);
                }
            } while (!Utils.Identical(pool, old_pool));

            return pool;
        }

        static public List<Concept> StrictCombine(Concept[] elements)
        {
            List<Concept> pool = elements.ToList<Concept>();
            List<Concept> old_pool;

            do
            {
                old_pool = pool;

                foreach (string rule in Utils.GetRuleSets(pool.ToArray()))
                {
                    Rule r = SearchRule(rule);
                    if (r != null)
                    {
                        pool = r.React(pool.ToArray());
                    }
                }
            } while (!Utils.Identical(pool, old_pool));

            return pool;
        }

        static public void SaveRules(string path)
        {
            Rules.Sort(delegate(Rule x, Rule y)
            {
                return Math.Sign(x._reactType.Count() - y._reactType.Count());
            });


            List<string> lines = new List<string>();
            string line;
            foreach (Rule item in Rules)
            {
                line = "";
                foreach (Site site in item._reactType)
                {
                    line += site.Type() + "+";
                }
                line = line.TrimEnd('+');
                line += "," + item._prodType + "," + item._pattern;
                lines.Add(line);
            }

            File.WriteAllLines(path, lines.ToArray(), Encoding.UTF8);

        }

        static public Rule SearchRule(string reactants)
        {
            string type;

            foreach (Rule r in Rules)
            {

                type = "";
                foreach (Site site in r._reactType)
                {
                    type += site.Type() + "+";
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

            Rules.Sort(delegate(Rule x, Rule y)
            {
                return Math.Sign(x._reactType.Count() - y._reactType.Count());
            });

        }
    }
}
