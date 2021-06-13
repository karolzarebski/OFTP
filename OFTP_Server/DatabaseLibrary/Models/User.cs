using System.Collections.Generic;

namespace DatabaseLibrary.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Login { get; set; }
        public byte[] Password { get; set; }
        public string Salt { get; set; }
        public ICollection<Friend> Friend { get; set; } = new List<Friend>();
    }
}
