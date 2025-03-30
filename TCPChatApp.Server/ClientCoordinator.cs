using System.Text.Json;
using TCPChatApp.Common.Helpers;
using TCPChatApp.Common.Models;

namespace TCPChatApp.Server
{
    public class ClientCoordinator
    {
        // 👥 List of connected clients
        private readonly List<ClientHandler> _clients = new();

        // ➕ Add new client connection
        public void AddClient(ClientHandler clientHandler)
        {
            lock (_clients)
            {
                _clients.Add(clientHandler);
            }
        }

        // ➖ Remove disconnected client
        public void RemoveClient(ClientHandler clientHandler)
        {
            lock (_clients)
            {
                _clients.Remove(clientHandler);
            }
        }

        // 👥 Get list of currently online users
        public List<User> GetOnlineUsers()
        {
            lock (_clients)
            {
                return [.. _clients
                    .Where(client => client.user != null)
                    .Select(client => client.user)];
            }
        }

        // 📢 Broadcast message to connected clients
        public void BroadcastMessage(string encryptedMessage, ClientHandler? excludeClient = null)
        {
            lock (_clients)
            {
                foreach (var client in _clients)
                {
                    if (client == excludeClient)
                        continue;

                    try
                    {
                        var stream = client._client.GetStream();
                        using var writer = new StreamWriter(stream, System.Text.Encoding.UTF8, 1024, leaveOpen: true)
                        { AutoFlush = true };
                        writer.WriteLine(encryptedMessage);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Broadcast error: {ex.Message}");
                    }
                }
            }
        }

        // 📢 Broadcast updated online users list
        public void BroadcastOnlineUsers()
        {
            var envelope = new Envelope
            {
                Type = "OnlineUsers",
                Users = GetOnlineUsers()
            };

            string json = JsonSerializer.Serialize(envelope);
            string encryptedMessage = CryptoHelper.Encrypt(json);

            BroadcastMessage(encryptedMessage);
        }
    }
}