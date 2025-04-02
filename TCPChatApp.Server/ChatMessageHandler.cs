using TCPChatApp.Common.Helpers;
using TCPChatApp.Common.Models;
using TCPChatApp.Server.DataAccess;

namespace TCPChatApp.Server
{
    // makes sense of what the client wants



    public class ChatMessageHandler(ClientCoordinator clientCoordinator)
    {

        // 📨 Process incoming chat messages
        public void HandleChatMessage(ClientHandler sender, Envelope envelope)
        {
            Console.WriteLine($"📨 Received from {envelope.Message.Sender}: {envelope.Message.Content}");
            envelope.Message.Content = $"{envelope.Message.Content}";
            
            // If recipient is specified and not "Everyone", send as a private message
            if (!string.IsNullOrEmpty(envelope.Message.Recipient) && envelope.Message.Recipient != "Everyone")
            {
                clientCoordinator.SendPrivateMessage(envelope);
            }
            else
            {
                BroadcastMessage(sender, envelope);
            }
        }

        // 📢 Send message to all connected clients except sender
        private void BroadcastMessage(ClientHandler sender, Envelope envelope)
        {
            clientCoordinator.BroadcastMessage(envelope, sender);
        }
    }
}