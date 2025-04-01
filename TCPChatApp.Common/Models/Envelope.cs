using System.Collections.Generic;

namespace TCPChatApp.Common.Models
{
    public class Envelope
    {
        public string Type { get; set; }
        public Message Message { get; set; }
        public User User { get; set; }
        public List<User> Users { get; set; }
        // <<<< NEW PROPERTY >>>>
        public List<Relation> Relations { get; set; }
    }
}
