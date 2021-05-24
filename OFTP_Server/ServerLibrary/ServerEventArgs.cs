using System;
using System.Net.Sockets;

namespace ServerLibrary
{
    public class ServerEventArgs : EventArgs
    {
        public TcpClient client { get; set; }
    }
}
