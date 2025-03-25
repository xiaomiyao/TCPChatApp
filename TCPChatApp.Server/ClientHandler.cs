using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using TCPChatApp.Common.Helpers;

namespace TCPChatApp.Server
{
    public class ClientHandler
    {
        private readonly TcpClient _client;
        private readonly List<TcpClient> _clients;

        public ClientHandler(TcpClient client, List<TcpClient> clients)
        {
            _client = client;
            _clients = clients;
        }

        public void HandleClient()
        {
            try
            {
                using var networkStream = _client.GetStream();
                using var reader = new StreamReader(networkStream);
                using var writer = new StreamWriter(networkStream) { AutoFlush = true };

                while (true)
                {
                    string encryptedMessage = reader.ReadLine();
                    if (string.IsNullOrEmpty(encryptedMessage))
                        break;

                    string plainText = TCPChatApp.Common.Helpers.EncryptionHelper.Decrypt(encryptedMessage);
                    Console.WriteLine($"Received: {plainText}");

                    BroadcastMessage(plainText);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error: {ex.Message}");
            }
            finally
            {
                lock (_clients)
                {
                    _clients.Remove(_client);
                }
                _client.Close();
            }
        }

        private void BroadcastMessage(string message)
        {
            string encryptedMessage = TCPChatApp.Common.Helpers.EncryptionHelper.Encrypt(message);
            lock (_clients)
            {
                foreach (var client in _clients)
                {
                    if (client != _client)
                    {
                        try
                        {
                            var stream = client.GetStream();
                            // Use leaveOpen = true to avoid closing the underlying stream
                            var writer = new StreamWriter(stream, System.Text.Encoding.UTF8, 1024, leaveOpen: true)
                            {
                                AutoFlush = true
                            };
                            writer.WriteLine(encryptedMessage);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error broadcasting message: {ex.Message}");
                        }
                    }
                }
            }
        }
    }
}
