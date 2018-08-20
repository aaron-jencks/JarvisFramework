using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace JarvisServerFramework
{
    public class ServerClient
    {

        #region Properties

        public TcpClient Client { get; protected set; }
        public NetworkStream Stream { get; protected set; }
        public int ID { get; protected set; }
        public static int ClientCount { get; protected set; } = 0;
        protected Task StreamReadTask { get; set; }
        protected CancellationTokenSource StreamReadCancellationSource { get; set; } = new CancellationTokenSource();
        protected CancellationToken StreamReadCancellationToken { get; set; }
        public bool isRunning { get; protected set; } = false;

        #endregion

        #region Constructor

        public ServerClient(TcpClient client)
        {
            Client = client;
            Stream = Client.GetStream();
            ID = ClientCount++;

            StreamReadCancellationSource = new CancellationTokenSource();
            StreamReadCancellationToken = StreamReadCancellationSource.Token;

            StreamReadTask = new Task(StreamReadManager, StreamReadCancellationToken);
            StreamReadTask.Start();

            isRunning = true;
        }

        #endregion

        #region Methods

        public virtual void Dispose()
        {
            Halt();
            Stream.Dispose();
            Client.Close();
        }

        public virtual void Halt()
        {
            if (isRunning)
            {
                isRunning = false;
                StreamReadCancellationSource.Cancel();
                while (!StreamReadTask.IsCompleted) ;
                StreamReadCancellationSource.Dispose();
            }
        }

        public virtual void Resume()
        {
            if(!isRunning)
            {
                StreamReadCancellationSource = new CancellationTokenSource();
                StreamReadCancellationToken = StreamReadCancellationSource.Token;

                StreamReadTask = new Task(StreamReadManager, StreamReadCancellationToken);
                StreamReadTask.Start();

                isRunning = true;
            }
        }

        #endregion

        #region Threads

        /// <summary>
        /// Continuously reads in data until the process is cancelled by the server upon exit
        /// </summary>
        protected virtual void StreamReadManager()
        {
            byte[] data = new byte[256];
            int bytesAvailable = 0;

            while(!StreamReadCancellationToken.IsCancellationRequested)
            {
                if((bytesAvailable = Stream.Read(data, 0, data.Length))!=0)
                    Task.Factory.StartNew(() => { OnMessageRxEvent(Encoding.ASCII.GetString(data, 0, bytesAvailable)); });
            }
        }

        #endregion

        #region Events

        public delegate void MessageRxEventHandler(object sender, string message);

        public event MessageRxEventHandler MessageRxEvent;

        protected virtual void OnMessageRxEvent(string message)
        {
            MessageRxEvent?.Invoke(this, message);
        }

        #endregion
    }

    public class ConnectionEventArgs : EventArgs
    {
        public ServerClient Client { get; protected set; }

        public ConnectionEventArgs(ServerClient Client)
        {
            this.Client = Client;
        }
    }

    public class ConnectionStatusEventArgs : EventArgs
    {
        public bool Status { get; protected set; }

        public ConnectionStatusEventArgs(bool status)
        {
            Status = status;
        }
    }

    public class TcpTxMessage
    {
        public string Message { get; set; }
        public int TargetID { get; set; }

        public TcpTxMessage(string message, int targetID = -1)
        {
            Message = message;
            TargetID = targetID;
        }
    }
}
