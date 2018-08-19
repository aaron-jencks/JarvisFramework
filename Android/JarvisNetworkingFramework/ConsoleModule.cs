using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using JarvisClientFramework;
using ModuleFramework;
using System.Threading.Tasks;

namespace JarvisClientFramework
{
    public class ConsoleModule : Module
    { 

        public ConsoleModule(ref Queue<CommPacket> commQueue):base(ref commQueue)
        {

        }

        protected override void Module_MessageRxEvent(object sender, MessageRxEventArgs e)
        {
            base.Module_MessageRxEvent(sender, e);
            switch(e.Packet.Command)
            {
                case "Post":
                    Task.Factory.StartNew(() =>
                    {
                        ConsolePostEventArgs d = (ConsolePostEventArgs)e.Packet.Data;
                        OnConsolePostEvent(d);
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