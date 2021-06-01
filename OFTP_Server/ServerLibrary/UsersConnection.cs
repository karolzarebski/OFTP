namespace ServerLibrary
{
    public class UsersConnection
    {
        public string _userStartingConnection;
        public string _userStartingConnectionIP;
        public string _userAcceptingConnection;
        public string _userAcceptingConnectionIP;
        public bool _userRejected = false;
        public bool _userAccepted;

        public UsersConnection(string userStartingConnection, string userStartingConnectionIP,
            string userAcceptingConnection, string userAcceptingConnectionIP)
        {
            _userStartingConnection = userStartingConnection;
            _userStartingConnectionIP = userStartingConnectionIP;
            _userAcceptingConnection = userAcceptingConnection;
            _userAcceptingConnectionIP = userAcceptingConnectionIP;
            _userAccepted = false;
        }

        public bool IsMe(string login, string ip)
        {
            if (login == _userAcceptingConnection && ip == _userAcceptingConnectionIP)
            {
                return true;
            }
            return false;
        }
    }
}
