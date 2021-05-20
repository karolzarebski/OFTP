using System;

namespace OFTP_Client.Events
{
    public class IncommingConnectionEvent : EventArgs
    {
        public string Message { get; set; }
    }
}
