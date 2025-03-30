using System.Net.Sockets;
using System.Text.Json;
using TCPChatApp.Common.Helpers;
using TCPChatApp.Common.Models;
using TCPChatApp.Server.DataAccess;

namespace TCPChatApp.Server
{
    public class AuthenticationHandler(ChatServer server, UserRepository userRepo)
    {
        // 🔐 Handle registration logic
        public void HandleRegister(Envelope envelope, StreamWriter writer)
        {
            Console.WriteLine("🔐 Registration request received.");
            if (envelope.User == null ||
                string.IsNullOrWhiteSpace(envelope.User.Username) ||
                string.IsNullOrWhiteSpace(envelope.User.PasswordHash))
            {
                Console.WriteLine("⚠️ Registration failed: invalid user details.");
                SendRegistrationResponse(writer, "Registration failed: Invalid user details.");
                return;
            }

            var existingUser = userRepo.GetUserByUsername(envelope.User.Username);
            if (existingUser != null)
            {
                Console.WriteLine("⚠️ Registration failed: username already exists.");
                SendRegistrationResponse(writer, "Registration failed: Username already exists.");
                return;
            }

            bool success = userRepo.AddUser(envelope.User);
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

        // 🔑 Handle login logic
        public void HandleLogin(Envelope envelope, StreamWriter writer)
        {
            Console.WriteLine("🔑 Login request received.");
            if (envelope.User == null ||
                string.IsNullOrWhiteSpace(envelope.User.Username) ||
                string.IsNullOrWhiteSpace(envelope.User.PasswordHash))
            {
                Console.WriteLine("⚠️ Login failed: invalid user details.");
                SendLoginResponse(writer, "Login failed: Invalid user details.");
                return;
            }

            var existingUser = userRepo.GetUserByUsername(envelope.User.Username);
            if (existingUser != null && existingUser.PasswordHash.Equals(envelope.User.PasswordHash))
            {
                if (!server.OnlineUsers.Exists(u => u.Username == envelope.User.Username))
                    server.OnlineUsers.Add(envelope.User);

                Console.WriteLine($"✅ User '{envelope.User.Username}' login successful.");
                SendLoginResponse(writer, "Login successful.");
                server.BroadcastOnlineUsers();
            }
            else
            {
                Console.WriteLine("⚠️ Login failed: Incorrect username or password.");
                SendLoginResponse(writer, "Login failed: Incorrect username or password.");
            }
        }

        // 💌 Send registration response back to the client
        private void SendRegistrationResponse(StreamWriter writer, string responseMessage)
        {
            var responseEnvelope = new Envelope
            {
                Type = "RegistrationResponse",
                Message = new Message
                {
                    Sender = "Server",
                    Content = responseMessage
                }
            };

            string jsonResponse = JsonSerializer.Serialize(responseEnvelope);
            string encryptedResponse = CryptoHelper.Encrypt(jsonResponse);
            writer.WriteLine(encryptedResponse);
        }

        // 💌 Send login response back to the client
        private void SendLoginResponse(StreamWriter writer, string responseMessage)
        {
            var responseEnvelope = new Envelope
            {
                Type = "LoginResponse",
                Message = new Message
                {
                    Sender = "Server",
                    Content = responseMessage
                },
                Users = server.OnlineUsers
            };

            string jsonResponse = JsonSerializer.Serialize(responseEnvelope);
            string encryptedResponse = CryptoHelper.Encrypt(jsonResponse);
            writer.WriteLine(encryptedResponse);        
        }
    }
}