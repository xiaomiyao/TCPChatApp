﻿using System.Net.Sockets;
using TCPChatApp.Common.Helpers;
using TCPChatApp.Common.Models;
using TCPChatApp.Server.DataAccess;

namespace TCPChatApp.Server
{
    public class ClientHandler(TcpClient client, AuthenticationHandler authHandler, ChatMessageHandler chatMessageHandler, ClientCoordinator coordinator, RelationRepository relationRepository)
    {
        // 🔌 Connection objects
        private readonly TcpClient _client = client;
        private StreamWriter writer;

        public User? user { get; private set; }

        public void HandleClient()
        {
            try
            {
                // 🔄 Setup streams
                using var networkStream = _client.GetStream();
                using var reader = new StreamReader(networkStream);
                writer = new StreamWriter(networkStream) { AutoFlush = true };

                // 👂 Message loop
                while (true)
                {
                    string encryptedMessage = reader.ReadLine();
                    if (string.IsNullOrEmpty(encryptedMessage))
                        break;

                    // 🔓 Use MessageProcessor to decrypt and deserialize the incoming message
                    var envelope = MessageProcessor.DecryptAndDeserialize(encryptedMessage);
                    ProcessEnvelope(envelope);
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



        public void WriteToClient(Envelope envelope)
        {
            var encryptedMessage = MessageProcessor.SerializeAndEncrypt(envelope);
            writer.WriteLine(encryptedMessage);
        }

        private void ProcessEnvelope(Envelope envelope)
        {
            System.Console.WriteLine($"Received message: {envelope?.Type} from {envelope?.User?.Username}");
            switch (envelope?.Type)
            {
                case "ChatMessage":
                    chatMessageHandler.HandleChatMessage(this, envelope);
                    break;
                case "Register":
                    authHandler.HandleRegister(this, envelope);
                    break;
                case "Login":
                    authHandler.HandleLogin(this, envelope);
                    break;
                case "Authenticate":
                    if (authHandler.ConfirmUserCredentials(envelope.User))
                    {
                        user = envelope.User;
                        coordinator.BroadcastOnlineUsers();
                    }
                    break;
                case "AddUser":
                    if (envelope.User != null && envelope.Message != null)
                    {
                        var relation = new Relation
                        {
                            UserName = envelope.User.Username,
                            TargetName = envelope.Message.Recipient,
                            IsFriend = true,
                            IsBlocked = false
                        };
                        bool success = relationRepository.AddRelation(relation);
                        Console.WriteLine(success ? $"✅ Added friend relation: {relation.TargetName}" : $"⚠️ Failed to add friend relation: {relation.TargetName}");
                    }
                    break;
                case "BlockUser":
                    if (envelope.User != null && envelope.Message != null)
                    {
                        // Prevent users from blocking themselves
                        if (envelope.User.Username.Equals(envelope.Message.Recipient, System.StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine("⚠️ Cannot block yourself.");
                            break;
                        }
                        var relation = new Relation
                        {
                            UserName = envelope.User.Username,
                            TargetName = envelope.Message.Recipient,
                            IsFriend = false,
                            IsBlocked = true
                        };
                        bool success = relationRepository.AddRelation(relation);
                        Console.WriteLine(success ? $"✅ Blocked user: {relation.TargetName}" : $"⚠️ Failed to block user: {relation.TargetName}");
                    }
                    break;
                default:
                    // Optionally handle unknown message types
                    break;
            }
        }

        private void RemoveClient()
        {
            writer.Close();
            coordinator.RemoveClient(this);
            _client.Close();
        }
    }
}
