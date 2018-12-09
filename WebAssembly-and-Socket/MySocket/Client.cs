using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MySocket
{
    public class Client
    {
        private readonly TcpClient client;
        public delegate void OnDisconnectEventHandler(object sender);
        public event OnDisconnectEventHandler OnDisconnect;
        public delegate void OnReceiveEventHandler(object sender, int length);
        public event OnReceiveEventHandler OnReceive;
        public bool IsConnected
        {
            get
            {
                if (client.Client != null)
                    return !(client.Client.Poll(1, SelectMode.SelectRead) && client.Available == 0);
                else return false;
            }
        }
        public Client()
        {
            client = new TcpClient();
        }

        private void DetectConnectivity()
        {
            while (IsConnected) Thread.Sleep(50);
            OnDisconnect?.Invoke(this);
        }

        private void HandleReceiveData()
        {
            while (IsConnected)
            {
                if (client.Available > 0)
                {
                    OnReceive?.Invoke(this, client.Available);
                }
                Thread.Sleep(50);
            }
        }

        public int Send(byte[] buffer)
        {
            if (IsConnected) return client.Client.Send(buffer);
            else throw new InvalidOperationException("Server not connected");
        }

        public byte[] ReadData(int length)
        {
            if (IsConnected)
            {
                var buffer = new byte[length];
                client.Client.Receive(buffer);
                return buffer.Take(length).ToArray();
            }
            else return new byte[0];
        }

        public void Connect(string hostname, ushort port)
        {
            if (client == null) throw new InvalidOperationException("Client not initialized");
            client.Connect(hostname, port);
            new Thread(() => DetectConnectivity()).Start();
            new Thread(() => HandleReceiveData()).Start();
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                client.Client.Disconnect(false);
                client.Dispose();
            }
        }
    }
}