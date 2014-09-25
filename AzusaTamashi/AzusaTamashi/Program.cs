using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AzusaTMS
{
    class Program
    {
        static int AZUSAPid = -1;
        static void AZUSALock()
        {
            try
            {
                System.Diagnostics.Process.GetProcessById(AZUSAPid);
            }
            catch
            {
                ConceptBase.Save();
                Assembler.SaveRules("rules.txt");

                Environment.Exit(0);
            }
        }

        static void Process(string msg, bool console)
        {

            Concept product = Parser.Parse(ConceptBase.Parse(msg).ToArray());

            Console.WriteLine(product._name + ", " + product._type + ", "+ product._content);

            //if (product._type == "MUTAN")
            //{
            //    Console.WriteLine(product._content);
            //}
            //else if (product._type == "NEWCONCEPT")
            //{
            //    ConceptBase.Update(product._content);
            //}
            //else if (product._type == "NEWRULE")
            //{
            //    Assembler.UpdateRule(product._content);
            //}
            //else if (product._type != "UNKNOWN")
            //{
            //    Console.WriteLine("AI(\"" + product._name + "\")");
            //}

        }


        static void Main(string[] args)
        {
            try
            {
                ConceptBase.Load("data\\main.txt");
                Assembler.LoadRules("rules.txt");
            }
            catch
            {
                Console.WriteLine("INIT FAIL. [AzusaTMS]");
                Environment.Exit(0);
            }

#if !DEBUG

            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine("GetAzusaPid()");
                try
                {
                    AZUSAPid = Convert.ToInt32(Console.ReadLine());
                    break;
                }
                catch { }
            }

            new Thread(new ThreadStart(AZUSALock)).Start();

            Console.WriteLine("RegisterAs(AI)");
            Console.WriteLine("LinkRID(INPUT,true)");
#endif
            string inp = "";
            while (true)
            {
                inp = Console.ReadLine();
                Process(inp, true);
            }

        }
    }
}
