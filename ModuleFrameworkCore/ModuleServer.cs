using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ModuleFramework
{
    public class ModuleServer : Module
    {
        #region Properties

        public List<int> ClientList { get; protected set; } = new List<int>(10);

        #endregion

        #region Constructors

        public ModuleServer(ref Queue<CommPacket> CommQueue) : base(ref CommQueue)
        {

        }

        #endregion

        #region Methods

        public virtual void Subscribe(Module module)
        {
            ClientList.Add(module.ID);
        }

        public virtual void Desubscribe(Module module)
        {
            if (ClientList.Contains(module.ID))
                ClientList.Remove(module.ID);
        }

        public override void SendMessage(CommPacket packet)
        {
            if (packet.TargetID == -1)
            {
                foreach(int id in ClientList)
                    base.SendMessage(packet.Command, packet.Data, id);
            }
            else
                base.SendMessage(packet);
        }

        #endregion

        #region Threads

        protected override void CommManager()
        {
            while (!isStopping)
            {
                lock (CommQueue)
                {
                    if (CommQueue.Count > 0)
                    {
                        if (CommQueue.Peek().TargetID == ID || CommQueue.Peek().TargetID == -1)
                        {
                            OnMessageRxEvent(CommQueue.Dequeue());
                        }
                    }
                }

                if (TxQueue.Count > 0)
                {
                    CommQueue.Enqueue(TxQueue.Dequeue());
                }

                Thread.Sleep(100);
            }
        }

        #endregion
    }
}
