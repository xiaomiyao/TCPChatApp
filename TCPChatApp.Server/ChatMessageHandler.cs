using System.Net.Sockets;
using TCPChatApp.Common.Helpers;
using TCPChatApp.Common.Models;

namespace TCPChatApp.Server
{
    public class ChatMessageHandler
    {
        // 🔌 Connection objects: sender client and the clients list
        private readonly TcpClient _senderClient;
        private readonly List<TcpClient> _clients;

        public ChatMessageHandler(TcpClient senderClient, List<TcpClient> clients)
        {
            _senderClient = senderClient;
            _clients = clients;
        }

        // 📨 Handle incoming chat message and broadcast it
        public void HandleChatMessage(Envelope envelope)
        {
            Console.WriteLine($"📨 Received from {envelope.Message.Sender}: {envelope.Message.Content}");
            BroadcastMessage(envelope);
        }

        // 📢 Broadcast chat message to all clients except the sender
        private void BroadcastMessage(Envelope envelope)
        {
            var encryptedMessage = MessageProcessor.SerializeAndEncrypt(envelope);

            lock (_clients)
            {
                foreach (var client in _clients)
                {
                    if (client == _senderClient)
                        continue;
                    try
                    {
                        var stream = client.GetStream();
                        using var writer = new StreamWriter(stream, System.Text.Encoding.UTF8, 1024, leaveOpen: true)
                        { AutoFlush = true };
                        writer.WriteLine(encryptedMessage);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Broadcast error: {ex.Message}");
                    }
                }
            }
        }
    }
}