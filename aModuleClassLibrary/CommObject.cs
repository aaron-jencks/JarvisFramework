using System;
using System.Collections.Generic;

namespace aModuleClassLibrary
{
    public class CommObject
    {
        #region Events

        public delegate void OnMsgRxHandler(object sender, EventArgs e);
        /// <summary>
        /// Occurs whenever a message is queued into the message system
        /// </summary>
        public event OnMsgRxHandler MsgRx;
        protected void OnMsgRx()
        {
            MsgRx?.Invoke(this, new EventArgs());
        }

        #endregion

        /// <summary>
        /// cluster used by the CommObject to determine whose queue belongs to whom
        /// </summary>
        private struct QueueCluster
        {
            /// <summary>
            /// Queue for the client
            /// </summary>
            public Queue<CommQueueMsg> Queue { get; }

            /// <summary>
            /// ID of the client that the queue belongs to
            /// </summary>
            public int ID { get; }

            public QueueCluster(int ID)
            {
                Queue = new Queue<CommQueueMsg>();
                this.ID = ID;
            }
        }

        #region Properties

        private List<QueueCluster> Clients;
        private static int ClientCount;
        private static int ServerCount = 0;
        public int ID { get; }

        #endregion

        #region Constructors

        public CommObject()
        {
            ID = ServerCount++;
            Clients = new List<QueueCluster>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Subscribes a client to the CommObject, creating it a queue and assigning it an ID.
        /// </summary>
        /// <param name="requestedID">The ID that the client has requested to occupy</param>
        /// <returns>Returns the ID that was assigned to the Client</returns>
        public int Subscribe(int requestedID = -1)
        {
            if(requestedID == -1)
            {
                Clients.Add(new QueueCluster(ClientCount++));
                return ClientCount;
            }
            else
            {
                if (Clients.Find((QueueCluster q) => { return q.ID == requestedID; }).ID == requestedID)
                    throw new InvalidOperationException("requested client ID was already taken");

                Clients.Add(new QueueCluster(requestedID));
                return requestedID;
            }
        }

        /// <summary>
        /// Desubscribes the Client from the CommObject, removes it's Queue and ID
        /// </summary>
        /// <param name="ID">ID of the client to remove</param>
        /// <returns>Returns a boolean status of success</returns>
        public bool DeSubscribe(int ID)
        {
            if (Clients.FindIndex((QueueCluster q) => { return q.ID == ID; }) >= 0)
            {
                Clients.RemoveAt(Clients.FindIndex((QueueCluster q) => { return q.ID == ID; }));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Fetches the Queue of the Client
        /// </summary>
        /// <param name="ID">ID of the client</param>
        /// <returns>The client's queue</returns>
        public Queue<CommQueueMsg> Queue(int ID)
        {
            return Clients.Find((QueueCluster q) => { return q.ID == ID; }).Queue;
        }

        /// <summary>
        /// Enqueues a global message to all subscribed clients
        /// </summary>
        /// <param name="msg">Message to send</param>
        public void GlobalEnQueue(CommQueueMsg msg)
        {
            foreach (QueueCluster q in Clients)
                q.Queue.Enqueue(msg);
            OnMsgRx();
        }

        /// <summary>
        /// Sends a message to a specific client
        /// </summary>
        /// <param name="ID">ID of the client to send to</param>
        /// <param name="msg">Message to be sent</param>
        /// <returns>Boolean status of success</returns>
        public bool TargetedEnQueue(int ID, CommQueueMsg msg)
        {
            if(Clients.Find((QueueCluster q) => { return q.ID == ID; }).ID == ID)
            {
                Clients.Find((QueueCluster q) => { return q.ID == ID; }).Queue.Enqueue(msg);
                OnMsgRx();
                return true;
            }
            return false;
        }

        #endregion
    }
}
