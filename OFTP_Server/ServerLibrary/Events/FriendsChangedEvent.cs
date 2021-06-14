using System.Net.Sockets;

namespace ServerLibrary.Events
{
    public class FriendsChangedEvent
    {
        public string Username1 { get; set; }
        public string Username2 { get; set; }
        public TcpClient Client1 { get; set; }
        public TcpClient Client2 { get; set; }
    }
}
