namespace SmtpLibrary
{
    public class SmtpConfiguration
    {
        public int Port { get; set; }
        public string Hostname { get; set; }
        public SenderCredentials SenderCredentials { get; set; }
    }

    public class SenderCredentials
    {
        public string SenderLogin { get; set; }
        public string SenderPassword { get; set; }
    }
}
