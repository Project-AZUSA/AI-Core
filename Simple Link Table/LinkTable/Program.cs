using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace LinkTable
{
    partial class Program
    {

        static List<Word> wordList;

        struct Word
        {
            public string translated;
            public string pronounced;
        }

        //初始化引擎
        static bool Initialize()
        {

            string[] rawList;
            string[] parsed;
            string cmd;
            wordList = new List<Word>();            


            string currentPath = Environment.CurrentDirectory;

            //載入指令表
            try
            {

                rawList = File.ReadAllLines(currentPath + @"\ls.txt");
            }
            catch
            {
                Console.WriteLine("ERR(Command list does not exist or is corrupted. [AI])");

                return false;
            }


            int numLine = 1;
            string tmp; //條件式暫存
            Stack<string> currentScope = new Stack<string>();
            foreach (string line in rawList)
            {
                try
                {
                    if (line.Trim() != "" && !line.StartsWith("#")) //無視備註和空行
                    {
                        if (line.EndsWith("{"))
                        {
                            currentScope.Push(line.Trim('{'));
                        }
                        else if (line == "}")
                        {
                            currentScope.Pop();
                        }
                        else
                        {
                            parsed = line.Split(',');
                            cmd = line.Replace(parsed[0] + ",", "");
                            Word word;
                            if (currentScope.Count != 0)
                            {
                                if (cmd.Contains("?"))
                                { //組合條件式
                                    tmp = "";
                                    foreach (string cond in currentScope)
                                    {
                                        tmp = tmp + cond + "&";
                                    }
                                    word.translated = tmp + cmd;
                                }
                                else
                                {
                                    tmp = "";
                                    foreach (string cond in currentScope)
                                    {
                                        tmp = tmp + cond + "&";
                                    }
                                    word.translated = tmp.TrimEnd('&') + "?" + cmd;
                                }
                            }
                            else
                            {
                                word.translated = cmd;
                            }

                            //tilde handling
                            word.pronounced = parsed[0].Replace("~", "+~+").Trim('+');

                            wordList.Add(word);
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("ERR(Unable to parse line " + numLine.ToString() + " in word list. [AI])");

                    return false;
                }
                numLine++;
            }


            return true;


        }

        //對消息進行處理       

        static void ProcessC(string message)
        {
            LookUp(message, wordList);
        }

        static void LookUp(string msg, List<Word> dict)
        {
            string[] spt_AND; //AND is splited first, OR-first spliting is not necessary due to multi-triggering
            string[] spt_OR;
            string remaining_msg = "";

            string tilde = "";
            Queue<string> tildeVals = new Queue<string>();
            bool found = false;
            bool match = false;
            bool writeTilde = false;


            foreach (Word word in dict)
            {
                remaining_msg = msg.Trim();
                tilde = "";
                found = false;
                match = false;
                writeTilde = false;

                //special case "~" 
                if (word.pronounced == "~")
                {
                    Console.WriteLine(word.translated.Trim().Replace("~", msg));
                }
                //end 

                match = false;

                spt_AND = word.pronounced.Split('+', ' ');

                foreach (string part in spt_AND)
                {
                    if (part.Trim() == "")
                    {
                        continue;
                    }
                    if (part == "~")
                    {
                        tilde = remaining_msg;
                        writeTilde = true;
                        continue;
                    }

                    spt_OR = part.Split('/');

                    found = false;
                    foreach (string tWord in spt_OR)
                    {
                        if (remaining_msg.ToLower().Trim().Contains(tWord.ToLower().Trim()))
                        {
                            remaining_msg = RSplit(remaining_msg.Trim(), tWord.Trim());
                            found = true;

                            if (writeTilde)
                            {
                                tilde = LSplit(tilde.Trim(), tWord.Trim());
                                tildeVals.Enqueue(tilde);
                            }

                            writeTilde = false;
                            break;
                        }
                    }
                    if (found) { match = true; }

                    if (!found && match) { match = false; break; }

                }


                if (match)
                {
                    //Unclosed tilde
                    if (tilde != "")
                    {
                        tildeVals.Enqueue(tilde);
                    }

                    string cmd = word.translated.Trim();
                    int index = 0;
                    while (cmd.Contains('~'))
                    {
                        if (tildeVals.Count == 1)
                        {
                            cmd = cmd.Replace("~", tildeVals.Dequeue());
                        }
                        else
                        {
                            index = cmd.IndexOf('~');
                            cmd = cmd.Remove(index, 1);
                            cmd = cmd.Insert(index, tildeVals.Dequeue());
                        }
                    }

                    Console.WriteLine(cmd);
                    tildeVals.Clear();

                }
            }
        }

        //接下來是與 AZUSA 和其他引擎溝通的部分, 一般不用更改
        //============================================================
        static Thread AZUSAlistener;
        static int AZUSAPid;
        static bool AZUSAAlive = true;



        static void Main(string[] args)
        {
            if (!Initialize())
            {
                Console.WriteLine("ERR(AI Engine initialization failed.)");
                return;
            }


            AZUSAlistener = new Thread(new ThreadStart(ListenToConsole));
            AZUSAlistener.Start();
        }



        static void ListenToConsole()
        {

            Console.WriteLine("LinkRID(EVENT,true)");
            Console.WriteLine("LinkRID(INPUT,true)");
            Console.WriteLine("RegisterAs(AI)");

            Console.WriteLine("GetAzusaPid()");
            AZUSAPid = Convert.ToInt32(Console.ReadLine());

            string msg;

            //Listen for PortHasChanged

            while (AZUSAAlive)
            {
                try
                {
                    System.Diagnostics.Process.GetProcessById(AZUSAPid);
                    msg = Console.ReadLine().Trim();
                    ProcessC(msg);

                }
                catch
                {
                    AZUSAAlive = false;
                    Environment.Exit(0);
                    break;
                }
            }
        }
    }
}