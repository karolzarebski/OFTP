namespace OFTP_Client.Events
{
    public class SendProgressEvent
    {
        public bool Receive { get; set; }
        public int Value { get; set; }
        public bool General { get; set; }
        public int FilesCount { get; set; }
    }
}
