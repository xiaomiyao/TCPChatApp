using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

                    if (envelope != null)
                    {
                        if (envelope.Type == "ChatMessage")
                        {
                            // 📝 Log the message
                            Console.WriteLine($"📨 Received from {envelope.Message.Sender}: {envelope.Message.Content}");

                            // 📢 Broadcast the envelope (as plain text)
                            BroadcastMessage(plainText);
                        }
                        else if (envelope.Type == "Register")
                        {
                            Console.WriteLine("🔐 Registration request received.");

                            // Validate registration details (User object must contain username and password hash)
                            if (envelope.User == null ||
                                string.IsNullOrWhiteSpace(envelope.User.Username) ||
                                string.IsNullOrWhiteSpace(envelope.User.PasswordHash))
                            {
                                Console.WriteLine("⚠️ Registration failed: invalid user details.");
                                SendRegistrationResponse(writer, "Registration failed: Invalid user details.");
                            }
                            else
                            {
                                // Check for duplicate registration
                                var existingUser = ChatServer.RegisteredUsers
                                    .FirstOrDefault(u => u.Username.Equals(envelope.User.Username, StringComparison.OrdinalIgnoreCase));

                                if (existingUser != null)
                                {
                                    Console.WriteLine("⚠️ Registration failed: username already exists.");
                                    SendRegistrationResponse(writer, "Registration failed: Username already exists.");
                                }
                                else
                                {
                                    // Add new user to the in-memory store
                                    ChatServer.RegisteredUsers.Add(envelope.User);
                                    Console.WriteLine($"✅ User '{envelope.User.Username}' registered successfully.");
                                    SendRegistrationResponse(writer, "Registration successful.");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("⚠️ Received envelope with unknown type or invalid format.");
                        }
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

        /// <summary>
        /// Sends a registration response back to the client.
        /// </summary>
        private void SendRegistrationResponse(StreamWriter writer, string responseMessage)
        {
            // 📦 Build response envelope for registration
            var responseEnvelope = new Envelope
            {
                Type = "RegistrationResponse",
                Message = new Message
                {
                    Sender = "Server",      // 🔧 Sender info
                    Content = responseMessage  // 📝 Response content
                }
            };

            // 🔒 Encrypt and send the registration response
            string jsonResponse = JsonSerializer.Serialize(responseEnvelope);
            string encryptedResponse = CryptoHelper.Encrypt(jsonResponse);
            writer.WriteLine(encryptedResponse);
        }

        private void BroadcastMessage(string plainTextMessage)
        {
            // 🔒 Encrypt the plain text message
            string encryptedMessage = CryptoHelper.Encrypt(plainTextMessage);
            lock (_clients)
            {
                foreach (var client in _clients)
                {
                    if (client == _client) continue; // ⛔ Skip sender

                    try
                    {
                        // 📤 Send the encrypted message
                        var stream = client.GetStream();
                        using var writer = new StreamWriter(stream, System.Text.Encoding.UTF8, 1024, leaveOpen: true)
                        {
                            AutoFlush = true
                        };
                        writer.WriteLine(encryptedMessage);
                    }
                    catch (Exception ex)
                    {
                        // ⚠️ Log broadcast error
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
