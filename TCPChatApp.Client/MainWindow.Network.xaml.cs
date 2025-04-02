// MainWindow.Network.xaml.cs (Network functionality)
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using TCPChatApp.Common.Helpers;
using TCPChatApp.Common.Models;

namespace TCPChatApp.Client
{
    public partial class MainWindow
    {
        private void ConnectToServer()
        {
            try
            {
                // 🌐 Connect
                _client = new TcpClient("127.0.0.1", 5000);
                var networkStream = _client.GetStream();
                // ✍️ Auto send
                _writer = new StreamWriter(networkStream) { AutoFlush = true };
                _reader = new StreamReader(networkStream);
                // 🔄 Listen for messages
                Thread listenThread = new Thread(ListenForMessages);
                listenThread.Start();

                RegisterConnection();
            }
            catch (Exception ex)
            {
                // ❌ Connection error
                MessageBox.Show($"Error connecting to server: {ex.Message}");
            }
        }

        private void RegisterConnection()
        {
            // 📬 Create envelope
            var envelope = new Envelope
            {
                Type = "Authenticate",
                User = CurrentUser
            };
            // 🔄 Serialize & 🔒 Encrypt
            string encrypted = MessageProcessor.SerializeAndEncrypt(envelope);
            // ✍️ Send
            _writer.WriteLine(encrypted);
        }

        private void ListenForMessages()
        {
            try
            {
                while (true)
                {
                    // 🔒 Read encrypted message
                    string encryptedMessage = _reader.ReadLine();
                    if (string.IsNullOrEmpty(encryptedMessage)) continue;

                    // 🔓 Decrypt message
                    // 📬 Attempt to deserialize as Envelope
                    var envelope = MessageProcessor.DecryptAndDeserialize(encryptedMessage);

                    if (envelope != null && envelope.Type == "ChatMessage")
                    {
                        HandleChatMessage(envelope);
                    }
                    else if (envelope != null && envelope.Type == "OnlineUsers")
                    {
                        UpdateOnlineUsersList(envelope.Users);
                    }
                    else if (envelope != null && envelope.Type == "RelationsList")
                    {
                        // Expecting a new property "Relations" in Envelope (List<Relation>)
                        UpdateRelationsDisplay(envelope.Relations);
                    }
                    else
                    {
                        // If envelope is null or not recognized, display the plain text
                        Dispatcher.Invoke(() => ChatDisplay.AppendText($"error, received: {envelope}\n"));
                    }
                }
            }
            catch (Exception ex)
            {
                // ❌ Receive error
                Dispatcher.Invoke(() => MessageBox.Show($"Error receiving message: {ex.Message}"));
            }
        }

        private void HandleChatMessage(Envelope envelope)
        {
            // 🖥️ Display message with sender and content
            if (Relations.Any(r => r.TargetName.Equals(envelope.Message.Sender, System.StringComparison.OrdinalIgnoreCase)
                && r.IsBlocked))
            {
                return;
            }

            string displayText = $"{envelope.Message.Content}";
            if (envelope.Message.Recipient == CurrentUser.Username)
            {
                ChatFileHandler.SaveMessage(envelope.Message, envelope.Message.Sender);
                _messageWindows.FirstOrDefault(
                    w => w.Recipient == envelope.Message.Sender
                )?.UpdateChat();
            }

            Dispatcher.Invoke(() =>
            {
                string displayText = $"{envelope.Message.Sender} to {envelope.Message.Recipient}: {envelope.Message.Content}\n";
                _fullChatHistory += displayText;
                Dispatcher.Invoke(() => ChatDisplay.AppendText(displayText));
            });

        }
    }
}