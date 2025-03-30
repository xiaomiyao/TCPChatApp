using System;

namespace TCPChatApp.Models
{
    public class Contact
    {
        public int Id { get; set; }
        public Guid OwnerUserId { get; set; }
        public Guid ContactUserId { get; set; }
        public string ContactName { get; set; }
        public DateTime AddedDate { get; set; }
    }
}