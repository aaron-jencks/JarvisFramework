using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using ModuleFramework;

namespace JarvisClientCurrent
{
    public partial class MainPage : ContentPage
    {
        private Module CommServer;
        private Module CommClient;
        private Queue<CommPacket> CommQueue = new Queue<CommPacket>(10);

        public MainPage()
        {
            InitializeComponent();

            CommServer = new Module(ref CommQueue);

            CommClient = new Module(ref CommQueue);
            CommClient.MessageRxEvent += (object sender, MessageRxEventArgs e) => OnCommMsgRx(sender, e);

            commandBar.Completed += CommandBar_Completed;
        }

        private void CommandBar_Completed(object sender, EventArgs e)
        {
            CommServer.SendMessage(new CommPacket(commandBar.Text));
        }

        public void OnCommMsgRx(object sender, MessageRxEventArgs e)
        {
            //runOnUiThread()
            //ConsoleCallback.Invoke("Received a message");
        }
    }
}

