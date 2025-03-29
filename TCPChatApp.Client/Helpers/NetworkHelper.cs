using System;
using System.IO;
using System.Net.Sockets;
using System.Windows;

namespace TCPChatApp.Client.Helpers
{
    public static class NetworkHelper
    {
        public static string SendMessageToServer(string serverAddress, int port, string message)
        {
            try
            {
                using TcpClient client = new TcpClient(serverAddress, port);
                using NetworkStream stream = client.GetStream();
                using var writer = new StreamWriter(stream) { AutoFlush = true };
                using var reader = new StreamReader(stream);

                writer.WriteLine(message);

                string response = reader.ReadLine();
                return response;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}