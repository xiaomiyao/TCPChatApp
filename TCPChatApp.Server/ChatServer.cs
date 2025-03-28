using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TCPChatApp.Common.Models;

namespace TCPChatApp.Server
{
    public class ChatServer
    {
        private TcpListener _listener;
        private bool _isRunning;
        private List<TcpClient> _clients = new List<TcpClient>();

        // 📝 In-memory store for registered users (for testing)
        public static List<User> RegisteredUsers = new List<User>();

        public void Start(int port = 5000)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            _isRunning = true;

            Console.WriteLine($"Server started on port {port}."); // 🚀 Server ready

            // 🧵 Start accepting clients
            Thread acceptThread = new Thread(AcceptClients);
            acceptThread.Start();
        }

        private void AcceptClients()
        {
            while (_isRunning)
            {
                try
                {
                    TcpClient client = _listener.AcceptTcpClient();
                    Console.WriteLine("➡️ Client connected!");

                    lock (_clients)
                    {
                        _clients.Add(client);
                    }

                    // 🔧 Handle client on a new thread
                    var handler = new ClientHandler(client, _clients);
                    Thread clientThread = new Thread(handler.HandleClient);
                    clientThread.Start();
                }
                catch (SocketException)
                {
                    break; // ❌ Listener stopped
                }
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _listener?.Stop(); // 🛑 Stop server
        }
    }
}
