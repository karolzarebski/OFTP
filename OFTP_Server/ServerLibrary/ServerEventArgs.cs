using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerLibrary
{
    public class ServerEventArgs : EventArgs
    {
        public TcpClient client { get; set; }
    }
}
