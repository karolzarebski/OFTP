using System;

namespace OFTP_Client.Events
{
    public class UsersListChangedEvent : EventArgs
    {
        public string username;
        public string IPAddress;
    }
}
