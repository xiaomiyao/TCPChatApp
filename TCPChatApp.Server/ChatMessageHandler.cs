using TCPChatApp.Common.Helpers;
using TCPChatApp.Common.Models;

namespace TCPChatApp.Server
{
    public class ChatMessageHandler(ClientCoordinator clientCoordinator)
    {
        // 📨 Process incoming chat messages
        public void HandleChatMessage(ClientHandler sender, Envelope envelope)
        {
            Console.WriteLine($"📨 Received from {envelope.Message.Sender}: {envelope.Message.Content}");
            BroadcastMessage(sender, envelope);
        }

        // 📢 Send message to all connected clients except sender
        private void BroadcastMessage(ClientHandler sender, Envelope envelope)
        {
            var encryptedMessage = MessageProcessor.SerializeAndEncrypt(envelope);
            clientCoordinator.BroadcastMessage(encryptedMessage, sender);
        }
    }
}