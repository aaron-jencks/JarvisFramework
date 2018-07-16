using System;
using System.Collections.Generic;
using System.Text;

namespace aModuleClassLibrary
{
    /// <summary>
    /// Client for modules to interface with the CommObject
    /// </summary>
    public class CommClient
    {
        public struct ServerCluster
        {
            public CommObject Server { get; set; }
            public int ID { get; set; }

            public ServerCluster(CommObject Server)
            {
                this.Server = Server;
                ID = Server.Subscribe();
            }
        }

        #region Events

        public delegate void OnMsgRxHandler(object sender, EventArgs e);
        /// <summary>
        /// Fired whenever this client receives a message.
        /// </summary>
        public event OnMsgRxHandler MsgRx;
        protected void OnMsgRx()
        {
            MsgRx?.Invoke(this, new EventArgs());
        }

        #endregion

        #region Properties

        public List<ServerCluster> Servers { get; set; }

        #endregion

        #region Constructors

        public CommClient(CommObject server = null)
        {
            if(server!=null)
            {
                Servers.Add(new ServerCluster(server));
                server.MsgRx += OnMsgRxServ;
            }
        }

        #endregion

        #region Methods

        public void OnMsgRxServ(object sender, EventArgs e)
        {
            foreach (ServerCluster Server in Servers)
                if (Server.Server.Queue(Server.ID).Count > 0)
                {
                    OnMsgRx();
                    break;
                }
        }

        /// <summary>
        /// Sends a global message to every client
        /// </summary>
        /// <param name="msg">Message to send</param>
        public void GlobalTx(CommQueueMsg msg)
        {
            foreach(ServerCluster Server in Servers)
                Server.Server.GlobalEnQueue(msg);
        }

        /// <summary>
        /// Sends a message to a specific client
        /// </summary>
        /// <param name="ID">ID of the client to send to</param>
        /// <param name="msg">Message to send</param>
        /// <returns>Boolean status of success</returns>
        public bool TargetedTx(int ID, CommQueueMsg msg)
        {
            foreach (ServerCluster Server in Servers)
                if (Server.Server.TargetedEnQueue(ID, msg))
                    return true;
            return false;
        }

        /// <summary>
        /// Connects to a new server
        /// </summary>
        /// <param name="server">Server to connect to</param>
        /// <returns>returns boolean status of success</returns>
        public bool ServerConnect(CommObject server)
        {
            if (Servers.Find((ServerCluster s) => { return s.Server.ID == server.ID; }).Server.ID == server.ID)
                return false;
            Servers.Add(new ServerCluster(server));
            server.MsgRx += OnMsgRxServ;
            return true;
        }

        /// <summary>
        /// Disconnects from a server
        /// </summary>
        /// <param name="server">server to disconnect from</param>
        /// <returns>returns boolean status of success</returns>
        public bool ServerDisconnect(CommObject server)
        {
            if (Servers.Find((ServerCluster s) => { return s.Server.ID == server.ID; }).Server.ID == server.ID)
            {
                ServerCluster tempServer = Servers.Find((ServerCluster s) => { return s.Server.ID == server.ID; });
                tempServer.Server.DeSubscribe(tempServer.ID);
                Servers.Remove(tempServer);
                return true;
            }
            return false;
        }

        #endregion
    }
}
