using System.Text.Json;
using TCPChatApp.Common.Helpers;
using TCPChatApp.Common.Models;

namespace TCPChatApp.Server
{
    public class AuthenticationHandler
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

            var existingUser = ChatServer.UserRepo.GetUserByUsername(envelope.User.Username);
            if (existingUser != null)
            {
                Console.WriteLine("⚠️ Registration failed: username already exists.");
                SendRegistrationResponse(writer, "Registration failed: Username already exists.");
                return;
            }

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

            var existingUser = ChatServer.UserRepo.GetUserByUsername(envelope.User.Username);
            if (existingUser != null && existingUser.PasswordHash.Equals(envelope.User.PasswordHash))
            {
                if (!ChatServer.OnlineUsers.Exists(u => u.Username == envelope.User.Username))
                    ChatServer.OnlineUsers.Add(envelope.User);

                Console.WriteLine($"✅ User '{envelope.User.Username}' login successful.");
                SendLoginResponse(writer, "Login successful.");
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
                Users = ChatServer.OnlineUsers
            };

            string jsonResponse = JsonSerializer.Serialize(responseEnvelope);
            string encryptedResponse = CryptoHelper.Encrypt(jsonResponse);
            writer.WriteLine(encryptedResponse);
        }
    }
}