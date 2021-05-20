using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace OFTP_Client
{
    public class DictionaryService
    {
        public Dictionary<string, IPAddress> users { get; private set; } = new Dictionary<string, IPAddress>();

        public DictionaryService()
        {
            users = FillDictionary();
        }

        private Dictionary<string, IPAddress> FillDictionary()
        {
            return new Dictionary<string, IPAddress>
            {
                { "Karol-PC", IPAddress.Parse("192.168.1.11") },
                { "Karol-Laptop", IPAddress.Parse("192.168.1.14") },
                {"Liam", IPAddress.Parse("192.168.1.8") },
                {"Olivia", IPAddress.Parse("192.168.29.22") },
                {"Noah" , IPAddress.Parse("192.168.83.32")},
                { "Emma", IPAddress.Parse("192.168.212.2")},
                { "Oliver", IPAddress.Parse("192.168.92.212")},
                { "Ava", IPAddress.Parse("192.168.13.93")},
                {"Elijah" , IPAddress.Parse("192.168.129.94")},
                {"Charlotte" , IPAddress.Parse("192.168.214.23")},
                {"William" , IPAddress.Parse("192.168.254.54")},
                { "Sophia", IPAddress.Parse("192.168.132.11")},
                { "James", IPAddress.Parse("192.168.53.123")},
                {"Amelia" , IPAddress.Parse("192.168.21.84")},
                {"Benjamin" , IPAddress.Parse("192.168.21.37")},
                {"Isabella" , IPAddress.Parse("192.168.11.98")},
                {"Lucas" , IPAddress.Parse("192.168.111.73")},
                {  "Mia", IPAddress.Parse("192.168.152.91")},
                { "Henry" , IPAddress.Parse("192.168.213.211")},
                { "Evelyn", IPAddress.Parse("192.168.10.182")},
                { "Alexander", IPAddress.Parse("192.168.251.43")},
                {"Harper" , IPAddress.Parse("192.168.167.142")}
            };
        }

        public string GetKeyByValue(string value)
        {
            return users.FirstOrDefault(x => x.Value.ToString() == value).Key;
        }

    }
}
