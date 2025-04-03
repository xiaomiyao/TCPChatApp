using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;
using TCPChatApp.Client.Helpers;
using TCPChatApp.Common.Helpers;
using TCPChatApp.Common.Models;

namespace TCPChatApp.Client
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent(); // 🔧 Init UI
        }

        // Allow window dragging when clicking the title bar
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        // Close button
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Minimize button
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        // Maximize/Restore button
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
                SystemCommands.MaximizeWindow(this);
            else
                SystemCommands.RestoreWindow(this);
        }

        // 👤 Register
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameInput.Text;
            string password = PasswordInput.Password;

            var envelope = new Envelope
            {
                Type = "Register",
                User = new User { Username = username, PasswordHash = password }
            };

            string encryptedMessage = MessageProcessor.SerializeAndEncrypt(envelope);
            string encryptedResponse = NetworkHelper.SendMessageToServer("127.0.0.1", 5000, encryptedMessage);

            if (!string.IsNullOrEmpty(encryptedResponse))
            {
                var responseEnvelope = MessageProcessor.DecryptAndDeserialize(encryptedResponse);
                MessageBox.Show(responseEnvelope?.Message?.Content, "Registration", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            MessageBox.Show("Register clicked!");
        }

        // 🔑 Login
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameInput.Text;
            string password = PasswordInput.Password;

            var envelope = new Envelope
            {
                Type = "Login",
                User = new User { Username = username, PasswordHash = password }
            };

            string encryptedMessage = MessageProcessor.SerializeAndEncrypt(envelope);
            string encryptedResponse = NetworkHelper.SendMessageToServer("127.0.0.1", 5000, encryptedMessage);

            if (!string.IsNullOrEmpty(encryptedResponse))
            {
                var responseEnvelope = MessageProcessor.DecryptAndDeserialize(encryptedResponse);

                if (responseEnvelope != null && responseEnvelope.Type == "LoginResponse" &&
                    responseEnvelope.Message != null && responseEnvelope.Message.Content.Contains("successful"))
                {
                    MainWindow chatWindow = new MainWindow(envelope.User);
                    chatWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show(responseEnvelope?.Message?.Content ?? "Login failed", "Login", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }
}