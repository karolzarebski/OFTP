﻿using LoginLibrary.Services;
using Microsoft.Extensions.Logging;
using ServerLibrary.Events;
using ServerLibrary.Resources;
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

        private readonly ServerConfiguration _serverConfiguration;
        private List<UsersConnection> usersConnections = new List<UsersConnection>();
        private Dictionary<string, string> availableUsers = new Dictionary<string, string>();
        Dictionary<TcpClient, CryptoService> clients = new Dictionary<TcpClient, CryptoService>();

        private event EventHandler<UsersCountChangedEvent> usersCountChangedEvent;

        public ServerService(ServerConfiguration serverConfiguration, ILoginService loginService,
            ILogger<ServerService> logger)
        {
            _serverConfiguration = serverConfiguration;
            _loginService = loginService;
            _logger = logger;

            usersCountChangedEvent += RefreshAvailableUsers;

            //Added for testing purposes

            //availableUsers = new Dictionary<string, string> 
            //{
            //    { "Karol-PC", "192.168.1.11" },
            //    { "Karol-Laptop", "192.168.1.14" },
            //    {"Liam", "192.168.1.8" },
            //    {"Olivia", "192.168.29.22" },
            //    {"Noah" , "192.168.83.32"},
            //    { "Emma", "192.168.212.2"},
            //    { "Oliver", "192.168.92.212"},
            //    { "Ava", "192.168.13.93"},
            //    {"Elijah" , "192.168.129.94"},
            //    {"Charlotte" , "192.168.214.23"},
            //    {"William" , "192.168.254.54"},
            //    { "Sophia", "192.168.132.11"},
            //    { "James", "192.168.53.123"},
            //    {"Amelia" , "192.168.21.84"},
            //    {"Benjamin" , "192.168.21.37"},
            //    {"Isabella" , "192.168.11.98"},
            //    {"Lucas" , "192.168.111.73"},
            //    {  "Mia", "192.168.152.91"},
            //    { "Henry" , "192.168.213.211"},
            //    { "Evelyn", "192.168.10.182"},
            //    { "Alexander", "192.168.251.43"},
            //    {"Harper" , "192.168.167.142"}
            //};
        }

        private async void RefreshAvailableUsers(object sender, UsersCountChangedEvent e)
        {
            _logger.LogInformation($"Refreshing users ({clients.Count} available users)");

            foreach (var client in clients)
            {
                if (e.newClient != client.Key)
                {
                    await SendMessage(client.Key, CodeNames.NewUser, e.Username);
                }
            }
        }

        //private async Task SendMessage(string message, TcpClient client)
        //{
        //    var encryptedData = await clients[client].EncryptData(message);

        //    var encryptedMessage = new byte[encryptedData.Length + 2];

        //    Array.Copy(encryptedData, 0, encryptedMessage, 2, encryptedData.Length);

        //    var msgLen = encryptedData.Length;

        //    encryptedMessage[0] = (byte)(msgLen / 256);
        //    encryptedMessage[1] = (byte)(msgLen % 256);

        //    await client.GetStream().WriteAsync(encryptedMessage);
        //}

        private async Task SendMessage(TcpClient client, string code, string message)
        {
            var encryptedData = await clients[client].EncryptData(message);
            var encryptedMessage = new byte[encryptedData.Length + 5];
            Array.Copy(encryptedData, 0, encryptedMessage, 5, encryptedData.Length);
            Array.Copy(Encoding.UTF8.GetBytes(code), 0, encryptedMessage, 0, 3);
            var len = encryptedData.Length;
            encryptedMessage[3] = (byte)(len / 256);
            encryptedMessage[4] = (byte)(len % 256);
            await client.GetStream().WriteAsync(encryptedMessage);
        }

        private async Task SendMessage(TcpClient client, string code)
        {
            var encryptedMessage = new byte[5];
            Array.Copy(Encoding.UTF8.GetBytes(code), 0, encryptedMessage, 0, 3);
            encryptedMessage[3] = 0;
            encryptedMessage[4] = 0;
            await client.GetStream().WriteAsync(encryptedMessage);
        }

        private async Task<string> ReceiveMessage(TcpClient client)
        {
            var header = new byte[5];
            await client.GetStream().ReadAsync(header, 0, 5);

            var len = header[3] * 256 + header[4];

            if (len != 0)
            {
                var message = new byte[len];
                await client.GetStream().ReadAsync(message, 0, len);

                return $"{Encoding.UTF8.GetString(header.Take(3).ToArray())}|{await clients[client].DecryptData(message)}";
            }

            return Encoding.UTF8.GetString(header.Take(3).ToArray());

            //if (isCodeReceived)
            //{
            //    var codeBuffer = new byte[256]; //TODO check length
            //    await client.GetStream().ReadAsync(codeBuffer, 0, codeBuffer.Length);
            //    return await clients[client].DecryptData(codeBuffer.Skip(2).Take(codeBuffer[0] * 256 + codeBuffer[1]).ToArray());
            //}
            //else
            //{
            //    var messageBuffer = new byte[1024];
            //    await client.GetStream().ReadAsync(messageBuffer, 0, messageBuffer.Length);
            //    return await clients[client].DecryptData(messageBuffer.Skip(2)
            //            .Take(messageBuffer[0] * 256 + messageBuffer[1]).ToArray());
            //}
        }

        public async Task StartServer()
        {
            TcpListener server = new TcpListener(IPAddress.Parse(_serverConfiguration.IpAddress), _serverConfiguration.Port);

            _logger.LogInformation($"Starting serwer at: {_serverConfiguration.IpAddress}:{_serverConfiguration.Port}");

            server.Start();

            Console.WriteLine($"Starting serwer at: {_serverConfiguration.IpAddress}:{_serverConfiguration.Port}");

            while (true)
            {
                byte[] signInBuffer = new byte[2048];
                byte[] codeBuffer = new byte[1];

                TcpClient client = await server.AcceptTcpClientAsync();

                string login = string.Empty;
                string password = string.Empty;

                Task.Run(async () =>
                {
                    await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes($"{CodeNames.Connected}00"));

                    var cryptoService = new CryptoService();

                    var publicKey = cryptoService.GeneratePublicKey();

                    var diffieHellmanMessage = new byte[publicKey.Length + 5];

                    Array.Copy(publicKey, 0, diffieHellmanMessage, 5, publicKey.Length);
                    Array.Copy(Encoding.UTF8.GetBytes(CodeNames.DiffieHellmanKey), 0, diffieHellmanMessage, 0, 3);
                    diffieHellmanMessage[3] = 0;
                    diffieHellmanMessage[4] = 72;
                    await client.GetStream().WriteAsync(diffieHellmanMessage);

                    byte[] clientPublicKey = new byte[77];
                    await client.GetStream().ReadAsync(clientPublicKey, 0, clientPublicKey.Length);

                    diffieHellmanMessage = new byte[21];

                    var iv = cryptoService.GenerateIV(clientPublicKey.Skip(5).ToArray());

                    Array.Copy(iv, 0, diffieHellmanMessage, 5, iv.Length);
                    Array.Copy(Encoding.UTF8.GetBytes(CodeNames.DiffieHellmanIV), 0, diffieHellmanMessage, 0, 3);
                    diffieHellmanMessage[3] = 0;
                    diffieHellmanMessage[4] = 16;

                    await client.GetStream().WriteAsync(diffieHellmanMessage);

                    bool loggedIn = false;

                    string clientIpAddress = client.Client.RemoteEndPoint.ToString();

                    _logger.LogInformation($"User {clientIpAddress} connected");
                    Console.WriteLine($"User {clientIpAddress} connected");

                    clients.Add(client, cryptoService);

                    while (!loggedIn)
                    {
                        var data = (await ReceiveMessage(client)).Split('|');

                        if (data[0] == CodeNames.Login)
                        {
                            login = data[1];

                            if (!availableUsers.ContainsKey(login))
                            {
                                if (await _loginService.CheckLoginCredentials(login, data[2]))
                                {
                                    await SendMessage(client, CodeNames.CorrectLoginData);
                                    loggedIn = true;

                                    if (!availableUsers.ContainsKey(login))
                                    {
                                        availableUsers.Add(login, clientIpAddress.Remove(clientIpAddress.IndexOf(':')));

                                        _logger.LogInformation($"User {login} logged in");
                                        Console.WriteLine($"User {login} logged in");

                                        usersCountChangedEvent.Invoke(this, new UsersCountChangedEvent { Username = login, newClient = client });
                                    }
                                }

                                else
                                {
                                    await SendMessage(client, CodeNames.WrongLoginData);
                                    loggedIn = false;
                                    //clients.Remove(client);
                                }
                            }
                            else
                            {
                                loggedIn = false;
                                await SendMessage(client, CodeNames.UserAlreadyLoggedIn);
                            }

                        }
                        else if (data[0] == CodeNames.Register)
                        {
                            login = data[1];
                            int registrationResultCode = await _loginService.RegisterAccount(login, data[2]);

                            if (registrationResultCode.ToString() == CodeNames.CorrectRegisterData)
                            {
                                await SendMessage(client, registrationResultCode.ToString());
                                loggedIn = true;

                                if (!availableUsers.ContainsKey(login))
                                {
                                    availableUsers.Add(login, clientIpAddress.Remove(clientIpAddress.IndexOf(':')));

                                    _logger.LogInformation($"User {login} registered");
                                    Console.WriteLine($"User {login} registered");

                                    usersCountChangedEvent.Invoke(this, new UsersCountChangedEvent { Username = login, newClient = client });
                                }
                            }
                            else
                            {
                                await SendMessage(client, registrationResultCode.ToString());
                                loggedIn = false;
                                //clients.Remove(client);
                            }
                        }
                        else if (data[0] == CodeNames.Disconnect)
                        {
                            clients.Remove(client);
                            client.Close();
                            client.Dispose();

                            _logger.LogInformation($"User {client.Client.RemoteEndPoint} disconnected");
                            Console.WriteLine($"User {client.Client.RemoteEndPoint} disconnected");

                            break;
                        }
                    }

                    if (loggedIn)
                    {
                        var usersCode = await ReceiveMessage(client);

                        if (usersCode == CodeNames.ActiveUsers)
                        {
                            if (availableUsers.Count - 1 != 0)
                            {
                                var tempUsers = availableUsers;

                                await SendMessage(client, CodeNames.ActiveUsers, $"{tempUsers.Count - 1}");

                                if (await ReceiveMessage(client) == CodeNames.ActiveUsers)
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

                                        await SendMessage(client, CodeNames.ActiveUsers, preparatedData.Remove(preparatedData.Length - 1));

                                        await Task.Delay(1); //server sends data too fast
                                    }
                                }
                            }
                            else
                            {
                                await SendMessage(client, CodeNames.ActiveUsers, "-1");
                            }
                        }

                        while (true)
                        {
                            try
                            {
                                var message = await ReceiveMessage(client);

                                if (message == CodeNames.LogOut)
                                {
                                    _logger.LogInformation($"User {client.Client.RemoteEndPoint} logged out");
                                    Console.WriteLine($"User {client.Client.RemoteEndPoint} logged out");

                                    usersCountChangedEvent.Invoke(this, new UsersCountChangedEvent { Username = login, newClient = client });

                                    await SendMessage(client, CodeNames.LogOut);

                                    clients.Remove(client);
                                    availableUsers.Remove(login);

                                    client.Dispose();

                                    break;
                                }

                                else if (message == CodeNames.AskUserForConnection)
                                {
                                    var tempClientLogin = await ReceiveMessage(client);
                                    var tempClientIp = availableUsers[tempClientLogin];

                                    var tempClient = clients.Keys.Where(x => x.Client.RemoteEndPoint.ToString().StartsWith(tempClientIp)).FirstOrDefault();

                                    usersConnections.Add(new UsersConnection(login, availableUsers[login], tempClientLogin, tempClientIp));

                                    await SendMessage(tempClient, CodeNames.AskUserForConnection, login);

                                    var user = usersConnections.Where(x => x._userStartingConnection == login).FirstOrDefault();

                                    while (!user._userAccepted) { }

                                    if (user._userRejected)
                                    {
                                        await SendMessage(client, CodeNames.RejectedIncomingConnection); //0 tu było
                                    }
                                    else
                                    {
                                        await SendMessage(client, CodeNames.AcceptedIncomingConnection, $"{tempClientLogin}|{tempClientIp}");
                                    }

                                    usersConnections.Remove(user);
                                }
                                else if (message == CodeNames.AcceptedIncomingConnection)
                                {
                                    var user = usersConnections.Where(x => x._userAcceptingConnection == login
                                                && x._userAcceptingConnectionIP == availableUsers[login]).FirstOrDefault();

                                    if (user.IsMe(login, availableUsers[login]))
                                    {
                                        user._userAccepted = true;
                                        SendMessage(client, CodeNames.AcceptedIncomingConnection, user._userStartingConnectionIP);
                                    }
                                }
                                else if (message == CodeNames.RejectedIncomingConnection)
                                {
                                    var user = usersConnections.Where(x => x._userAcceptingConnection == login
                                                && x._userAcceptingConnectionIP == availableUsers[login]).FirstOrDefault();

                                    if (user.IsMe(login, availableUsers[login]))
                                    {
                                        user._userAccepted = true;
                                        user._userRejected = true;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogInformation($"Critical error for user {availableUsers[login]}");
                                Console.WriteLine($"Critical error for user {availableUsers[login]}");

                                usersCountChangedEvent.Invoke(this, new UsersCountChangedEvent { Username = login, newClient = client });

                                clients.Remove(client);

                                availableUsers.Remove(login);

                                break;
                            }
                        }
                    }
                });
            }
        }
    }
}
