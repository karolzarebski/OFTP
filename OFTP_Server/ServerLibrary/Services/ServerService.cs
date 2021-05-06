using LoginLibrary.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerLibrary.Services
{
    public class ServerService : IServerService
    {
        private readonly ILoginService _loginService;
        private readonly ILogger<ServerService> _logger;

        private readonly ServerConfiguration _serverConfiguration;
        private Dictionary<string, string> availableUsers = new Dictionary<string, string>();

        private event EventHandler<ServerEventArgs> newUserAvailableEvent;

        private bool eventFired = false;
        private int usersCount = 0;

        public ServerService(ServerConfiguration serverConfiguration, ILoginService loginService, ILogger<ServerService> logger)
        {
            _serverConfiguration = serverConfiguration;
            _loginService = loginService;
            _logger = logger;

            newUserAvailableEvent += RefreshAvailableUsers;
        }

        private void RefreshAvailableUsers(object sender, ServerEventArgs e)
        {
            usersCount = availableUsers.Count;
            Debug.WriteLine("FIRED");
            eventFired = true;

            while (usersCount > 0) { }
            eventFired = false;
        }

        public async Task StartServer()
        {
            TcpListener server = new TcpListener(IPAddress.Parse(_serverConfiguration.IpAddress), _serverConfiguration.Port);

            server.Start();

            while (true)
            {
                byte[] signInBuffer = new byte[2048];
                byte[] codeBuffer = new byte[1];

                TcpClient client = await server.AcceptTcpClientAsync();

                string login = string.Empty;
                string password = string.Empty;

                await Task.Run(async () => //TODO remove await
                 {
                     await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes("1"));

                     bool loggedIn = false;

                     string clientIpAddress = client.Client.RemoteEndPoint.ToString();

                     while (!loggedIn)
                     {
                         await client.GetStream().ReadAsync(signInBuffer, 0, signInBuffer.Length);

                         var data = Encoding.UTF8.GetString(signInBuffer).Replace("\0", "").Split('|');

                         switch (data[0])
                         {
                             case "2":
                                 if (await _loginService.CheckLoginCredentials(data[1], data[2]))
                                 {
                                     await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes("5"));
                                     loggedIn = true;

                                     if (!availableUsers.ContainsKey(login))
                                     {
                                         availableUsers.Add(login, clientIpAddress.Remove(clientIpAddress.IndexOf(':')));

                                         newUserAvailableEvent.Invoke(this, new ServerEventArgs { client = client });
                                     }
                                 }
                                 else
                                 {
                                     await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes("4"));
                                     loggedIn = false;
                                 }
                                 break;
                             case "3":
                                 int registrationResultCode = await _loginService.RegisterAccount(data[1], data[2]);

                                 if (registrationResultCode == 6)
                                 {
                                     await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(registrationResultCode.ToString()));
                                     loggedIn = true;

                                     if (!availableUsers.ContainsKey(login))
                                     {
                                         availableUsers.Add(login, clientIpAddress.Remove(clientIpAddress.IndexOf(':')));
                                         newUserAvailableEvent.Invoke(this, new ServerEventArgs { client = client });
                                     }
                                 }
                                 else
                                 {
                                     await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(registrationResultCode.ToString()));
                                     loggedIn = false;
                                 }
                                 break;
                             default:
                                 break;
                         }
                     }

                     var s = availableUsers.Keys;

                     while (true)
                     {
                         if (eventFired)
                         {
                             await Task.Run(async () => //TODO remove await
                             {
                                 foreach (KeyValuePair<string, string> dictionaryEntry in availableUsers)
                                 {
                                     await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(dictionaryEntry.Key));
                                     usersCount--;
                                 }
                             });
                         }

                         // hmm await to stopuje program, by trzeba walnąć jakoś taskiem?
                     }

                 });
            }
        }
    }
}
