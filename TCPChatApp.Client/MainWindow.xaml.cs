using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TCPChatApp.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient _client;
        private StreamWriter _writer;
        private StreamReader _reader;

        public MainWindow()
        {
            InitializeComponent();
            ConnectToServer();
        }

        private void ConnectToServer()
        {
            try
            {
                _client = new TcpClient("127.0.0.1", 5000);
                var networkStream = _client.GetStream();
                _writer = new StreamWriter(networkStream) { AutoFlush = true };
                _reader = new StreamReader(networkStream);

                // Start listening for messages from the server
                Thread listenThread = new Thread(ListenForMessages);
                listenThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to server: {ex.Message}");
            }
        }

        private void ListenForMessages()
        {
            try
            {
                while (true)
                {
                    string encryptedMessage = _reader.ReadLine();
                    if (string.IsNullOrEmpty(encryptedMessage))
                        continue;

                    string message = TCPChatApp.Common.Helpers.EncryptionHelper.Decrypt(encryptedMessage);
                    Dispatcher.Invoke(() => ChatDisplay.AppendText($"{message}\n"));
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => MessageBox.Show($"Error receiving message: {ex.Message}"));
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string message = MessageInput.Text;
                if (!string.IsNullOrEmpty(message))
                {
                    string encrypted = TCPChatApp.Common.Helpers.EncryptionHelper.Encrypt(message);
                    _writer.WriteLine(encrypted);
                    MessageInput.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending message: {ex.Message}");
            }
        }
    }
}