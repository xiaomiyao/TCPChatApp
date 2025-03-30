using System.Net.Sockets;
using TCPChatApp.Common.Helpers;
using TCPChatApp.Common.Models;

namespace TCPChatApp.Server
{
    public class ClientHandler
    {
        // 🔌 Connection objects
        private readonly TcpClient _client;
        private readonly List<TcpClient> _clients;
        private readonly AuthenticationHandler _authHandler;

        public ClientHandler(TcpClient client, List<TcpClient> clients, AuthenticationHandler authHandler)
        {
            _client = client;
            _clients = clients;
            _authHandler = authHandler;
        }

        public List<TcpClient> Clients { get; }

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
                    new ChatMessageHandler(_client, _clients).HandleChatMessage(envelope);
                    break;
                case "Register":
                    _authHandler.HandleRegister(envelope, writer);
                    break;
                case "Login":
                    _authHandler.HandleLogin(envelope, writer);
                    break;
                default:
                    // Optionally handle unknown message types
                    break;
            }
        }

        private void RemoveClient()
        {
            lock (_clients)
            {
                _clients.Remove(_client);
            }
            _client.Close();
        }
    }
}
