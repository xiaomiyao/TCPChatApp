using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPChatApp.Common.Models
{
    public class Envelope
    {
        public string Type { get; set; }
        public Message Message { get; set; }
        public User User { get; set; }
        public List<User> Users { get; set; }
    }
}
