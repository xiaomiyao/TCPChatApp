using System;
using System.IO;
using System.Net.Sockets;
using TCPChatApp.Common.Helpers;

class ClientTest
{
    static void Main()
    {
        using var client = new TcpClient("127.0.0.1", 5000);
        using var stream = client.GetStream();
        using var writer = new StreamWriter(stream) { AutoFlush = true };
        using var reader = new StreamReader(stream);

        string message = "Hello from test client!";
        string encrypted = TCPChatApp.Common.Helpers.EncryptionHelper.Encrypt(message);

        writer.WriteLine(encrypted);
        string responseEncrypted = reader.ReadLine();
        string response = TCPChatApp.Common.Helpers.EncryptionHelper.Decrypt(responseEncrypted);

        Console.WriteLine($"Response from server: {response}");
    }
}
