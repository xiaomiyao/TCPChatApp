// MainWindow.xaml.cs (base class)
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Input;
using TCPChatApp.Common.Helpers;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using TCPChatApp.Common.Models;

namespace TCPChatApp.Client
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<Message> temp;
        // 🌐 Network connection
        private TcpClient _client;
        // ✍️ Send data
        private StreamWriter _writer;
        // 📖 Receive data
        private StreamReader _reader;

        public User CurrentUser { get; }
        public List<Relation> Relations { get; set; } = new List<Relation>();
        public List<User> OnlineUsers { get; set; } = new List<User>();
        private List<MessageUserWindow> _messageWindows = new List<MessageUserWindow>();
        private string _fullChatHistory = string.Empty;

        public MainWindow(User user)
        {
            CurrentUser = user;
            // 🖼️ Initialize UI
            InitializeComponent();
            // 🌐 Connect to server
            ConnectToServer();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Allow dragging the window
            DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
                SystemCommands.MaximizeWindow(this);
            else
                SystemCommands.RestoreWindow(this);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void ChatFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filterText = ChatFilter.Text.ToLower();

            if (string.IsNullOrEmpty(filterText))
            {
                ChatDisplay.Text = _fullChatHistory;
                return;
            }

            // Filter messages and update display
            var filteredLines = _fullChatHistory
                .Split('\n')
                .Where(line => line.ToLower().Contains(filterText))
                .ToList();

            ChatDisplay.Text = string.Join("\n", filteredLines);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (_client != null && _client.Connected)
                {
                    var envelope = new Envelope
                    {
                        Type = "Disconnect",
                        User = CurrentUser
                    };

                    string encrypted = MessageProcessor.SerializeAndEncrypt(envelope);
                    _writer.WriteLine(encrypted);

                    // Clean up resources
                    _writer.Close();
                    _reader.Close();
                    _client.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during disconnect: {ex.Message}");
            }
            finally
            {
                base.OnClosing(e);
            }
        }
    }
}