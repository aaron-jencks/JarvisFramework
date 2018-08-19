using System;
using System.Collections.Generic;
using System.Threading;

namespace ModuleFramework
{
    public class Module
    {
        #region Properties

        /// <summary>
        /// The ID of the module, used for targetted messaging
        /// </summary>
        public int ID { get; protected set; }

        /// <summary>
        /// The current number of modules deployed
        /// </summary>
        public static int ModuleCount { get; protected set; } = 0;

        /// <summary>
        /// The communication Queue passed in when the module was created
        /// </summary>
        public Queue<CommPacket> CommQueue { get; protected set; }

        /// <summary>
        /// Queue used to send outgoing messages to other modules
        /// </summary>
        protected Queue<CommPacket> TxQueue { get; set; } = new Queue<CommPacket>(10);

        protected Thread CommManagerThread { get; set; }

        /// <summary>
        /// Flagged when the module exits
        /// </summary>
        protected bool isStopping { get; set; } = false;

        #endregion

        #region Constructors

        /// <summary>
        /// A template class that can be used to create a queued multi-threaded system.
        /// </summary>
        /// <param name="CommQueue">The universal Communication Queue used to link all of the modules together</param>
        public Module(ref Queue<CommPacket> CommQueue)
        {
            SetID();
            this.CommQueue = CommQueue;

            // Sets up the Rx decoding event
            MessageRxEvent += Module_MessageRxEvent;

            // Starts the Comm Manager
            CommManagerThread = new Thread(CommManager);
            CommManagerThread.Start();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the ID of the module
        /// </summary>
        protected virtual void SetID()
        {
            ID = ModuleCount++;
        }

        /// <summary>
        /// Stops the current processes, and clears the TxQueue
        /// </summary>
        public virtual void Dispose()
        {
            isStopping = true;

            while (CommManagerThread.IsAlive) ;

            TxQueue.Clear();
        }

        /// <summary>
        /// Sends a message to other modules
        /// </summary>
        /// <param name="packet">The comm packet to send</param>
        public virtual void SendMessage(CommPacket packet)
        {
            packet.AuthorID = ID;
            TxQueue.Enqueue(packet);
        }

        /// <summary>
        /// Sends a message to other modules
        /// </summary>
        /// <param name="message">The string message to send</param>
        /// <param name="data">The data to send</param>
        /// <param name="targetID">The targetted ID of the module to send to</param>
        public virtual void SendMessage(string message, object data = null, int targetID = -1)
        {
            SendMessage(new CommPacket(message, ID, data, targetID));
        }

        /// <summary>
        /// Occurs when the Module receives a message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Module_MessageRxEvent(object sender, MessageRxEventArgs e)
        {
            if (e.Packet.Command == "STOP")
                Dispose();
        }

        /// <summary>
        /// Returns the list of known commands for this module
        /// </summary>
        /// <returns></returns>
        public virtual string[] GetCommands()
        {
            return new string[] { "STOP" };
        }

        #endregion

        #region Threads

        /// <summary>
        /// Handles the reception and transmission of messages
        /// </summary>
        protected virtual void CommManager()
        {
            while(!isStopping)
            {
                lock (CommQueue)
                {
                    if (CommQueue.Count > 0)
                    {
                        if (CommQueue.Peek().TargetID == ID)
                        {
                            OnMessageRxEvent(CommQueue.Dequeue());
                        }
                    }
                }

                if(TxQueue.Count > 0)
                {
                    CommQueue.Enqueue(TxQueue.Dequeue());
                }

                Thread.Sleep(100);
            }
        }

        #endregion

        #region Events

        #region MessageRxEvent

        public delegate void MessageRxEventHandler(object sender, MessageRxEventArgs e);

        /// <summary>
        /// Triggered whenever a message is received by the RxManager
        /// </summary>
        public event MessageRxEventHandler MessageRxEvent;

        /// <summary>
        /// Triggers the MessageRxEvent for the provided packet
        /// </summary>
        /// <param name="packet">The comm packet that was received</param>
        protected virtual void OnMessageRxEvent(CommPacket packet)
        {
            MessageRxEvent?.Invoke(this, new MessageRxEventArgs(packet));
        }

        #endregion

        #endregion
    }
}
