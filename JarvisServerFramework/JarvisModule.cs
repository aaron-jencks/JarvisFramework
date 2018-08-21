using ModuleFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace JarvisServerFramework
{
    public class JarvisModule : ConsoleModule
    {
        #region Properties

        protected NetworkingTCPServer TCPServer { get; set; }
        public bool UseJarvisTextHeader { get; set; } = true;


        #endregion

        #region Constructor

        public JarvisModule(ref Queue<CommPacket> commQueue, bool Start = true) : base(ref commQueue)
        {
            TCPServer = new NetworkingTCPServer(ref commQueue);
            if (Start)
                ServerStart();
        }

        #endregion

        #region Methods

        public override void Dispose()
        {
            base.Dispose();
            ServerStop();
            PostToConsole("Shutting Down...");
        }

        public virtual void ServerStart()
        {
            TCPServer.ServerStart();
            PostToConsole("Server Started at " + TCPServer.IP.ToString() + " on port " + TCPServer.Port);
        }

        public virtual void ServerStop()
        {
            TCPServer.ServerStop();
        }

        protected virtual void PostToConsole(string message, bool append = true, bool newline = true)
        {
            if (UseJarvisTextHeader)
                message = "Jarvis: " + message;

            OnConsolePostEvent(new ConsolePostEventArgs(message, append, newline));
        }

        #endregion

        #region Threads



        #endregion

        #region Events



        #endregion
    }
}
