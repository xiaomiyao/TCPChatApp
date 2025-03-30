using TCPChatApp.Common.Models;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Windows;
using System.Threading;
using System.Collections.Generic;

namespace TCPChatApp.Client
{
    public partial class MainWindow : Window
    {
        // 🌐 Network connection
        private TcpClient _client;
        // ✍️ Send data
        private StreamWriter _writer;
        // 📖 Receive data
        private StreamReader _reader;

        public MainWindow()
        {
            // 🖼️ Initialize UI
            InitializeComponent();
            // 🌐 Connect to server
            ConnectToServer();

        }

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
            }
            catch (Exception ex)
            {
                // ❌ Connection error
                MessageBox.Show($"Error connecting to server: {ex.Message}");
            }
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
                    string plainText = TCPChatApp.Common.Helpers.CryptoHelper.Decrypt(encryptedMessage);
                    // 📬 Attempt to deserialize as Envelope
                    var envelope = JsonSerializer.Deserialize<Envelope>(plainText);

                    if (envelope != null && envelope.Type == "ChatMessage")
                    {
                        // 🖥️ Display message with sender and content
                        string displayText = $"{envelope.Message.Sender}: {envelope.Message.Content}";
                        Dispatcher.Invoke(() => ChatDisplay.AppendText($"{displayText}\n"));
                    }
                    else if (envelope != null && envelope.Type == "OnlineUsers")
                    {
                        UpdateOnlineUsersList(envelope.Users);
                    }
                    else
                    {
                        // If envelope is null or not recognized, display the plain text
                        Dispatcher.Invoke(() => ChatDisplay.AppendText($"{plainText}\n"));
                    }
                }
            }
            catch (Exception ex)
            {
                // ❌ Receive error
                Dispatcher.Invoke(() => MessageBox.Show($"Error receiving message: {ex.Message}"));
            }
        }

        public void UpdateOnlineUsersList(List<User> users)
        {
            Dispatcher.Invoke(() =>
            {
                OnlineUsersList.Items.Clear();
                foreach (var user in users)
                {
                    OnlineUsersList.Items.Add(user.Username);
                }
            });
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 📝 Get input
                string message = MessageInput.Text;
                if (!string.IsNullOrEmpty(message))
                {
                    // 📨 Create message
                    var messageModel = new Message
                    {
                        Sender = "Client",
                        Recipient = "Everyone",
                        Content = message,
                        Timestamp = DateTime.Now
                    };

                    // 📬 Create envelope
                    var envelope = new Envelope
                    {
                        Type = "ChatMessage",
                        Message = messageModel,
                        User = null // Optional: Add user info if needed
                    };

                    // 🔄 Serialize
                    string json = JsonSerializer.Serialize(envelope);
                    // 🔒 Encrypt
                    string encrypted = TCPChatApp.Common.Helpers.CryptoHelper.Encrypt(json);
                    // ✍️ Send
                    _writer.WriteLine(encrypted);

                    // 🧹 Clear input
                    MessageInput.Clear();
                    // 🖥️ Update UI
                    Dispatcher.Invoke(() => ChatDisplay.AppendText($"You: {message}\n"));
                }
            }
            catch (Exception ex)
            {
                // ❌ Send error
                MessageBox.Show($"Error sending message: {ex.Message}");
            }
        }
    }
}