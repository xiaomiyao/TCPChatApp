using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TCPChatApp.Common.Models;
using TCPChatApp.Server.DataAccess;

namespace TCPChatApp.Server
{
    public class ChatServer
    {
        private TcpListener _listener;
        private bool _isRunning;
        private List<TcpClient> _clients = new List<TcpClient>();

        // Remove the in‑memory store and use the repository instead
        public static UserRepository UserRepo;

        public void Start(int port = 5000)
        {
            // Initialize the repository (update the connection string as needed)
            UserRepo = new UserRepository("Server=MSI; Database=TCPChatApp; Trusted_Connection=True; Encrypt=True; TrustServerCertificate=True;");

            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            _isRunning = true;

            Console.WriteLine($"Server started on port {port}.");

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

                    var handler = new ClientHandler(client, _clients);
                    Thread clientThread = new Thread(handler.HandleClient);
                    clientThread.Start();
                }
                catch (SocketException)
                {
                    break;
                }
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _listener?.Stop();
        }
    }
}
