using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace JarvisFramework
{
    public class Client
    {
        // Receive event triggered whenever a message is received
        public delegate void RxMessageEventHandler(object sender, string message);

        public event RxMessageEventHandler RxMessageEvent;

        protected virtual void OnRxMessageEvent(string message)
        {
            RxMessageEvent?.Invoke(this, message);
        }

        private TcpClient server;
        private int port;
        private IPAddress ip;
        private Thread Rx;
        private Thread Tx;
        private NetworkStream stream;
        private bool isExitting;
        private bool isReading;
        private bool isTransmitting;
        private Queue<string> TxQueue;

        public TcpClient ServerObj { get => server; set => server = value; }
        public int Port { get => port; set => port = value; }
        public IPAddress Ip { get => ip; set => ip = value; }

        public Client(int port, IPAddress ip)
        {
            this.port = port;
            this.ip = ip;
            server = new TcpClient();
            Rx = new Thread(new ThreadStart(RxManager));
            Tx = new Thread(new ThreadStart(TxManager));
            TxQueue = new Queue<string>();
            isReading = false;
            isTransmitting = false;
            isExitting = false;
        }

        public void Start()
        {
            Console.WriteLine("Attempting to connect to the host.");
            server.Connect(ip, port);
            stream = server.GetStream();
            Rx.Start();
            Tx.Start();
        }

        public void Close()
        {
            isExitting = true;
            byte[] msg = Encoding.ASCII.GetBytes("Connection.Close");
            stream.Write(msg, 0, msg.Length);
            while (isTransmitting || isReading) ;   // Wait until the other modules have exitted.
            stream.Close();
            server.Close();
        }

        public void RxManager()
        {
            // Handles received messages
            Console.WriteLine("Receiver manager initialized, awaiting incoming messages...");
            while (!isExitting)
            {
                Thread.Sleep(1000);

                isReading = true;
                byte[] bytes = new byte[256];
                // Checks if it can read
                int count = stream.Read(bytes, 0, bytes.Length);
                if (count > 0)
                {
                    // Found a message waiting to be read
                    string msg = Encoding.ASCII.GetString(bytes, 0, count);
                    OnRxMessageEvent(msg);  // Triggers the receive event if a message was found
                }
                isReading = false;
            }
        }

        public void TxManager()
        {
            // Handles Transmitted messges
            Console.WriteLine("Transmission manager initialized, awaiting outgoing messages...");
            while (!isExitting)
            {
                Thread.Sleep(1000);

                isTransmitting = true;
                if (TxQueue.Count > 0)
                {
                    // Transmits a message if there exists one in the queue
                    byte[] msg = Encoding.ASCII.GetBytes(TxQueue.Dequeue());
                    // Transmits to the host
                    try
                    {
                        stream.Write(msg, 0, msg.Length);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("Exception: {0}", e.Message);
                    }
                }
                isTransmitting = false;
            }
        }

        public void SendMessage(string message)
        {
            TxQueue.Enqueue(message);
        }
    }
}
