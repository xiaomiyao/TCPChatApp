using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using TCPChatApp.Common.Models;

namespace TCPChatApp.Client
{
    public partial class MessageUserWindow : Window
    {
        private readonly Action<Message> _messageHandler;
        private readonly string _currentUser;
        public string Recipient { get; private set; }
        public ObservableCollection<Message> ChatMessages { get; }
        private ObservableCollection<Message> _allMessages;

        public MessageUserWindow(string currentUser, string recipient, Action<Message> messageHandler, ObservableCollection<Message> chatMessages)
        {
            InitializeComponent();
            _currentUser = currentUser;
            _messageHandler = messageHandler;
            Recipient = recipient;
            Title = $"Chat with {recipient}";
            ChatMessages = chatMessages;
            _allMessages = new ObservableCollection<Message>();
            ChatDisplayListBox.ItemsSource = ChatMessages;
            UpdateChat();
        }

        public void UpdateChat()
        {
            var messages = ChatFileHandler.LoadMessages(Recipient);
            Application.Current.Dispatcher.Invoke(() =>
            {
                ChatMessages.Clear();
                _allMessages.Clear();
                foreach (var message in messages)
                {
                    ChatMessages.Add(message);
                    _allMessages.Add(message);
                }
                if (ChatMessages.Count > 0)
                {
                    ChatDisplayListBox.ScrollIntoView(ChatMessages[ChatMessages.Count - 1]);
                }
            });
        }

        private void ChatFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filterText = ChatFilter.Text.ToLower();
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                ChatMessages.Clear();
                
                if (string.IsNullOrEmpty(filterText))
                {
                    foreach (var message in _allMessages)
                    {
                        ChatMessages.Add(message);
                    }
                }
                else
                {
                    var filteredMessages = _allMessages.Where(m => 
                        m.Content.ToLower().Contains(filterText) ||
                        m.Sender.ToLower().Contains(filterText)).ToList();

                    foreach (var message in filteredMessages)
                    {
                        ChatMessages.Add(message);
                    }
                }

                if (ChatMessages.Count > 0)
                {
                    ChatDisplayListBox.ScrollIntoView(ChatMessages[ChatMessages.Count - 1]);
                }
            });
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MessageTextBox.Text))
                return;

            var message = new Message
            {
                Sender = _currentUser,
                Recipient = Recipient,
                Content = MessageTextBox.Text.Trim(),
                Timestamp = DateTime.Now
            };

            ChatFileHandler.SaveMessage(message, Recipient);
            
            MessageTextBox.Clear();
            MessageTextBox.Focus();

            _messageHandler?.Invoke(message);
            UpdateChat();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}