using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AzusaTMS
{
    class Concept
    {
        public string _name;
        public string _content;
        public string _type;
        public string _fromFile;

        public Concept(string Name, string Type, string Content)
        {
            _name = Name;
            _content = Content;
            _type = Type;
        }
    }


    static class ConceptBase
    {
        static List<Concept> DB = new List<Concept>();

        static public void Load(string path)
        {
            FileManager.AddPath(path);
            string[] rawDB = File.ReadAllLines(path);
            string itmName, itmContent, itmType;

            foreach (string line in rawDB)
            {
                if (line.Contains(','))
                {
                    itmName = line.Split(',')[0];
                    itmType = line.Split(',')[1];
                    itmContent = line.Replace(itmName + "," + itmType + ",", "");

                    if (itmType.Trim().ToUpper() == "IMPORT")
                    {
                        ConceptBase.Load(itmContent.Trim());
                        FileManager.AddPath(itmContent.Trim());
                    }

                    Concept item = new Concept(itmName.Trim(), itmType.Trim(), itmContent.Trim());
                    item._fromFile = path;
                    DB.Add(item);
                }
            }

            DB.Sort(delegate(Concept x, Concept y)
            {
                return Math.Sign(y._name.Length - x._name.Length);
            });
        }

        static public void Save()
        {
            DB.Sort(delegate(Concept x, Concept y)
            {
                return Math.Sign(y._name.Length - x._name.Length);
            });

            foreach (string path in FileManager.GetPath())
            {
                List<string> lines = new List<string>();

                foreach (Concept item in DB)
                {
                    if (item._fromFile == path)
                    {
                        lines.Add(item._name + "," + item._type + "," + item._content);
                    }
                }

                File.WriteAllLines(path, lines.ToArray(), Encoding.UTF8);
            }
        }

        //Extract one concept from string
        static public Concept Extract(string from, out string Left, out string Right)
        {

            foreach (Concept item in DB)
            {
                if (from.Contains(item._name))
                {
                    Left = Utils.LSplit(from, item._name);
                    Right = Utils.RSplit(from, item._name);
                    return item;
                }
            }

            //if nothing hits
            Left = "";
            Right = "";
            return new Concept(from.Trim(), "UNKNOWN", from.Trim());
        }

        static public List<Concept> Parse(string from)
        {
            List<Concept> results = new List<Concept>();

            string Left, Right;
            Concept Current;
            List<Concept> LList = new List<Concept>(), RList = new List<Concept>();

            Current = Extract(from, out Left, out Right);
            if (Left != "")
            {
                LList = Parse(Left);
            }
            if (Right != "")
            {
                RList = Parse(Right);
            }

            foreach (Concept item in LList)
            {
                results.Add(item);
            }

            results.Add(Current);

            foreach (Concept item in RList)
            {
                results.Add(item);
            }

            return results;
        }

        static public void Update(string line)
        {
            string itmName, itmType, itmContent;
            if (line.Contains(','))
            {
                itmName = line.Split(',')[0];
                itmType = line.Split(',')[1];
                itmContent = line.Replace(itmName + "," + itmType + ",", "");

                if (DB.Exists(c => { return c._name == itmName && c._type == itmType; }))
                {
                    if (itmContent.ToUpper() != "")
                    {
                        DB.Find(c => { return c._name == itmName && c._type == itmType; })._content = itmContent;
                    }
                    else
                    {
                        DB.Remove(DB.Find(c => { return c._name == itmName && c._type == itmType; }));
                    }
                }
                else
                {
                    DB.Add(new Concept(itmName.Trim(), itmType.Trim(), itmContent.Trim()));
                }
            }

            DB.Sort(delegate(Concept x, Concept y)
            {
                return Math.Sign(y._name.Length - x._name.Length);
            });
        }
    }
}
