using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Net;

namespace JarvisClientFramework
{
    public class ConnectionChangeEventArgs : EventArgs
    {
        public bool ConnectionStatus { get; set; }

        public ConnectionChangeEventArgs(bool ConnectionStatus)
        {
            this.ConnectionStatus = ConnectionStatus;
        }
    }

    public class TCPMessageEventArgs : EventArgs
    {
        public string Message { get; set; }

        public TCPMessageEventArgs(string message)
        {
            Message = message;
        }
    }

    public class ConnectionData
    {
        public IPAddress IP { get; set; }
        public int Port { get; set; }

        public ConnectionData(IPAddress IP, int Port)
        {
            this.IP = IP;
            this.Port = Port;
        }
    }

    public class ConsolePostEventArgs : EventArgs
    {
        public bool Append { get; set; }
        public bool Newline { get; set; }
        public string Message { get; set; }

        public ConsolePostEventArgs(string message, bool append = true, bool newline = true)
        {
            Message = message;
            Append = append;
            Newline = newline;
        }
    }
}