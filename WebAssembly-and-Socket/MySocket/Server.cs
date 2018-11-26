using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MySocket
{
    class Server
    {
        private readonly TcpListener server;
        public delegate void OnAcceptEventHandler(object sender, string clientId);
        public event OnAcceptEventHandler OnAccept;
        public delegate void OnReceiveEventHandler(object sender, string clientId, int length);
        public event OnReceiveEventHandler OnReceive;
        private ConcurrentDictionary<string, Socket> clients = new ConcurrentDictionary<string, Socket>();
        
        public delegate void OnDisconnectEventHandler(object sender, string clientId);
        public event OnDisconnectEventHandler OnDisconnect;
        public bool IsStarted { get; set; } = false;
        public Server(long ip, ushort port)
        {
            server = new TcpListener(new IPAddress(ip), port);
        }

        public void Start()
        {
            if (server == null) throw new InvalidOperationException("Server not initialized");
            server.Start();
            IsStarted = true;
            new Thread(() => HandleIncomingConnection()).Start();
        }

        public void Stop()
        {
            var clientIds = clients.Select(i => i.Key);

            foreach (var id in clientIds)
            {
                Disconnect(id);
            }
            IsStarted = false;
            server.Stop();
        }

        public void Disconnect(string clientId)
        {
            if (clients.ContainsKey(clientId))
            {
                clients.TryRemove(clientId, out var client);
                client.Disconnect(false);
                client.Dispose();
            }
        }

        public int Send(string clientId, byte[] buffer)
        {
            if (clients.ContainsKey(clientId))
            {
                return clients[clientId].Send(buffer);
            }
            else throw new InvalidOperationException("Client not exists");
        }

        private void HandleReceiveData(string clientId)
        {
            while (clients.ContainsKey(clientId))
            {
                if (clients[clientId].Poll(1, SelectMode.SelectRead) && clients[clientId].Available == 0)
                {
                    Disconnect(clientId);
                    OnDisconnect?.Invoke(this, clientId);
                    return;
                }
                if (clients[clientId].Available > 0)
                {
                    OnReceive?.Invoke(this, clientId, clients[clientId].Available);
                }
                Thread.Sleep(50);
            }
        }

        public byte[] ReadData(string clientId, int length)
        {
            if (clients.ContainsKey(clientId))
            {
                var buffer = new byte[length];
                var len = clients[clientId].Receive(buffer, SocketFlags.None);
                return buffer.Take(len).ToArray();
            }
            return new byte[0];
        }

        private void HandleIncomingConnection()
        {
            while (IsStarted)
            {
                if (server.Pending())
                {
                    var id = Guid.NewGuid().ToString();
                    var client = server.AcceptSocket();
                    if (clients.TryAdd(id, client))
                    {
                        new Thread(() => HandleReceiveData(id)).Start();
                        OnAccept?.Invoke(this, id);
                        Send(id, Encoding.UTF8.GetBytes("Hello, i'm server!"));
                    }
                    else
                    {
                        client.Shutdown(SocketShutdown.Both);
                        client.Close();
                    }
                }
                Thread.Sleep(50);
            }
        }
    }
}