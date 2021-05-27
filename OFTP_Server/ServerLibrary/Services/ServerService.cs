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

            availableUsers = new Dictionary<string, string>
            {
                { "Karol-PC", "192.168.1.11" },
                { "Karol-Laptop", "192.168.1.14" },
                {"Liam", "192.168.1.8" },
                {"Olivia", "192.168.29.22" },
                {"Noah" , "192.168.83.32"},
                { "Emma", "192.168.212.2"},
                { "Oliver", "192.168.92.212"},
                { "Ava", "192.168.13.93"},
                {"Elijah" , "192.168.129.94"},
                {"Charlotte" , "192.168.214.23"},
                {"William" , "192.168.254.54"},
                { "Sophia", "192.168.132.11"},
                { "James", "192.168.53.123"},
                {"Amelia" , "192.168.21.84"},
                {"Benjamin" , "192.168.21.37"},
                {"Isabella" , "192.168.11.98"},
                {"Lucas" , "192.168.111.73"},
                {  "Mia", "192.168.152.91"},
                { "Henry" , "192.168.213.211"},
                { "Evelyn", "192.168.10.182"},
                { "Alexander", "192.168.251.43"},
                {"Harper" , "192.168.167.142"}
            };

            for (int i = 0; i < 100; i++)
            {
                availableUsers.Add($"Name{i}", $"192.168.99.{i + 1}");
            }
     
        }

        private async void RefreshAvailableUsers(object sender, UsersCountChangedEvent e)
        {
            foreach (var client in clients)
            {
                if (e.newClient != client.Key)
                {
                    //var encryptedData = await client.Value.EncryptData();

                    //var message = new byte[encryptedData.Length + 1];

                    //Array.Copy(encryptedData, 0, message, 1, encryptedData.Length);

                    //message[0] = (byte)encryptedData.Length;

                    //await client.Key.GetStream().WriteAsync(message);

                    await SendMessage($"{Resources.CodeNames.NewUser}|{e.Username}", client.Key);
                }

            }
        }

        private async Task SendMessage(string message, TcpClient client)
        {
            var encryptedData = await clients[client].EncryptData(message);

            var encryptedMessage = new byte[encryptedData.Length + 2];

            Array.Copy(encryptedData, 0, encryptedMessage, 2, encryptedData.Length);

            var msgLen = encryptedData.Length;

            encryptedMessage[0] = (byte)(msgLen / 256);
            encryptedMessage[1] = (byte)(msgLen % 256);

            await client.GetStream().WriteAsync(encryptedMessage);
        }

        private async Task<string> ReceiveMessage(TcpClient client, bool isCodeReceived = false)
        {
            if (isCodeReceived)
            {
                var codeBuffer = new byte[256]; //TODO check length
                await client.GetStream().ReadAsync(codeBuffer, 0, codeBuffer.Length);
                return await clients[client].DecryptData(codeBuffer.Skip(2).Take(codeBuffer[0] * 256 + codeBuffer[1]).ToArray());
            }
            else
            {
                var messageBuffer = new byte[1024];
                await client.GetStream().ReadAsync(messageBuffer, 0, messageBuffer.Length);
                return await clients[client].DecryptData(messageBuffer.Skip(2)
                        .Take(messageBuffer[0] * 256 + messageBuffer[1]).ToArray());
            }
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
                    await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(Resources.CodeNames.Connected));

                    var cryptoService = new CryptoService();

                    var publicKey = cryptoService.GeneratePublicKey();
                    await client.GetStream().WriteAsync(publicKey);

                    byte[] clientPublicKey = new byte[72];
                    await client.GetStream().ReadAsync(clientPublicKey, 0, clientPublicKey.Length);

                    await client.GetStream().WriteAsync(cryptoService.GenerateIV(clientPublicKey));

                    bool loggedIn = false;

                    string clientIpAddress = client.Client.RemoteEndPoint.ToString();

                    clients.Add(client, cryptoService);

                    while (!loggedIn)
                    {
                        var data = (await ReceiveMessage(client, false)).Split('|');

                        if (data[0] == Resources.CodeNames.Login)
                        {
                            login = data[1];
                            if (await _loginService.CheckLoginCredentials(login, data[2]))
                            {
                                await SendMessage(Resources.CodeNames.CorrectLoginData, client);
                                loggedIn = true;

                                if (!availableUsers.ContainsKey(login))
                                {
                                    availableUsers.Add(login, clientIpAddress.Remove(clientIpAddress.IndexOf(':')));

                                    usersCountChangedEvent.Invoke(this, new UsersCountChangedEvent { Username = login, newClient = client });
                                }
                            }

                            else
                            {
                                await SendMessage(Resources.CodeNames.WrongLoginData, client);
                                loggedIn = false;
                                clients.Remove(client);
                            }
                        }
                        else if (data[0] == Resources.CodeNames.Register)
                        {
                            login = data[1];
                            int registrationResultCode = await _loginService.RegisterAccount(login, data[2]);

                            if (registrationResultCode.ToString() == Resources.CodeNames.CorrectRegisterData)
                            {
                                await SendMessage(registrationResultCode.ToString(), client);
                                loggedIn = true;

                                if (!availableUsers.ContainsKey(login))
                                {
                                    availableUsers.Add(login, clientIpAddress.Remove(clientIpAddress.IndexOf(':')));
                                    usersCountChangedEvent.Invoke(this, new UsersCountChangedEvent { Username = login, newClient = client });
                                }
                            }
                            else
                            {
                                await SendMessage(registrationResultCode.ToString(), client);
                                loggedIn = false;
                                clients.Remove(client);
                            }
                        }
                        else if (data[0] == Resources.CodeNames.Disconnect)
                        {
                            clients.Remove(client);
                            client.Close();
                            client.Dispose();
                            break;
                        }
                    }

                    if (loggedIn)
                    {

                        if (availableUsers.Count - 1 != 0)
                        {
                            var tempUsers = availableUsers;

                            await SendMessage($"{Resources.CodeNames.ActiveUsers}|{tempUsers.Count - 1}", client);

                            if (await ReceiveMessage(client, true) == Resources.CodeNames.ActiveUsers)
                            {
                                while (tempUsers.Any())
                                {
                                    var preparatedData = string.Empty;

                                    var partOfData = tempUsers.Take(100);
                                    tempUsers = tempUsers.Skip(100).ToDictionary(p => p.Key, p => p.Value);

                                    foreach (var user in partOfData)
                                    {
                                        if (user.Key != login)
                                        {
                                            preparatedData += $"{user.Key}\n";
                                        }
                                    }

                                    await SendMessage(preparatedData.Remove(preparatedData.Length - 1), client);

                                    await Task.Delay(10); //server sends data too fast
                                }
                            }
                        }
                        else
                        {
                            await SendMessage($"{Resources.CodeNames.ActiveUsers}|0", client);
                        }

                        while (true)
                        {
                            var message = await ReceiveMessage(client, true);

                            if (message == Resources.CodeNames.LogOut)
                            {

                                usersCountChangedEvent.Invoke(this, new UsersCountChangedEvent { Username = login, newClient = client });

                                await SendMessage(Resources.CodeNames.LogOut, client);

                                clients.Remove(client);
                                availableUsers.Remove(login);

                                client.Dispose();

                                break;
                            }
                            else if (message == Resources.CodeNames.GetIp)
                            {
                                var username = await ReceiveMessage(client, false);
                                await SendMessage(availableUsers[username].ToString(), client);
                            }
                        }
                    }
                });
            }
        }
    }
}
