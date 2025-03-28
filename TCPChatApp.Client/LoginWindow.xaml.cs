using System;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Windows;
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

            try
            {
                // 🔌 Connect & send
                using TcpClient client = new TcpClient("127.0.0.1", 5000);
                using NetworkStream stream = client.GetStream();
                using var writer = new StreamWriter(stream) { AutoFlush = true };
                using var reader = new StreamReader(stream);

                string json = JsonSerializer.Serialize(envelope);
                string encryptedMessage = CryptoHelper.Encrypt(json);
                writer.WriteLine(encryptedMessage);

                // ⏳ Wait & show reply
                string encryptedResponse = reader.ReadLine();
                if (!string.IsNullOrEmpty(encryptedResponse))
                {
                    string responseJson = CryptoHelper.Decrypt(encryptedResponse);
                    var responseEnvelope = JsonSerializer.Deserialize<Envelope>(responseJson);
                    MessageBox.Show(responseEnvelope?.Message?.Content, "Registration", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                // ⚠️ Error
                MessageBox.Show($"Registration error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

            try
            {
                // 🔌 Connect & send
                using TcpClient client = new TcpClient("127.0.0.1", 5000);
                using NetworkStream stream = client.GetStream();
                using var writer = new StreamWriter(stream) { AutoFlush = true };
                using var reader = new StreamReader(stream);

                string json = JsonSerializer.Serialize(envelope);
                string encryptedMessage = CryptoHelper.Encrypt(json);
                writer.WriteLine(encryptedMessage);

                // ⏳ Wait
                string encryptedResponse = reader.ReadLine();
                if (!string.IsNullOrEmpty(encryptedResponse))
                {
                    string responseJson = CryptoHelper.Decrypt(encryptedResponse);
                    var responseEnvelope = JsonSerializer.Deserialize<Envelope>(responseJson);

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
            catch (Exception ex)
            {
                // ⚠️ Error
                MessageBox.Show($"Login error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}