using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using ZeroMQ;

namespace LinkTable
{
    class Program
    {

        static List<Word> wordList;
        static List<Word> wordListC;

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
            wordListC = new List<Word>();


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
                            if (word.pronounced.StartsWith("@"))
                            {
                                word.pronounced = word.pronounced.TrimStart('@');
                                wordListC.Add(word);
                            }
                            else
                            {
                                wordList.Add(word);
                            }

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
        static void Process(List<string> messages)
        {

            foreach (string msg in messages)
            {

                LookUp(msg, wordList);
            }
        }

        static void ProcessC(string message)
        {
            LookUp(message, wordListC);
        }

        static void LookUp(string msg, List<Word> dict)
        {
            string[] spt_AND; //AND is splited first, OR-first spliting is not necessary due to multi-triggering
            string[] spt_OR;
            string[] spt_MSG = msg.Split(' ');
            int pointer = 0;
            string tilde = "";
            bool found = false;
            bool match = false;
            bool writeTilde = false;                     
            

            foreach (Word word in dict)
            {
                
                pointer = 0;
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

                spt_AND = word.pronounced.Split('+');

                foreach (string part in spt_AND)
                {
                    if (part.Trim() == "")
                    {
                        continue;
                    }

                    if (part == "~")
                    {
                        writeTilde = true;

                    }

                    spt_OR = part.Split('/');


                    if (pointer >= spt_MSG.Length) { match = false; }

                    for (int i = pointer; i < spt_MSG.Length; i++)
                    {
                        found = false;
                        foreach (string tWord in spt_OR)
                        {
                            if (spt_MSG[i].ToLower().Trim() == tWord.ToLower().Trim())
                            {
                                pointer = i + 1;
                                found = true;
                                writeTilde = false;
                                break;
                            }
                        }
                        if (found) { match = true; break; }

                        if (writeTilde)
                        {
                            tilde += spt_MSG[i] + " ";
                            pointer = i + 1;
                        }
                        else if (match)
                        {
                            match = false; break;
                        }


                    }

                    if (!match) { break; }

                }

                if (match)
                {
                    Console.WriteLine(word.translated.Replace("~", tilde.Trim()).Trim());

                }
            }
        }

        //接下來是與 AZUSA 和其他引擎溝通的部分, 一般不用更改
        //============================================================
        static Thread AZUSAlistener;
        static Thread ZMQ;
        static int AZUSAPid;
        static bool AZUSAAlive = true;

        static string[] InputPorts = new string[] { };
        static List<string> CurrentPorts = new List<string>();

        static ZmqSocket client;
        static List<string> messages = new List<string>();



        static void Main(string[] args)
        {
            if (!Initialize())
            {
                Console.WriteLine("ERR(AI Engine initialization failed.)");
                return;
            }

            ZMQ = new Thread(new ThreadStart(ZMQListener));
            ZMQ.Start();

            AZUSAlistener = new Thread(new ThreadStart(ListenToConsole));
            AZUSAlistener.Start();



        }

        static void ZMQListener()
        {
            using (var ctx = ZmqContext.Create())
            {
                while (AZUSAAlive)
                {
                    client = ctx.CreateSocket(SocketType.SUB);

                    while (InputPorts.Count() == 0)
                    {
                        Thread.Sleep(160);
                    }

                    foreach (string port in InputPorts)
                    {
                        if (port.Trim() != "")
                        {
                            Console.WriteLine("Connecting to " + port);
                            client.Connect(port);
                            Console.WriteLine("Connected to " + port);
                        }
                    }


                    client.Subscribe(Encoding.UTF8.GetBytes(""));




                    while (AZUSAAlive)
                    {

                        messages.Add(client.Receive(Encoding.UTF8));


                        if (messages.Count != 0)
                        {
                            Process(messages);
                        }

                        messages.Clear();
                    }
                }

            }
        }

        static void ListenToConsole()
        {


            Console.WriteLine("RegisterAs(AI)");
            Console.WriteLine("GetInputPorts()");
            InputPorts = Console.ReadLine().Split(',');


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
                    if (msg == "PortHasChanged")
                    {


                        Console.WriteLine("GetInputPorts()");
                        InputPorts = Console.ReadLine().Split(',');

                        foreach (string port in InputPorts)
                        {
                            if (port.Trim() != "" && !CurrentPorts.Contains(port))
                            {
                                Console.WriteLine("Connecting to " + port);
                                client.Connect(port);
                                CurrentPorts.Add(port);
                                Console.WriteLine("Connected to " + port);
                            }
                        }

                    }
                    else
                    {
                        ProcessC(msg);
                    }
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