using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryTCPChatApp.Common.Models
{
    public class Message
    {
        public string Sender { get; set; }      
        public string Recipient { get; set; }    
        public string Content { get; set; }     
        public DateTime Timestamp { get; set; }
    }
}
