// MainWindow.UserActions.xaml.cs (User action functionality)
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TCPChatApp.Common.Helpers;
using TCPChatApp.Common.Models;

namespace TCPChatApp.Client
{
    public partial class MainWindow
    {
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
                OpenPrivateMessageWindow(username);
            }
        }

        private void OpenPrivateMessageWindow(string username)
        {
            temp = new ObservableCollection<Message>();
            var msgWindow = new MessageUserWindow(CurrentUser.Username, username, HandlePrivateMessage, temp)
            {
                Owner = this
            };
            _messageWindows.Add(msgWindow);
            msgWindow.Show();
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            string username = GetSelectedUsername(sender);
            if (!string.IsNullOrEmpty(username))
            {
                SendUserActionRequest("AddUser", username);
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

                SendUserActionRequest("BlockUser", username);
            }
        }

        private void UnblockUser_Click(object sender, RoutedEventArgs e)
        {
            string username = GetSelectedUsername(sender);
            if (string.IsNullOrEmpty(username))
            {
                return;
            }

            MessageBox.Show($"Unblocking {username}");
            SendUserActionRequest("UnblockUser", username);
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            string username = GetSelectedUsername(sender);
            if (string.IsNullOrEmpty(username))
            {
                return;
            }

            MessageBox.Show($"Removing {username} from your friend list");
            SendUserActionRequest("DeleteUser", username);
        }

        private void SendUserActionRequest(string actionType, string targetUsername)
        {
            var envelope = new Envelope
            {
                Type = actionType,
                Message = new Message
                {
                    Sender = CurrentUser.Username,
                    Recipient = targetUsername,
                    Content = $"Request to {actionType} {targetUsername}",
                    Timestamp = DateTime.Now
                },
                User = CurrentUser
            };

            string encrypted = MessageProcessor.SerializeAndEncrypt(envelope);
            _writer.WriteLine(encrypted);
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