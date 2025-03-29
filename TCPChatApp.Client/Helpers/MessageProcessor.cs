using System.Text.Json;
using TCPChatApp.Common.Models;
using TCPChatApp.Common.Helpers;
using TCPChatApp.Client.Helpers;

namespace TCPChatApp.Client.Helpers
{
    public static class MessageProcessor
    {
        public static string SerializeAndEncrypt(Envelope envelope)
        {
            string json = JsonSerializer.Serialize(envelope);
            return CryptoHelper.Encrypt(json);
        }

        public static Envelope DecryptAndDeserialize(string encryptedResponse)
        {
            string responseJson = CryptoHelper.Decrypt(encryptedResponse);
            return JsonSerializer.Deserialize<Envelope>(responseJson);
        }
    }
}