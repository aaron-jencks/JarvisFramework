using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModuleFramework;
using JarvisServerFramework;

namespace JarvisServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Jarvis: Hello!\nBooting Up...");
            Queue<CommPacket> GlobalCommQueue = new Queue<CommPacket>(10);
            JarvisModule Jarvis = new JarvisModule(ref GlobalCommQueue, false);
            Jarvis.ConsolePostEvent += Jarvis_ConsolePostEvent;
            Jarvis.ServerStart();
        }

        private static void Jarvis_ConsolePostEvent(object sender, ConsolePostEventArgs e)
        {
            if(e.Append)
            {
                if (e.Newline)
                    Console.WriteLine(e.Message);
                else
                    Console.Write(e.Message);
            }
            else
            {
                Console.Clear();

                if (e.Newline)
                    Console.WriteLine(e.Message);
                else
                    Console.Write(e.Message);
            }
        }
    }
}
