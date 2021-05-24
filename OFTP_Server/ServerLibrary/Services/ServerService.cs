using DatabaseLibrary.DAL.Services;
using LoginLibrary.Services;
using Microsoft.Extensions.Logging;
using ServerLibrary.Events;
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
        private readonly ILoginService _loginService;
        private readonly ILogger<ServerService> _logger;
        private readonly IDatabaseService _storageService;


        private readonly ServerConfiguration _serverConfiguration;
        private Dictionary<string, string> availableUsers = new Dictionary<string, string>();
        Dictionary<TcpClient, CryptoService> clients = new Dictionary<TcpClient, CryptoService>();

        private event EventHandler<UsersCountChangedEvent> usersCountChangedEvent;

        private bool eventFired = false;
        private int usersCount = 0;

        public ServerService(ServerConfiguration serverConfiguration, ILoginService loginService,
            ILogger<ServerService> logger, IDatabaseService storageService)
        {
            _storageService = storageService;
            _serverConfiguration = serverConfiguration;
            _loginService = loginService;
            _logger = logger;

            usersCountChangedEvent += RefreshAvailableUsers;
        }

        private async void RefreshAvailableUsers(object sender, UsersCountChangedEvent e)
        {
            foreach (var client in clients)
            {
                var encryptedData = await client.Value.EncryptData($"10|{e.Username}|{e.IPAddress}");

                var message = new byte[encryptedData.Length + 1];

                Array.Copy(encryptedData, 0, message, 1, encryptedData.Length);

                message[0] = (byte)encryptedData.Length;

                await client.Key.GetStream().WriteAsync(message);
            }
        }

        private async Task SendMessage(string message, TcpClient client)
        {
            var encryptedData = await clients[client].EncryptData(message);

            var encryptedMessage = new byte[encryptedData.Length + 1];

            Array.Copy(encryptedData, 0, encryptedMessage, 1, encryptedData.Length);

            encryptedMessage[0] = (byte)encryptedData.Length;

            await client.GetStream().WriteAsync(encryptedMessage);
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

                 Task.Run(async () => //TODO remove await
                 {
                     await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes("1"));

                     //Add key service

                     var cryptoService = new CryptoService();

                     var publicKey = cryptoService.GeneratePublicKey();
                     await client.GetStream().WriteAsync(publicKey);

                     byte[] clientPublicKey = new byte[72];
                     await client.GetStream().ReadAsync(clientPublicKey, 0, clientPublicKey.Length);

                     await client.GetStream().WriteAsync(cryptoService.GenerateIV(clientPublicKey));

                     bool loggedIn = false;

                     string clientIpAddress = client.Client.RemoteEndPoint.ToString();

                     while (!loggedIn)
                     {
                         await client.GetStream().ReadAsync(signInBuffer, 0, signInBuffer.Length);

                         var data = (await cryptoService.DecryptData(signInBuffer.Skip(1)
                             .Take(Convert.ToInt32(signInBuffer[0])).ToArray())).Split('|');

                         login = data[1];

                         switch (data[0])
                         {
                             case "2":
                                 if (await _loginService.CheckLoginCredentials(login, data[2]))
                                 {
                                     await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes("5"));
                                     loggedIn = true;

                                     if (!availableUsers.ContainsKey(login))
                                     {
                                         availableUsers.Add(login, clientIpAddress.Remove(clientIpAddress.IndexOf(':')));

                                         usersCountChangedEvent.Invoke(this, new UsersCountChangedEvent { Username = login, IPAddress = availableUsers[login]});

                                         clients.Add(client, cryptoService);
                                     }
                                 }
                                 else
                                 {
                                     await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes("4"));
                                     loggedIn = false;
                                 }
                                 break;
                             case "3":
                                 int registrationResultCode = await _loginService.RegisterAccount(login, data[2]);

                                 if (registrationResultCode == 6)
                                 {
                                     await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(registrationResultCode.ToString()));
                                     loggedIn = true;

                                     if (!availableUsers.ContainsKey(login))
                                     {
                                         availableUsers.Add(login, clientIpAddress.Remove(clientIpAddress.IndexOf(':')));
                                         usersCountChangedEvent.Invoke(this, new UsersCountChangedEvent { Username = login, IPAddress = availableUsers[login]});
                                         clients.Add(client, cryptoService);
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

                     while (true)
                     {
                         var codeBuffer = new byte[16];

                         await client.GetStream().ReadAsync(codeBuffer, 0, codeBuffer.Length);

                         var code = await cryptoService.DecryptData(codeBuffer);

                         if (code == "9")
                         {
                             clients.Remove(client);

                             usersCountChangedEvent.Invoke(this, new UsersCountChangedEvent { Username = login, IPAddress = availableUsers[login]});

                             availableUsers.Remove(login);

                             await client.GetStream().WriteAsync(await cryptoService.EncryptData("9"));

                             client.Dispose();

                             break;
                         }

                         //if (eventFired)
                         //{
                         //    await Task.Run(async () => //TODO remove await
                         //    {
                         //        foreach (KeyValuePair<string, string> dictionaryEntry in availableUsers)
                         //        {
                         //            await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(dictionaryEntry.Key));
                         //            usersCount--;
                         //        }
                         //    });
                         //}

                         // hmm await to stopuje program, by trzeba walnąć jakoś taskiem?
                     }

                 });
            }
        }
    }
}
