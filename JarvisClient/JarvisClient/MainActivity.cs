using System;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using ModuleFramework;
using System.Collections.Generic;
using JarvisClientFramework;

namespace JarvisClient
{
	[Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
	public class MainActivity : AppCompatActivity
	{
        #region Properties

        private EditText CommandBar;
        private TextView titleLabel;
        private TextView consoleOutput;

        private ConsoleModule ConsoleClient;
        private ModuleServer ClientServer;
        private NetworkingTCPClient TCPClient;
        private Queue<CommPacket> CommQueue = new Queue<CommPacket>(10);

        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
		{
            #region Android Stuff

            base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.activity_main);

			Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

			FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            #endregion

            consoleOutput = FindViewById<TextView>(Resource.Id.ConsoleOutput);
            titleLabel = FindViewById<TextView>(Resource.Id.titleLabel);
            CommandBar = FindViewById<EditText>(Resource.Id.CommandBar);

            // Sets up the base console system
            ClientServer = new ModuleServer(ref CommQueue);
            int id = ClientServer.ID;
            ConsoleClient = new ConsoleModule(ref CommQueue);
            id = ConsoleClient.ID;
            ConsoleClient.ConsolePostEvent += ConsoleClient_MessageRxEvent;
            ClientServer.Subscribe(ConsoleClient);

            // Sets up the networking and port system
            TCPClient = new NetworkingTCPClient(ref CommQueue);
            ClientServer.Subscribe(TCPClient);

            CommandBar.KeyPress += (object sender, View.KeyEventArgs e) => {
                e.Handled = false;
                if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
                {
                    Toast.MakeText(this, "Message Transmitting!", ToastLength.Short).Show();
                    e.Handled = true;

                    ClientServer.SendMessage("Post", new List<string>() { "Command: " + CommandBar.Text }, ConsoleClient.ID);   // Updates the console

                    List<string> arguments = CommandSyntaxInterpreter.ParseMessageBySpace(CommandBar.Text); // Finds the arguments based on the location of spaces
                    string command = arguments[0];                                                          // Extracts the actual command
                    arguments.RemoveAt(0);                                                                  // Removes the command from the list of arguments

                    ClientServer.SendMessage(command, arguments);                                           // Sends the command
                    CommandBar.Text = "";
                }
            };
        }

        private void ConsoleClient_MessageRxEvent(object sender, ConsolePostEventArgs e)
        {
            RunOnUiThread(() =>
            {
                if (e.Append)
                    if(e.Newline)
                        consoleOutput.Append(e.Message + "\n");
                    else
                        consoleOutput.Append(e.Message);
                else
                    if(e.Newline)
                        consoleOutput.Text = e.Message + "\n";
                    else
                        consoleOutput.Text = e.Message;
            });
        }

        #region Android stuff

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View) sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }

        #endregion
    }
}

