using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Linq;

namespace JarvisFramework
{
    public class NetUser
    {
        private string username;
        private string password;
        private TcpClient client;
        private NetworkStream stream;
        private long id;
        private static long userCount = 0;
        private bool isConnected = false;

        public string Username { get => username; }
        public TcpClient Client { get => client; }
        public NetworkStream Stream { get => stream; set => stream = value; }
        public long Id { get => id; set => id = value; }
        public bool IsConnected { get => isConnected; set => isConnected = value; }

        public NetUser(string username, string password, TcpClient client)
        {
            this.username = username;
            this.password = password;
            this.client = client;
            stream = client.GetStream();
            id = userCount++;
            isConnected = true;
        }

        public NetUser(string username, string password)
        {
            this.username = username;
            this.password = password;
            id = userCount++;
        }

        public void UpdateClient(TcpClient client)
        {
            this.client = client;
            stream = client.GetStream();
            isConnected = true;
        }

        public void Close()
        {
            if (isConnected)
            {
                stream.Close();
                client.Close();
                isConnected = false;
            }
        }

        public bool CheckPassword(string password)
        {
            return this.password == password;
        }
    }
}
