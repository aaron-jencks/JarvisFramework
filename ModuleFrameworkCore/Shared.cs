using System;

namespace ModuleFramework
{
    public class CommPacket
    {
        public string Command { get; set; }
        public object Data { get; set; }
        public int TargetID { get; set; }
        public int AuthorID { get; set; }

        public CommPacket(string command, int authorID, object data = null, int targetID = -1)
        {
            Command = command;
            Data = data ?? new object();
            TargetID = targetID;
            AuthorID = authorID;
        }
    }

    public class MessageRxEventArgs : EventArgs
    {
        public CommPacket Packet { get; set; }

        public MessageRxEventArgs(CommPacket packet)
        {
            Packet = packet;
        }
    }
}