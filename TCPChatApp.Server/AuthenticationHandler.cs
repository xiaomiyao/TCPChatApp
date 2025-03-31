using System.Text.Json;
using TCPChatApp.Common.Helpers;
using TCPChatApp.Common.Models;
using TCPChatApp.Server.DataAccess;

namespace TCPChatApp.Server
{
    public class AuthenticationHandler(UserRepository userRepo, ClientCoordinator coordinator)
    {
        // 🔐 Process new user registration
        public void HandleRegister(ClientHandler client,Envelope envelope)
        {
            Console.WriteLine("🔐 Registration request received.");
            if (envelope.User == null ||
                string.IsNullOrWhiteSpace(envelope.User.Username) ||
                string.IsNullOrWhiteSpace(envelope.User.PasswordHash))
            {
                Console.WriteLine("⚠️ Registration failed: invalid user details.");
                SendRegistrationResponse(client, "Registration failed: Invalid user details.");
                return;
            }

            var existingUser = userRepo.GetUserByUsername(envelope.User.Username);
            if (existingUser != null)
            {
                Console.WriteLine("⚠️ Registration failed: username already exists.");
                SendRegistrationResponse(client, "Registration failed: Username already exists.");
                return;
            }

            bool success = userRepo.AddUser(envelope.User);
            if (success)
            {
                Console.WriteLine($"✅ User '{envelope.User.Username}' registered successfully.");
                SendRegistrationResponse(client, "Registration successful.");
            }
            else
            {
                Console.WriteLine("⚠️ Registration failed: Database error.");
                SendRegistrationResponse(client, "Registration failed: Database error.");
            }
        }

        // 🔑 Process user login attempts
        public void HandleLogin(ClientHandler client, Envelope envelope)
        {
            Console.WriteLine("🔑 Login request received.");
            if (envelope.User == null ||
                string.IsNullOrWhiteSpace(envelope.User.Username) ||
                string.IsNullOrWhiteSpace(envelope.User.PasswordHash))
            {
                Console.WriteLine("⚠️ Login failed: invalid user details.");
                SendLoginResponse(client, "Login failed: Invalid user details.");
                return;
            }

            if (ConfirmUserCredentials(envelope.User))
            {
                Console.WriteLine($"✅ User '{envelope.User.Username}' login successful.");
                SendLoginResponse(client, "Login successful.");
                coordinator.BroadcastOnlineUsers();
                return;
            }
            else
            {
                Console.WriteLine("⚠️ Login failed: Incorrect username or password.");
                SendLoginResponse(client, "Login failed: Incorrect username or password.");
                return;
            }
        }

        public bool ConfirmUserCredentials(User user)
        {
            var existingUser = userRepo.GetUserByUsername(user.Username);
            return (existingUser != null && existingUser.PasswordHash.Equals(user.PasswordHash));
        }

        // 💌 Send registration result to client
        private void SendRegistrationResponse(ClientHandler client, string responseMessage)
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

            client.WriteToClient(responseEnvelope);
        }

        // 💌 Send login result to client
        private void SendLoginResponse(ClientHandler client, string responseMessage)
        {
            var responseEnvelope = new Envelope
            {
                Type = "LoginResponse",
                Message = new Message
                {
                    Sender = "Server",
                    Content = responseMessage
                },
                Users = coordinator.GetOnlineUsers()
            };

            client.WriteToClient(responseEnvelope);
        }
    }
}