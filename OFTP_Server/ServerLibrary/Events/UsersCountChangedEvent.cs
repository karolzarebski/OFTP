using System;
using System.Net.Sockets;

namespace ServerLibrary.Events
{
    public class UsersCountChangedEvent : EventArgs
    {
        public string Username { get; set; }
        public TcpClient newClient { get; set; }
    }
}
