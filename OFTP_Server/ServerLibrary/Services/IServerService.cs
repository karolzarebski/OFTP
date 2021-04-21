using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLibrary.Services
{
    public interface IServerService
    {
        public Task StartServer();
    }
}
