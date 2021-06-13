using System;

namespace OFTP_Client.Events
{
    public class SendEmailEvent : EventArgs 
    {
        public string UnavailableUsername { get; set; }
        public string Username { get; set; }
    }
}
