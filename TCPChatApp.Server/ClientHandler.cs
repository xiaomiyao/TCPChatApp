using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using TCPChatApp.Common.Helpers;
using TCPChatApp.Common.Models;

namespace TCPChatApp.Server
{
    public class ClientHandler
    {
        // 🔌 Connection objects
        private readonly TcpClient _client;
        private readonly List<TcpClient> _clients;

        public ClientHandler(TcpClient client, List<TcpClient> clients)
        {
            _client = client;
            _clients = clients;
        }

        public void HandleClient()
        {
            try
            {
                // 🔄 Setup streams
                using var networkStream = _client.GetStream();
                using var reader = new StreamReader(networkStream);
                using var writer = new StreamWriter(networkStream) { AutoFlush = true };

                // 👂 Message loop
                while (true)
                {
                    // 📥 Get message
                    string encryptedMessage = reader.ReadLine();
                    if (string.IsNullOrEmpty(encryptedMessage)) break;

                    // 🔓 Process message: decrypt then deserialize as Envelope
                    string plainText = CryptoHelper.Decrypt(encryptedMessage);
                    var envelope = JsonSerializer.Deserialize<Envelope>(plainText);

                    if (envelope != null && envelope.Type == "ChatMessage")
                    {
                        // 📝 Log the message
                        Console.WriteLine($"📨 Received from {envelope.Message.Sender}: {envelope.Message.Content}");

                        // 📢 Broadcast the envelope (as plain text)
                        BroadcastMessage(plainText);
                    }
                    else
                    {
                        Console.WriteLine("⚠️ Received envelope with unknown type or invalid format.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Client error: {ex.Message}");
            }
            finally
            {
                RemoveClient();
            }
        }

        private void BroadcastMessage(string plainTextMessage)
        {
            string encryptedMessage = CryptoHelper.Encrypt(plainTextMessage);
            lock (_clients)
            {
                foreach (var client in _clients)
                {
                    if (client == _client) continue; // Skip sender

                    try
                    {
                        // 📤 Send message
                        var stream = client.GetStream();
                        using var writer = new StreamWriter(stream, System.Text.Encoding.UTF8, 1024, leaveOpen: true)
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

        private void RemoveClient()
        {
            lock (_clients)
            {
                _clients.Remove(_client);
            }
            _client.Close(); // 👋 Cleanup
        }
    }
}
