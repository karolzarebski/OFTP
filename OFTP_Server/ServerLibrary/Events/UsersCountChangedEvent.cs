using System;

namespace ServerLibrary.Events
{
    public class UsersCountChangedEvent : EventArgs
    {
        public string Username { get; set; }
        public string IPAddress { get; set; }
        public string Status { get; set; }
    }
}
