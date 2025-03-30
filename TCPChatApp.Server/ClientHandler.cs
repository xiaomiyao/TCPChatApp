using System.Net.Sockets;
using TCPChatApp.Common.Helpers;
using TCPChatApp.Common.Models;

namespace TCPChatApp.Server
{
    public class ClientHandler(TcpClient client, AuthenticationHandler authHandler, ChatMessageHandler chatMessageHandler, ClientCoordinator coordinator)
    {

        // 🔌 Connection objects
        public readonly TcpClient _client = client;

        public User? user { get; private set; }

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
                    string encryptedMessage = reader.ReadLine();
                    if (string.IsNullOrEmpty(encryptedMessage))
                        break;

                    // 🔓 Use MessageProcessor to decrypt and deserialize the incoming message
                    var envelope = MessageProcessor.DecryptAndDeserialize(encryptedMessage);
                    ProcessEnvelope(envelope, writer);
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

        private void ProcessEnvelope(Envelope envelope, StreamWriter writer)
        {
            switch (envelope?.Type)
            {
                case "ChatMessage":
                    chatMessageHandler.HandleChatMessage(this, envelope);
                    break;
                case "Register":
                    authHandler.HandleRegister(envelope, writer);
                    break;
                case "Login":
                    user = authHandler.HandleLogin(envelope, writer);
                    break;
                default:
                    // Optionally handle unknown message types
                    break;
            }
        }

        private void RemoveClient()
        {
            coordinator.RemoveClient(this);
            _client.Close();
        }
    }
}
