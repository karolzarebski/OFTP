using DatabaseLibrary.DAL.Services;
using LoginLibrary.Services;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerLibrary.Services
{
    public class ServerService : IServerService
    {
        private readonly IDatabaseService _databaseService;
        private readonly ILoginService _loginService;
        private readonly ILogger<ServerService> _logger;

        private readonly ServerConfiguration _serverConfiguration;
        private bool badCredentials = false;

        public ServerService(ServerConfiguration serverConfiguration, IDatabaseService databaseService,
            ILoginService loginService, ILogger<ServerService> logger)
        {
            _serverConfiguration = serverConfiguration;
            _databaseService = databaseService;
            _loginService = loginService;
            _logger = logger;
        }

        private async Task<bool> HandleLogin(string login, string password)
        {
            if (await _loginService.CheckData(login, password))
            {
                _logger.LogInformation($"User: {login} logged in");
                return true;
            }
            else
            {
                _logger.LogInformation($"User: {login} bad password");
                return false;
            }
        }

        public async Task StartServer()
        {
            //_databaseService.AddUserDataAsync(new DatabaseLibrary.Models.User()
            //{
            //    Login = "OK123",
            //    Password = Encoding.ASCII.GetBytes("Pass")
            //});

            //var s = await _databaseService.GetUserDataAsync();

            //await Task.Delay(100);


            TcpListener server = new TcpListener(IPAddress.Parse(_serverConfiguration.IpAddress), _serverConfiguration.Port);

            server.Start();

            while (true)
            {
                byte[] signInBuffer = new byte[2048];
                byte[] codeBuffer = new byte[1];

                TcpClient client = await server.AcceptTcpClientAsync();

                string login = string.Empty;
                string password = string.Empty;

                await Task.Run(async () =>
                 {
                     await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes("1"));

                     await client.GetStream().ReadAsync(signInBuffer, 0, signInBuffer.Length);

                     var data = Encoding.UTF8.GetString(signInBuffer).Split('|');

                     switch (data[0])
                     {
                         case "2":
                             await client.GetStream().WriteAsync(await HandleLogin(data[1], data[2])
                                 ? Encoding.UTF8.GetBytes("5")
                                 : Encoding.UTF8.GetBytes("4"));
                             break;
                         case "3":
                             await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes((
                                 await _loginService.RegisterAccount(data[1], data[2])).ToString()));
                             break;
                         default:
                             break;
                     }
                 });
            }
        }
    }
}
