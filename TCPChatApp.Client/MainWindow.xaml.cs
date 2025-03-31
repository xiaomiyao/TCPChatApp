using TCPChatApp.Common.Models;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Input;
using TCPChatApp.Common.Helpers;

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

        public User CurrentUser { get; }

        public MainWindow(User user)
        {
            CurrentUser = user;
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

                RegisterConnection();

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
                    // 📬 Attempt to deserialize as Envelope
                    var envelope = MessageProcessor.DecryptAndDeserialize(encryptedMessage);

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

                    // 🔄 Serialize & 🔒 Encrypt
                    string encrypted = MessageProcessor.SerializeAndEncrypt(envelope);
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

        private string GetSelectedUsername(object sender)
        {
            var menuItem = sender as MenuItem;
            if (menuItem?.Parent is ContextMenu contextMenu &&
                contextMenu.PlacementTarget is ListBoxItem listBoxItem)
            {
                return listBoxItem.Content?.ToString();
            }
            return null;
        }

        private void MessageUser_Click(object sender, RoutedEventArgs e)
        {
            string username = GetSelectedUsername(sender);
            if (!string.IsNullOrEmpty(username))
            {
                var msgWindow = new MessageUserWindow(username) { Owner = this };
                if (msgWindow.ShowDialog() == true)
                {
                    // The user clicked Send. Get the entered text.
                    string messageToSend = msgWindow.MessageText;

                    // 📨 Create private message
                    var messageModel = new Message
                    {
                        Sender = "Client",
                        Recipient = username,
                        Content = messageToSend,
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

                    // Update UI: Display the sent message with recipient info
                    Dispatcher.Invoke(() => ChatDisplay.AppendText($"You to {username}: {messageToSend}\n"));
                }
            }
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            string username = GetSelectedUsername(sender);
            if (!string.IsNullOrEmpty(username))
            {
                MessageBox.Show($"Adding {username} as a user");
                // TODO: add code to add the user to a friend list or similar functionality
            }
        }

        private void BlockUser_Click(object sender, RoutedEventArgs e)
        {
            string username = GetSelectedUsername(sender);
            if (!string.IsNullOrEmpty(username))
            {
                MessageBox.Show($"Blocking {username}");
                // TODO: add code to block the user so that messages are ignored
            }
        }

        private void ListBoxItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var listBoxItem = sender as ListBoxItem;
            if (listBoxItem != null)
            {
                listBoxItem.IsSelected = true;
            }
        }
    }
}