using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLibrary.Models
{
    public class Friend
    {
        public long Id { get; set; }
        public User User { get; set; }
        public long UserId { get; set; }
        public string Username { get; set; }
    }
}
