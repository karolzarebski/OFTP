using DatabaseLibrary.DAL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerLibrary.Services
{
    public class ServerService : IServerService
    {
        private readonly IDatabaseService _databaseService;

        private readonly ServerConfiguration _serverConfiguration;

        public ServerService(ServerConfiguration serverConfiguration, IDatabaseService databaseService)
        {
            _serverConfiguration = serverConfiguration;
            _databaseService = databaseService;
        }

        public async Task StartServer()
        {
            //_databaseService.AddUserDataAsync(new DatabaseLibrary.Models.User()
            //{
            //    Login = "OK123",
            //    Password = Encoding.ASCII.GetBytes("Pass")
            //});

            var s = await _databaseService.GetUserDataAsync();

            await Task.Delay(100);


            //TcpListener server = new TcpListener(IPAddress.Parse(_serverConfiguration.IpAddress), _serverConfiguration.Port);
            //server.Start();

            //while (true)
            //{
            //    byte[] signInBuffer = new byte[2048];

            //    TcpClient client = await server.AcceptTcpClientAsync();

            //    Task.Run(async () =>
            //    {
            //        await client.GetStream().ReadAsync(signInBuffer, 0, 2);
            //    });
            //}
        }
    }
}
