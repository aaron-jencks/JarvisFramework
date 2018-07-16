using JarvisFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server_Testbench
{
    class Program
    {
        static void Main(string[] args)
        {
            Jarvis j = new Jarvis();
            Thread.Sleep(1000);
            Console.Write("Port Number for client: ");
            int port = Convert.ToInt32(Console.ReadLine());
            Client client = new Client(port, new IPAddress(new byte[] { 127, 0, 0, 1 }));
            client.RxMessageEvent += OnRxMessageEvent;
            client.Start();
            Thread.Sleep(1000);
            Console.WriteLine("Client initialized, type away!");

            string msg = "";
            do
            {
                Thread.Sleep(100);
                Console.Write(">>> ");
                msg = Console.ReadLine();
                client.SendMessage(msg);
            } while (msg != "Connection.Close");
            j.ServerStop();
        }

        public static void OnRxMessageEvent(object sender, string message)
        {
            Console.WriteLine("Message from Jarvis: {0}", message);
        }
    }
}
