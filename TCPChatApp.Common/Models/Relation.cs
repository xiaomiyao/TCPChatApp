namespace TCPChatApp.Common.Models
{
    public class Relation
    {
        public string UserName { get; set; }
        public string TargetName { get; set; }

        public bool IsBlocked { get; set; } = false;
        public bool IsFriend { get; set; } = false;
    }
}