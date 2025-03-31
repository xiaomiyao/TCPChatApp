using System.Net.Sockets;
using System.Text.Json;
using TCPChatApp.Common.Helpers;
using TCPChatApp.Common.Models;

namespace TCPChatApp.Server
{
    // does coordination of clients




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
        public void BroadcastMessage(Envelope envelope, ClientHandler? excludeClient = null)
        {
            lock (_clients)
            {
                foreach (var client in _clients)
                {
                    if (client == excludeClient || client.user == null)
                        continue;

                    try
                    {
                        client.WriteToClient(envelope);
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

            BroadcastMessage(envelope);
        }

        // 📩 Send a private message to a specific recipient
        public void SendPrivateMessage(string recipient, Envelope envelope)
        {
            lock (_clients)
            {
                var recipientClient = _clients.FirstOrDefault(client => client.user != null && client.user.Username == recipient);
                if (recipientClient != null)
                {
                    try
                    {
                        recipientClient.WriteToClient(envelope);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Private message error: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"⚠️ Recipient {recipient} not found.");
                }
            }
        }
    }
}