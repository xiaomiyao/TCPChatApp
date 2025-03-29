using System;
using System.Text.Json;
using System.Windows;
using TCPChatApp.Client.Helpers; // Ensure that MessageProcessor is referenced
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

        // 👤 Register
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // 📝 Get input
            string username = UsernameInput.Text;
            string password = PasswordInput.Password;

            // 📦 Build envelope
            var envelope = new Envelope
            {
                Type = "Register",
                User = new User { Username = username, PasswordHash = password } // 💡 Hash it later!
            };

            // Serialize and encrypt
            string encryptedMessage = MessageProcessor.SerializeAndEncrypt(envelope);

            // 🔌 Send message
            string encryptedResponse = NetworkHelper.SendMessageToServer("127.0.0.1", 5000, encryptedMessage);

            if (!string.IsNullOrEmpty(encryptedResponse))
            {
                var responseEnvelope = MessageProcessor.DecryptAndDeserialize(encryptedResponse);
                MessageBox.Show(responseEnvelope?.Message?.Content, "Registration", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // 🔑 Login
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // 📝 Get input
            string username = UsernameInput.Text;
            string password = PasswordInput.Password;

            // 📦 Build envelope
            var envelope = new Envelope
            {
                Type = "Login",
                User = new User { Username = username, PasswordHash = password } // 💡 Hash it later!
            };

            // Serialize and encrypt
            string encryptedMessage = MessageProcessor.SerializeAndEncrypt(envelope);

            // 🔌 Send message
            string encryptedResponse = NetworkHelper.SendMessageToServer("127.0.0.1", 5000, encryptedMessage);

            if (!string.IsNullOrEmpty(encryptedResponse))
            {
                var responseEnvelope = MessageProcessor.DecryptAndDeserialize(encryptedResponse);

                // ✅ Success?
                if (responseEnvelope != null && responseEnvelope.Type == "LoginResponse" &&
                    responseEnvelope.Message != null && responseEnvelope.Message.Content.Contains("successful"))
                {
                    // 🚀 Open chat
                    MainWindow chatWindow = new MainWindow();
                    chatWindow.Show();
                    this.Close();
                }
                else
                {
                    // ❗ Fail
                    MessageBox.Show(responseEnvelope?.Message?.Content ?? "Login failed", "Login", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }
}