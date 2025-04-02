using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TCPChatApp.Common.Models;

namespace TCPChatApp.Client
{
    public static class ChatFileHandler
    {
        private static readonly string StorageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "storage");

        public static List<Message> LoadMessages(string recipient)
        {
            string filePath = Path.Combine(StorageFolder, $"{recipient}.json");
            EnsureStorageFolderExists();

            if (!File.Exists(filePath))
                return new List<Message>();

            try
            {
                string jsonContent = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<List<Message>>(jsonContent) ?? new List<Message>();
            }
            catch (Exception)
            {
                return new List<Message>();
            }
        }

        public static void SaveMessage(Message message, string recipient)
        {
            string filePath = Path.Combine(StorageFolder, $"{recipient}.json");
            EnsureStorageFolderExists();

            var messages = LoadMessages(recipient);
            messages.Add(message);

            string updatedJson = JsonSerializer.Serialize(messages, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, updatedJson);
        }

        private static void EnsureStorageFolderExists()
        {
            if (!Directory.Exists(StorageFolder))
            {
                Directory.CreateDirectory(StorageFolder);
            }
        }
    }
}