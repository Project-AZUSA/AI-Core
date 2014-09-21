using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azusa;

namespace AzusaTMS
{
    class Program
    {


        static ZMQSub.DProcess Process = (List<string> msg, bool console) =>
        {
            List<Concept> products;
            foreach (string message in msg)
            {
                foreach (Concept node in ConceptBase.Parse(message))
                {
                    Console.WriteLine("IN: "+node._name + "," + node._type + "," + node._content);
                }

                products = Assembler.Combine(ConceptBase.Parse(message).ToArray());

                foreach (Concept node in products)
                {

                    Console.WriteLine(node._name + "," + node._type + "," + node._content);


                    if (node._type == "MUTAN")
                    {
                        Console.WriteLine(node._content);
                    }
                    else if (node._type == "UPDATE")
                    {
                        ConceptBase.Update(node._content);
                    }
                    else if (node._type == "RULE")
                    {
                        Assembler.Update(node._content);
                    }
                }
            }

        };


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


            string inp = "";
            while (inp != "EXIT")
            {
                inp = Console.ReadLine();
                Process((new string[] { inp }).ToList(), true);
            }

            ConceptBase.Save();
            Assembler.SaveRules("rules.txt");

            ////TODO FIX SAVING
            //ZMQSub Azusa = new ZMQSub(Process, true);
            //Azusa.Start();



        }
    }
}
