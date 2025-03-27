using TCPChatApp.Common.Models;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Windows;

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
                    // 🔒 Read encrypted
                    string encryptedMessage = _reader.ReadLine();
                    if (string.IsNullOrEmpty(encryptedMessage)) continue;

                    // 🔓 Decrypt
                    string message = TCPChatApp.Common.Helpers.CryptoHelper.Decrypt(encryptedMessage);
                    // 🖥️ Update UI
                    Dispatcher.Invoke(() => ChatDisplay.AppendText($"{message}\n"));
                }
            }
            catch (Exception ex)
            {
                // ❌ Receive error
                Dispatcher.Invoke(() => MessageBox.Show($"Error receiving message: {ex.Message}"));
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 📝 Get input
                string message = MessageInput.Text;
                if (!string.IsNullOrEmpty(message))
                {
                    // 👤 Sender
                    // 👥 Recipient
                    // ✉️ Content
                    // ⏰ Timestamp
                    var messageModel = new Message
                    {
                        Sender = "Client",
                        Recipient = "Everyone",
                        Content = message,
                        Timestamp = DateTime.Now
                    };

                    // 🔄 Serialize
                    string json = JsonSerializer.Serialize(messageModel);
                    // 🔒 Encrypt
                    string encrypted = TCPChatApp.Common.Helpers.CryptoHelper.Encrypt(json);
                    // ✍️ Send
                    _writer.WriteLine(encrypted);

                    // 🧹 Clear input
                    MessageInput.Clear();
                    // 🖥️ Update UI
                    Dispatcher.Invoke(() => ChatDisplay.AppendText($"{message}\n"));
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