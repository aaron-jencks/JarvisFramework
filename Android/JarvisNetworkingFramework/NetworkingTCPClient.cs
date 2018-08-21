using System;
using System.Collections.Generic;
using ModuleFramework;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System.Text;

namespace JarvisClientFramework
{
    public class NetworkingTCPClient : Module
    {
        #region Properties

        /// <summary>
        /// Current IP that the module is connected to
        /// </summary>
        public IPAddress IP { get; protected set; }

        /// <summary>
        /// Boolean flag indicating if the module has connected to the server yet
        /// </summary>
        public bool isConnected { get; protected set; } = false;

        /// <summary>
        /// Current Port that the module is connected to
        /// </summary>
        public int Port { get; protected set; }

        /// <summary>
        /// The phrase to be sent at the end of every transmission to signify the end of the phrase.
        /// </summary>
        public string TerminatingPhrase { get; set; } = "{[END?]}";

        /// <summary>
        /// Buffer of received characters that don't yet contain the Terminating Phrase
        /// </summary>
        protected string RxBuffer { get; set; } = "";

        /// <summary>
        /// The TCP client being used by the module
        /// </summary>
        protected TcpClient Client { get; set; }

        protected bool isStreamOpen { get; set; } = false;

        /// <summary>
        /// Queue to hold strings of data to be sent to the server
        /// </summary>
        protected Queue<string> PortTxQueue { get; set; } = new Queue<string>(10);

        /// <summary>
        /// Thread for the Port listener and writer
        /// </summary>
        protected Thread PortCommManagerThread { get; set; }

        #endregion

        public NetworkingTCPClient(ref Queue<CommPacket> commQueue) : base(ref commQueue)
        {
            PortCommManagerThread = new Thread(PortCommManager);
            PortCommManagerThread.Start();
        }

        #region Methods

        /// <summary>
        /// Attempts to connect the module to the given address
        /// </summary>
        /// <param name="ip">IP to connect to</param>
        /// <param name="port">port to connect to</param>
        /// <returns>boolean indicator of success</returns>
        public virtual bool Connect(IPAddress ip, int port)
        {
            if (isConnected)
                return false;
            else
            {
                Client = new TcpClient();
                IP = ip;
                Port = port;
                SendMessage(new CommPacket("Post", ID, new List<string>() { "Connecting..." }));
                Client.Connect(ip, port);
                isConnected = Client.Connected;
                Task.Factory.StartNew(() => { OnConnectionChangeEvent(); });
                return isConnected;
            }
        }

        /// <summary>
        /// Appends the message to the buffer, then searches for the terminating phrase,
        /// if found, it returns the complete phrase in message and then the unused section is left in buffer
        /// </summary>
        /// <param name="message">message received/found</param>
        /// <param name="buffer">the standing buffer for previous messages</param>
        /// <returns>returns a boolean indicating whether a match was found</returns>
        protected virtual bool SearchForTermination(ref string message)
        {
            string temp = RxBuffer + message;
            if (temp.Contains(TerminatingPhrase))
            {
                int index = temp.IndexOf(TerminatingPhrase);
                message = temp.Substring(0, index);
                temp.Remove(0, index + TerminatingPhrase.Length);
                RxBuffer = temp;
                return true;
            }
            else
            {
                RxBuffer = temp;
                return false;
            }
        }

        /// <summary>
        /// Attempts to disconnect the module from the current address
        /// </summary>
        /// <returns>Returns a boolean indicator of success</returns>
        public virtual bool Disconnect()
        {
            if (isConnected)
            {
                isConnected = false;
                while (isStreamOpen) ;

                Client.Close();

                Task.Factory.StartNew(() => { OnConnectionChangeEvent(); });

                return true;
            }
            else
                return false;
        }

