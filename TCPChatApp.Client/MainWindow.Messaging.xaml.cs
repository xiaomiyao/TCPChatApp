// MainWindow.Messaging.xaml.cs (Messaging functionality)
using System;
using System.Windows;
using TCPChatApp.Common.Helpers;
using TCPChatApp.Common.Models;

namespace TCPChatApp.Client
{
    public partial class MainWindow
    {
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 📝 Get input
                string message = MessageInput.Text;
                if (!string.IsNullOrEmpty(message))
                {
                    SendPublicMessage(message);
                }
            }
            catch (Exception ex)
            {
                // ❌ Send error
                MessageBox.Show($"Error sending message: {ex.Message}");
            }
        }

        private void SendPublicMessage(string content)
        {
            // 📨 Create message
            var messageModel = new Message
            {
                Sender = CurrentUser.Username,
                Recipient = "Everyone",
                Content = content,
                Timestamp = DateTime.Now
            };

            // 📬 Create envelope
            var envelope = new Envelope
            {
                Type = "ChatMessage",
                Message = messageModel,
                User = null // Optional: Add user info if needed
            };

            // 🔄 Serialize & 🔒 Encrypt
            string encrypted = MessageProcessor.SerializeAndEncrypt(envelope);
            // ✍️ Send
            _writer.WriteLine(encrypted);

            // 🧹 Clear input
            MessageInput.Clear();
            // 🖥️ Update UI
            Dispatcher.Invoke(() => ChatDisplay.AppendText($"You: {content}\n"));
        }

        private void HandlePrivateMessage(Message message)
        {
            // Create envelope
            var envelope = new Envelope
            {
                Type = "ChatMessage",
                Message = message,
                User = null
            };

            // Serialize & Encrypt
            string encrypted = MessageProcessor.SerializeAndEncrypt(envelope);
            // Send
            _writer.WriteLine(encrypted);

            // Update UI: Display the sent message with recipient info
            Dispatcher.Invoke(() => 
            {
                ChatDisplay.AppendText($"You to {message.Recipient}: {message.Content}\n");
                _fullChatHistory += $"You to {message.Recipient}: {message.Content}\n";
            });
        }
    }
}