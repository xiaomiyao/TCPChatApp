using TCPChatApp.Common.Models;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Input;
using TCPChatApp.Common.Helpers;
using System.Windows.Media;

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
        public List<Relation> Relations { get; set; } = new List<Relation>();
        public List<User> OnlineUsers { get; set; } = new List<User>();

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
                        if (Relations.Any(r => r.TargetName.Equals(envelope.Message.Sender, System.StringComparison.OrdinalIgnoreCase)
                            && r.IsBlocked))
                        {
                            continue;
                        }
                        string displayText = $"{envelope.Message.Content}";
                        Dispatcher.Invoke(() => ChatDisplay.AppendText($"{displayText}\n"));
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

        public void UpdateOnlineUsersList(List<User> users)
        {
            OnlineUsers = users;
            RenderOnlineUsersList();
        }

        private void RenderOnlineUsersList()
        {
            Dispatcher.Invoke(() =>
            {
                OnlineUsersList.Items.Clear();
                // 🖥️ Display online users, users in Relations where isBlocked==True must have red font, where isFriend==True must have the green font
                foreach (var user in OnlineUsers)
                {
                    if (user.Username.Equals(CurrentUser.Username, System.StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var listBoxItem = new ListBoxItem
                    {
                        Content = user.Username,
                        ContextMenu = new ContextMenu()
                    };

                    // Add context menu items 

                    var messageUserMenuItem = new MenuItem { Header = "Message User" };
                    messageUserMenuItem.Click += MessageUser_Click;
                    listBoxItem.ContextMenu.Items.Add(messageUserMenuItem);

                    //add user unless already a friend 
                    if (!Relations.Any(r => r.TargetName == user.Username && r.IsFriend))
                    {
                        var addUserMenuItem = new MenuItem { Header = "Add User" };
                        addUserMenuItem.Click += AddUser_Click;
                        listBoxItem.ContextMenu.Items.Add(addUserMenuItem);
                    }
                    else
                    {
                        var removeUserMenuItem = new MenuItem { Header = "Remove User" };
                        removeUserMenuItem.Click += DeleteUser_Click;
                        listBoxItem.ContextMenu.Items.Add(removeUserMenuItem);
                    }

                    // block user unless already blocked
                    if (!Relations.Any(r => r.TargetName == user.Username && r.IsBlocked))
                    {
                        var blockUserMenuItem = new MenuItem { Header = "Block User" };
                        blockUserMenuItem.Click += BlockUser_Click;
                        listBoxItem.ContextMenu.Items.Add(blockUserMenuItem);
                    }
                    else
                    {
                        var unblockUserMenuItem = new MenuItem { Header = "Unblock User" };
                        unblockUserMenuItem.Click += UnblockUser_Click;
                        listBoxItem.ContextMenu.Items.Add(unblockUserMenuItem);
                    }

                    // Check if the user is blocked or a friend
                    var relation = Relations.FirstOrDefault(r => r.UserName == CurrentUser.Username && r.TargetName == user.Username);
                    if (relation != null)
                    {
                        if (relation.IsBlocked)
                        {
                            listBoxItem.Foreground = Brushes.Red; // Blocked users in red
                        }
                        else if (relation.IsFriend)
                        {
                            listBoxItem.Foreground = Brushes.Green; // Friends in green
                        }
                    }

                    OnlineUsersList.Items.Add(listBoxItem);
                }

            });
        }

        private void UnblockUser_Click(object sender, RoutedEventArgs e)
        {
            string username = GetSelectedUsername(sender);
            if (string.IsNullOrEmpty(username))
            {
                return;
            }

            MessageBox.Show($"Unblocking {username}");
            var envelope = new Envelope
            {
                Type = "UnblockUser",
                Message = new Message
                {
                    Sender = CurrentUser.Username,
                    Recipient = username,
                    Content = $"Request to unblock {username}",
                    Timestamp = DateTime.Now
                },
                User = CurrentUser
            };

            string encrypted = MessageProcessor.SerializeAndEncrypt(envelope);
            _writer.WriteLine(encrypted);
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            string username = GetSelectedUsername(sender);
            if (string.IsNullOrEmpty(username))
            {
                return;
            }

            MessageBox.Show($"Removing {username} from your friend list");
            var envelope = new Envelope
            {
                Type = "DeleteUser",
                Message = new Message
                {
                    Sender = CurrentUser.Username,
                    Recipient = username,
                    Content = $"Request to remove {username} from friend list",
                    Timestamp = DateTime.Now
                },
                User = CurrentUser
            };

            string encrypted = MessageProcessor.SerializeAndEncrypt(envelope);
            _writer.WriteLine(encrypted);
        }

        private void UpdateRelationsDisplay(List<Relation> relations)
        {
            Relations = relations;
            RenderOnlineUsersList();
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
                        Sender = CurrentUser.Username,
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
                        Sender = CurrentUser.Username,
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
                var envelope = new Envelope
                {
                    Type = "AddUser",
                    Message = new Message
                    {
                        Sender = CurrentUser.Username,
                        Recipient = username,
                        Content = $"Request to add {username}",
                    },
                    User = CurrentUser
                };

                string encrypted = MessageProcessor.SerializeAndEncrypt(envelope);
                _writer.WriteLine(encrypted);
            }
        }

        private void BlockUser_Click(object sender, RoutedEventArgs e)
        {
            string username = GetSelectedUsername(sender);
            if (!string.IsNullOrEmpty(username))
            {
                // Prevent blocking self in the client
                if (username.Equals(CurrentUser.Username, System.StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("You cannot block yourself.", "Block User", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                MessageBox.Show($"Blocking {username}");
                var envelope = new Envelope
                {
                    Type = "BlockUser",
                    Message = new Message
                    {
                        Sender = CurrentUser.Username,
                        Recipient = username,
                        Content = $"Request to block {username}",
                        Timestamp = DateTime.Now
                    },
                    User = CurrentUser
                };

                string encrypted = MessageProcessor.SerializeAndEncrypt(envelope);
                _writer.WriteLine(encrypted);
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