        protected override void Module_MessageRxEvent(object sender, MessageRxEventArgs e)
        {
            base.Module_MessageRxEvent(sender, e);
            List<string> arguments = (List<string>)e.Packet.Data;
            switch(e.Packet.Command)
            {
                case "Connect":
                    Task.Factory.StartNew(() => {
                        if (arguments.Count < 2)
                        {
                            SendMessage("Post", new List<string>() { "Invalid number of arguments" });
                            SendMessage("Post", new List<string>() { "Usage is: Connect {IPAddress} {Port}" });
                        }
                        else
                        {
                            ConnectionData d = new ConnectionData(IPAddress.Parse(arguments[0]), Convert.ToInt32(arguments[1]));
                            Connect(d.IP, d.Port);
                        }
                    });
                    break;

                case "Disconnect":
                    Task.Factory.StartNew(() => {
                        Disconnect();
                    });
                    break;

                case "TerminatingPhrase":
                    if (arguments.Count < 1)
                        SendMessage("Post", new List<string>() { "Usage is: TerminatingPhrase {New Phrase}" });
                    else
                        TerminatingPhrase = arguments[0];
                    break;
            }
        }

        /// <summary>
        /// Gets the available commands for this module.
        /// </summary>
        /// <returns></returns>
        public override string[] GetCommands()
        {
            return new string[] { "STOP", "Connect", "Disconnect", "TerminatingPhrase" };
        }

        #endregion

        #region Threads

        /// <summary>
        /// Handles the transmission and reception of data from the TCP connection
        /// </summary>
        protected virtual void PortCommManager()
        {
            while(!isStopping)
            {
                if(isConnected)
                {
                    isStreamOpen = true;

                    NetworkStream stream = Client.GetStream();

                    // Handles transmission
                    if (PortTxQueue.Count > 0)
                    {
                        for (int i = 0; i < PortTxQueue.Count; i++)
                        {
                            string message = PortTxQueue.Dequeue();

                            byte[] data = Encoding.ASCII.GetBytes(message + TerminatingPhrase);

                            stream.Write(data, 0, data.Length);

                            Task.Factory.StartNew(() => { OnTCPMessageTXEvent(message); });
                        }
                    }

                    // Handles reception
                    if(stream.DataAvailable)
                    {
                        byte[] data = new byte[256];

                        int bytes = stream.Read(data, 0, data.Length);

                        string message = Encoding.ASCII.GetString(data, 0, bytes);
                        if (SearchForTermination(ref message))
                        {
                            Task.Factory.StartNew(() => { OnTCPMessageRxEvent(message); });
                        }
                    }

                    stream.Close();

                    isStreamOpen = false;
                }
            }
        }

        #endregion

        #region Events

        #region Connection Change

        public delegate void ConnectionChangeEventHandler(object sender, ConnectionChangeEventArgs e);

        /// <summary>
        /// Triggered everytime the module connects or disconnects from the server
        /// </summary>
        public event ConnectionChangeEventHandler ConnectionChangeEvent;

        /// <summary>
        /// Causes the ConnectionChangeEvent to occur
        /// </summary>
        protected virtual void OnConnectionChangeEvent()
        {
            ConnectionChangeEvent?.Invoke(this, new ConnectionChangeEventArgs(isConnected));
        }

        #endregion

        #region TCP Message Send

        public delegate void TCPMessageTxEventHandler(object sender, TCPMessageEventArgs e);

        /// <summary>
        /// Triggered every time the module writes data to the port
        /// </summary>
        public event TCPMessageTxEventHandler TCPMessageTxEvent;

        /// <summary>
        /// Causes the TCPMessageTxEvent to occur
        /// </summary>
        /// <param name="message">Message that was sent</param>
        protected virtual void OnTCPMessageTXEvent(string message)
        {
            TCPMessageTxEvent?.Invoke(this, new TCPMessageEventArgs(message));
        }

        #endregion

        #region TCP Message Receive

        public delegate void TCPMessageRxEventHandler(object sender, TCPMessageEventArgs e);

        /// <summary>
        /// Triggered every time that a message is received at the port
        /// </summary>
        public event TCPMessageRxEventHandler TCPMessageRxEvent;

        /// <summary>
        /// Causes the TCPMessageRxEvent to occur
        /// </summary>
        /// <param name="message">The message that was received</param>
        protected virtual void OnTCPMessageRxEvent(string message)
        {
            TCPMessageRxEvent?.Invoke(this, new TCPMessageEventArgs(message));
        }

        #endregion

        #endregion
    }
}
