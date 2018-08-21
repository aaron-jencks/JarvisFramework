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