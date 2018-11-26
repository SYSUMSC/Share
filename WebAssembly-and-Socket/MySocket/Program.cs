using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MySocket
{
    class Program
    {

        static void Main(string[] argv)
        {
            var server = new Server(0, 23333);
            server.OnAccept += (obj, id) => Console.WriteLine($"Server: new client connected: {id}");
            server.OnReceive += (obj, id, len) => Console.WriteLine($"Server: received data from {id}: {Encoding.UTF8.GetString(server.ReadData(id, len))}");
            server.OnDisconnect += (obj, id) => Console.WriteLine($"Server: client disconnected: {id}");
            server.Start();
            Console.WriteLine("Socket started on 0.0.0.0:23333");
            while (true)
            {
                var host = Console.ReadLine();
                if (host == "quit") break;
                var serveraddr = host.Split(":");
                var client = new Client();
                client.OnDisconnect += obj => Console.WriteLine("Client: disconnected");
                client.OnReceive += (obj, len) => Console.WriteLine($"Client: received data from server: {Encoding.UTF8.GetString(client.ReadData(len))}");
                client.Connect(serveraddr[0], ushort.Parse(serveraddr[1]));
                var content = Console.ReadLine();
                client.Send(Encoding.UTF8.GetBytes(content));
                //client.Disconnect();
            }
            server.Stop();
            Console.ReadKey();
        }
    }
}
