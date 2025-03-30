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
                                // Use the repository to check for an existing user
                                var existingUser = ChatServer.UserRepo.GetUserByUsername(envelope.User.Username);

                                if (existingUser != null)
                                {
                                    Console.WriteLine("⚠️ Registration failed: username already exists.");
                                    SendRegistrationResponse(writer, "Registration failed: Username already exists.");
                                }
                                else
                                {
                                    // Add the user to the database
                                    bool success = ChatServer.UserRepo.AddUser(envelope.User);
                                    if (success)
                                    {
                                        Console.WriteLine($"✅ User '{envelope.User.Username}' registered successfully.");
                                        SendRegistrationResponse(writer, "Registration successful.");
                                    }
                                    else
                                    {
                                        Console.WriteLine("⚠️ Registration failed: Database error.");
                                        SendRegistrationResponse(writer, "Registration failed: Database error.");
                                    }
                                }
                            }
                        }
                        else if (envelope.Type == "Login")
                        {
                            Console.WriteLine("🔑 Login request received.");

                            // Validate login details
                            if (envelope.User == null ||
                                string.IsNullOrWhiteSpace(envelope.User.Username) ||
                                string.IsNullOrWhiteSpace(envelope.User.PasswordHash))
                            {
                                Console.WriteLine("⚠️ Login failed: invalid user details.");
                                SendLoginResponse(writer, "Login failed: Invalid user details.");
                            }
                            else
                            {
                                // Use the repository to retrieve user info
                                var existingUser = ChatServer.UserRepo.GetUserByUsername(envelope.User.Username);

                                if (existingUser != null && existingUser.PasswordHash.Equals(envelope.User.PasswordHash))
                                {                                    // Add the user to the online users list (if not already added)
                                    if (!ChatServer.OnlineUsers.Exists(u => u.Username == envelope.User.Username))
                                    {
                                        ChatServer.OnlineUsers.Add(envelope.User);
                                    }
                                    Console.WriteLine($"✅ User '{envelope.User.Username}' login successful.");
                                    SendLoginResponse(writer, "Login successful.");
                                }
                                else
                                {
                                    Console.WriteLine("⚠️ Login failed: Incorrect username or password.");
                                    SendLoginResponse(writer, "Login failed: Incorrect username or password.");
                                }
                            }
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

        /// <summary>
        /// Sends a login response back to the client.
        /// </summary>
        private void SendLoginResponse(StreamWriter writer, string responseMessage)
        {
            // 📦 Build response envelope for login
            var responseEnvelope = new Envelope
            {
                Type = "LoginResponse",
                Message = new Message
                {
                    Sender = "Server",
                    Content = responseMessage
                },
                Users = ChatServer.OnlineUsers // 🧑‍🤝‍🧑 Online users list
            };

            // 🔒 Encrypt and send the login response
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
