using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarvisFramework
{
    public class ServerRxEventArgs : EventArgs
    {
        private string message;
        private long id;

        public ServerRxEventArgs(long ID, string message = "")
        {
            this.message = message;
            id = ID;
        }

        public string Message { get => message; set => message = value; }
        public long ID { get => id; set => id = value; }
    }

    public class ServerTxEventArgs : EventArgs
    {
        private string message;

        public ServerTxEventArgs(string message = "")
        {
            this.message = message;
        }

        public string Message { get => message; set => message = value; }
    }

    public class CommandParser
    {
        /// <summary>
        /// Parses commands out of the command string sent in.
        /// </summary>
        /// <param name="commandString">String of commands to be parsed</param>
        /// <returns>Returns a list of command clusters that were parsed out of the string</returns>
        public List<CommandCluster> ParseCommands(string commandString)
        {
            List<char> charList = commandString.ToList();
            return new List<CommandCluster>();
        }
    }

    /// <summary>
    /// Cluster used to house commands sent to Jarvis
    /// </summary>
    public class CommandCluster
    {
        #region Properties

        private string command;
        private object data;

        #endregion

        public CommandCluster(string command, object data = null)
        {
            this.command = command;
            this.data = data;
        }

        #region Accessor Methods

        /// <summary>
        /// Command to be executed
        /// </summary>
        public string Command { get => command; set => command = value; }

        /// <summary>
        /// Data sent with the command
        /// </summary>
        public object Data { get => data; set => data = value; }

        #endregion
    }
}
