using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using TCPChatApp.Common.Models;
using TCPChatApp.Server.DataAccess;
using TCPChatApp.Common.Helpers;

namespace TCPChatApp.Server
{
    public class ChatServer
    {
        private TcpListener _listener;
        private bool _isRunning;
        public List<TcpClient> _clients = new List<TcpClient>();

        // Use the repository for user data
        private UserRepository UserRepo;
        private AuthenticationHandler AuthHandler;

        // New: Persistent online users list
        public List<User> OnlineUsers { get; set; } = new List<User>();

        public void Start(int port = 5000)
        {
            // Initialize the repository (update the connection string as needed)
            UserRepo = new UserRepository("Server=MSI; Database=TCPChatApp; Trusted_Connection=True; Encrypt=True; TrustServerCertificate=True;");
            AuthHandler = new AuthenticationHandler(this, UserRepo);

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

                    var handler = new ClientHandler(client, _clients, AuthHandler);
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

        // New: Broadcast the latest online users list to all connected clients
        public void BroadcastOnlineUsers()
        {
            var envelope = new Envelope
            {
                Type = "OnlineUsers",
                Users = OnlineUsers
            };

            string json = JsonSerializer.Serialize(envelope);
            string encryptedMessage = CryptoHelper.Encrypt(json);

            lock (_clients)
            {
                foreach (var client in _clients)
                {
                    try
                    {
                        var stream = client.GetStream();
                        using var writer = new System.IO.StreamWriter(stream, System.Text.Encoding.UTF8, 1024, leaveOpen: true)
                        {
                            AutoFlush = true
                        };
                        writer.WriteLine(encryptedMessage);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Broadcast error: {ex.Message}");
                    }
                }
            }
        }
    }
}
