using aModuleClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace JarvisClient_Android_Current
{
	public partial class MainPage : ContentPage
	{
        private CommObject CommServer;
        private CommClient CommClient;

		public MainPage()
		{
			InitializeComponent();

            CommServer = new CommObject();

            CommClient = new CommClient(CommServer);
            CommClient.MsgRx += OnCommMsgRx;

            commandBar.TextChanged += CommandBar_TextChanged;
		}

        private void CommandBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            CommServer.GlobalEnQueue(new CommQueueMsg(commandBar.Text));
        }

        public void OnCommMsgRx(object sender, EventArgs e)
        {
            consoleOutput.Text += "Received a message\n";
        }
	}
}
