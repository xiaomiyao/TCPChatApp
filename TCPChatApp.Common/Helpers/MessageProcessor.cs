using System.Text.Json;
using TCPChatApp.Common.Models;

namespace TCPChatApp.Common.Helpers
{
    public static class MessageProcessor
    {
        // 🔄 Serialize the envelope into JSON and encrypt it
        public static string SerializeAndEncrypt(Envelope envelope)
        {
            string json = JsonSerializer.Serialize(envelope);
            return CryptoHelper.Encrypt(json);
        }

        // 🔓 Decrypt the incoming message and deserialize it into an envelope
        public static Envelope DecryptAndDeserialize(string encryptedResponse)
        {
            string responseJson = CryptoHelper.Decrypt(encryptedResponse);
            return JsonSerializer.Deserialize<Envelope>(responseJson);
        }
    }
}