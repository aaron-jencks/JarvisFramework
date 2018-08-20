using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using ModuleFramework;
using System.Threading;

namespace JarvisServerFramework
{
    public class NetworkingTCPServer : ModuleServer
    {
        #region Properties

        /// <summary>
        /// Current Port that the server is listening on
        /// </summary>
        public int Port { get; protected set; }

        /// <summary>
        /// Current IP that the server is listening on
        /// </summary>
        public IPAddress IP { get; protected set; }

        /// <summary>
        /// Boolean flag indicating if the server is running
        /// </summary>
        public bool isServerRunning { get; protected set; } = false;

        /// <summary>
        /// The server that does the physical listening for connections
        /// </summary>
        protected TcpListener Server { get; set; }

        /// <summary>
        /// Task for accepting new clients, this allows it to be cancelled when the server shuts down
        /// </summary>
        protected Task AwaitClientTask { get; set; }

        protected CancellationTokenSource AwaitClientTaskCancellationSource { get; set; } = new CancellationTokenSource();
        protected CancellationToken AwaitClientTaskCancellationToken { get; set; }

        /// <summary>
        /// List of clients currently logged into the server
        /// </summary>
        protected List<ServerClient> tcpClients { get; set; } = new List<ServerClient>(10);

        #endregion

        #region Constructor

        public NetworkingTCPServer(ref Queue<CommPacket> CommQueue):base(ref CommQueue)
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts the server given the corresponding local ip address and the port
        /// </summary>
        /// <param name="IP">IP address to use</param>
        /// <param name="Port">port to use</param>
        public virtual void ServerStart(IPAddress IP, int Port)
        {
            if (!isServerRunning)
            {
                this.IP = IP;
                this.Port = Port;

                Server = new TcpListener(IP, Port);
                Server.Start();
                isServerRunning = true;

                Task.Factory.StartNew(() => { OnServerStatusChangeEvent(); });

                foreach (ServerClient serverClient in tcpClients)
                    if (!serverClient.isRunning)
                        serverClient.Resume();
            }
        }

        /// <summary>
        /// Starts the server given the port number assuming the localhost address
        /// </summary>
        /// <param name="Port">Port to use</param>
        public virtual void ServerStart(int Port)
        {
            ServerStart(IPAddress.Parse("127.0.0.1"), Port);
        }

        /// <summary>
        /// Stops the server
        /// </summary>
        public virtual void ServerStop()
        {
            if(isServerRunning)
            {
                isServerRunning = false;

                try
                {
                    AwaitClientTask.Wait();     // Waits for the current listening task to be finished
                }
                catch (AggregateException e)
                {
                    Console.WriteLine("Stopping the waiting Task, this is not an error");
                }
                finally
                {
                    AwaitClientTaskCancellationSource.Dispose();
                }

                foreach (ServerClient serverClient in tcpClients)
                    serverClient.Halt();

                Server.Stop();

                Task.Factory.StartNew(() => { OnServerStatusChangeEvent(); });
            }
        }

        /// <summary>
        /// Allows peaceful stop of a client's connection from the server
        /// </summary>
        /// <param name="ID"></param>
        public virtual void ClientDisconnect(int ID)
        {

        }

        #endregion

        #region Threads

        /// <summary>
        /// Handles accepting connections
        /// </summary>
        protected virtual void ConnectionManager()
        {
            while(!isStopping)
            {
                if(isServerRunning && AwaitClientTask.IsCompleted)
                {
                    AwaitClientTaskCancellationSource = new CancellationTokenSource();
                    AwaitClientTaskCancellationToken = AwaitClientTaskCancellationSource.Token;

                    AwaitClientTask = new Task(() =>
                    {
                        AwaitClientTaskCancellationToken.ThrowIfCancellationRequested();

                        // Allows the task to be cancelled later if the server is stopped while it's still waiting for a new connection
                        Task.Factory.StartNew(() =>
                        {
                            while (!AwaitClientTaskCancellationToken.IsCancellationRequested)
                            {
                                AwaitClientTaskCancellationToken.ThrowIfCancellationRequested();
                            }
                        });

                        TcpClient client = Server.AcceptTcpClient();    // Waits for the next client to connect

                        ServerClient serverClient = new ServerClient(client);

                        serverClient.MessageRxEvent += ServerClient_MessageRxEvent; // Allows the new stream to send data to the server

                        tcpClients.Add(serverClient);
                        Task.Factory.StartNew(() => { OnNewConnectionEvent(serverClient); });

                    }, AwaitClientTaskCancellationToken);

                    AwaitClientTask.Start();
                }
                else if(!isServerRunning)
                {
                    if (!AwaitClientTask.IsCanceled)
                        AwaitClientTaskCancellationSource.Cancel();
                }
            }
        }

        #endregion

        #region Events

        #region Server Status

        public delegate void ServerStatusChangeEventHandler(object sender, ConnectionStatusEventArgs e);

        /// <summary>
        /// Triggers every time the server is either turned on, or off
        /// </summary>
        public event ServerStatusChangeEventHandler ServerStatusChangeEvent;

        /// <summary>
        /// Causes the ServerStatusChangeEvent to occur
        /// </summary>
        protected virtual void OnServerStatusChangeEvent()
        {
            ServerStatusChangeEvent?.Invoke(this, new ConnectionStatusEventArgs(isServerRunning));
        }

        #endregion

        #region New Connection

        public delegate void NewConnectionEventHandler(object sender, ConnectionEventArgs e);

        /// <summary>
        /// Triggered whenever a new client connects to the server
        /// </summary>
        public event NewConnectionEventHandler NewConnectionEvent;

        /// <summary>
        /// Causes the NewConnectionEvent to occur
        /// </summary>
        /// <param name="client">Client that has connected</param>
        protected virtual void OnNewConnectionEvent(ServerClient client)
        {
            NewConnectionEvent?.Invoke(this, new ConnectionEventArgs(client));
        }

        #endregion

        /// <summary>
        /// Triggered any time that a client sends a message to the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        private void ServerClient_MessageRxEvent(object sender, string message)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
