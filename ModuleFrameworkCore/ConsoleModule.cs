using ModuleFramework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ModuleFramework
{
    public class ConsoleModule : Module
    {

        public ConsoleModule(ref Queue<CommPacket> commQueue) : base(ref commQueue)
        {

        }

        protected override void Module_MessageRxEvent(object sender, MessageRxEventArgs e)
        {
            base.Module_MessageRxEvent(sender, e);
            List<string> arguments = (List<string>)e.Packet.Data;
            switch (e.Packet.Command)
            {
                case "Post":
                    Task.Factory.StartNew(() =>
                    {
                        if (arguments.Count < 1)
                            SendMessage("Post", "Usage is: Post {Message} {Append?} {Newline?}");
                        else
                        {
                            ConsolePostEventArgs d;
                            if (arguments.Count == 1)
                                d = new ConsolePostEventArgs(arguments[0]);
                            else if (arguments.Count == 2)
                                d = new ConsolePostEventArgs(arguments[0], Convert.ToBoolean(arguments[1]));
                            else
                                d = new ConsolePostEventArgs(arguments[0], Convert.ToBoolean(arguments[1]), Convert.ToBoolean(arguments[2]));
                            OnConsolePostEvent(d);
                        }
                    });
                    break;

                case "Clear":
                    Task.Factory.StartNew(() =>
                    {
                        OnConsolePostEvent(new ConsolePostEventArgs("", false, false));
                    });
                    break;
            }
        }

        public override string[] GetCommands()
        {
            return new string[] { "STOP", "Post", "Clear" };
        }

        public delegate void ConsolePostEventHandler(object sender, ConsolePostEventArgs e);

        /// <summary>
        /// Occurs every time a string is attempting to be posted to the console
        /// </summary>
        public event ConsolePostEventHandler ConsolePostEvent;

        /// <summary>
        /// Causes the ConsolePostEvent to occur
        /// </summary>
        /// <param name="e">Posting parameters to use</param>
        protected virtual void OnConsolePostEvent(ConsolePostEventArgs e)
        {
            ConsolePostEvent?.Invoke(this, e);
        }

    }
}
