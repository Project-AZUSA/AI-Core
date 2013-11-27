using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ZMQ;

namespace AI_Boilerplate
{
    class Program
    {
        //set the resolution of the AI
        static int resolution = 1000;

        static void Process(List<string> messages)
        {
            //handle input signals here
        }


        static Thread AZUSAlistener;
        static string[] InputPorts;
        static bool PortChanged = false;
        static List<Socket> connections = new List<Socket>();
        static List<string> messages = new List<string>();

        static void Main(string[] args)
        {
            AZUSAlistener = new Thread(new ThreadStart(ListenToConsole));

            using (Context ctx = new Context())            
            {
                while (true)
                {
                    connections.Clear();

                    foreach (string port in InputPorts)
                    {
                        Socket client = ctx.Socket(SocketType.SUB);
                        client.Connect(port);
                        client.Subscribe("", Encoding.UTF8);

                        connections.Add(client);
                    }

                    PortChanged = false;

                    while (!PortChanged)
                    {                        
                        foreach (Socket socket in connections)
                        {
                            messages.Add(socket.Recv(Encoding.UTF8));
                        }

                        Process(messages);

                        messages.Clear();

                        System.Threading.Thread.Sleep(1000);

                    }
                }
                
            }
            
        }

        static void ListenToConsole()
        {
            Console.WriteLine("RegisterAs(AI)");

            //Listen for PortHasChanged

            while (true)
            {
                if (Console.ReadLine().Trim() == "PortHasChanged")
                {
                    Console.WriteLine("GetInputPorts()");
                    InputPorts = Console.ReadLine().Split(',');
                    PortChanged = true;
                }
            }
        }
    }
}
