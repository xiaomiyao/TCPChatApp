
using System;
using System.IO;
using System.Net.Sockets;
using TCPChatApp.Common.Helpers;

namespace TCPChatApp.Server
{
    public class ClientHandler
    {
        private readonly TcpClient _client;

        public ClientHandler(TcpClient client)
        {
            _client = client;
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

                    
                    string response = $"Server echo: {plainText}";
                    string encryptedResponse = TCPChatApp.Common.Helpers.EncryptionHelper.Encrypt(response);
                    writer.WriteLine(encryptedResponse);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error: {ex.Message}");
            }
            finally
            {
                _client.Close();
            }
        }
    }
}
