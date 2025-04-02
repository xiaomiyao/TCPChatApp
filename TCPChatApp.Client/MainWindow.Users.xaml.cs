// MainWindow.Users.xaml.cs (User management)
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TCPChatApp.Common.Models;

namespace TCPChatApp.Client
{
    public partial class MainWindow
    {
        public void UpdateOnlineUsersList(List<User> users)
        {
            NotifyOfFriendChanges(users);
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

                    AddContextMenuOptions(listBoxItem, user.Username);
                    SetUserColor(listBoxItem, user.Username);

                    OnlineUsersList.Items.Add(listBoxItem);
                }
            });
        }

        private void AddContextMenuOptions(ListBoxItem listBoxItem, string username)
        {
            // Add message option
            var messageUserMenuItem = new MenuItem { Header = "Message User" };
            messageUserMenuItem.Click += MessageUser_Click;
            listBoxItem.ContextMenu.Items.Add(messageUserMenuItem);

            // Add/remove friend option
            if (!Relations.Any(r => r.TargetName == username && r.IsFriend))
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

            // Block/unblock option
            if (!Relations.Any(r => r.TargetName == username && r.IsBlocked))
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
        }

        private void SetUserColor(ListBoxItem listBoxItem, string username)
        {
            // Check if the user is blocked or a friend
            var relation = Relations.FirstOrDefault(r => r.UserName == CurrentUser.Username && r.TargetName == username);
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
        }

        private void UpdateRelationsDisplay(List<Relation> relations)
        {
            Relations = relations;
            RenderOnlineUsersList();
            RenderFriendsList();
        }

        private void NotifyOfFriendChanges(List<User> newOnlineUsers)
        {
            //compare newRelations with old relations, create 2 lists, "added" and "removed"
            var added = newOnlineUsers.Where(r => !OnlineUsers.Any(o => o.Username == r.Username)).ToList();
            var removed = OnlineUsers.Where(r => !newOnlineUsers.Any(o => o.Username == r.Username)).ToList();

            // Notify user of changes
            foreach (var user in added)
                if (Relations.Any(r => r.TargetName == user.Username && r.IsFriend))
                {
                    var message = $"Friend Online: {user.Username} \n";
                    _fullChatHistory += message;
                    Dispatcher.Invoke(() => ChatDisplay.AppendText(message));
                }

            foreach (var user in removed)
                if (Relations.Any(r => r.TargetName == user.Username && r.IsFriend))
                {
                    var message = $"Friend Offline: {user.Username} \n";
                    _fullChatHistory += message;
                    Dispatcher.Invoke(() => ChatDisplay.AppendText(message));
                }
        }

        private void RenderFriendsList()
        {
            //loop through relations and add friends to the list
            Dispatcher.Invoke(() =>
            {
                FriendsList.Items.Clear();

                foreach (var relation in Relations)
                {
                    if (relation.IsFriend)
                    {
                        var listBoxItem = new ListBoxItem
                        {
                            Content = relation.TargetName,
                            ContextMenu = new ContextMenu()
                        };
                        listBoxItem.Foreground = Brushes.Green;

                        // Add context menu items 
                        var messageUserMenuItem = new MenuItem { Header = "Message User" };
                        messageUserMenuItem.Click += MessageUser_Click;
                        listBoxItem.ContextMenu.Items.Add(messageUserMenuItem);

                        var removeUserMenuItem = new MenuItem { Header = "Remove User" };
                        removeUserMenuItem.Click += DeleteUser_Click;
                        listBoxItem.ContextMenu.Items.Add(removeUserMenuItem);



                        FriendsList.Items.Add(listBoxItem);
                    }
                }
            });
        }

        private void FriendsFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filterText = FriendsFilter.Text.ToLower();
            //if filtertext is empty, show all friends
            if (string.IsNullOrEmpty(filterText))
            {
                RenderFriendsList();
                return;
            }

            FriendsList.Items.Clear();
            foreach (var relation in Relations.Where(r => r.IsFriend))
            {
                if (relation.TargetName.ToLower().Contains(filterText))
                {
                    var listBoxItem = new ListBoxItem
                    {
                        Content = relation.TargetName,
                        Foreground = Brushes.Green
                    };
                    FriendsList.Items.Add(listBoxItem);
                }
            }
        }

    }
}