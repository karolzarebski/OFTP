﻿using OFTP_Client.Events;
using OFTP_Client.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OFTP_Client.FilesService
{
    public class SendFilesService
    {
        private readonly string _serverIP;
        private TcpClient _client;
        private CryptoService _cryptoService;
        private int bufferLen = 1023; //25 KB 25599
        private bool isPaused = false, isStopped = false;
        private readonly bool _isEncryptionUsed;

        public event EventHandler<SendProgressEvent> SendFileProgress;

        public SendFilesService(string serverIP, bool isEncryptionUsed)
        {
            _isEncryptionUsed = isEncryptionUsed;

            _serverIP = serverIP;
            _isEncryptionUsed = isEncryptionUsed;
            _cryptoService = new CryptoService();
        }

        public async Task<bool> Connect()
        {
            try
            {
                await (_client = new TcpClient()).ConnectAsync(_serverIP, 12138);

                var code = new byte[5];
                await _client.GetStream().ReadAsync(code, 0, code.Length);

                byte[] publicKey = new byte[77];
                await _client.GetStream().ReadAsync(publicKey, 0, publicKey.Length);

                var clientPublicKey = new byte[77];

                Array.Copy(_cryptoService.GeneratePublicKey(), 0, clientPublicKey, 5, 72);
                Array.Copy(Encoding.UTF8.GetBytes(ServerRequestCodes.DiffieHellmanKey), 0, clientPublicKey, 0, 3);
                clientPublicKey[3] = 0;
                clientPublicKey[4] = 72;
                await _client.GetStream().WriteAsync(clientPublicKey);

                byte[] iv = new byte[21];
                await _client.GetStream().ReadAsync(iv, 0, iv.Length);
                _cryptoService.AssignIV(publicKey.Skip(5).ToArray(), iv.Skip(5).ToArray());

                return true;
            }
            catch (SocketException ex)
            {
                MessageBox.Show($"Błąd łączenia z klientem\n{ex.Message}", "Błąd połączenia",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private async Task SendData(byte[] data)
        {
            var encryptedData = await _cryptoService.EncryptData(data);
            var encryptedMessage = new byte[encryptedData.Length + 7];
            Array.Copy(encryptedData, 0, encryptedMessage, 7, encryptedData.Length);
            Array.Copy(Encoding.UTF8.GetBytes(FileTransmissionCodes.NextPartialData), 0, encryptedMessage, 0, 3);
            var len = encryptedData.Length;
            encryptedMessage[3] = (byte)(len / 256);
            encryptedMessage[4] = (byte)(len % 256);
            encryptedMessage[5] = (byte)(data.Length / 256);
            encryptedMessage[6] = (byte)(data.Length % 256);
            await Task.Run(() => _client.Client.Send(encryptedMessage, encryptedMessage.Length, SocketFlags.Partial));
        }

        private async Task SendPlainData(byte[] data)
        {
            var message = new byte[data.Length + 5];
            Array.Copy(data, 0, message, 5, data.Length);
            Array.Copy(Encoding.UTF8.GetBytes(FileTransmissionCodes.NextPartialData), 0, message, 0, 3);
            var len = data.Length;
            message[3] = (byte)(len / 256);
            message[4] = (byte)(len % 256);
            await Task.Run(() => _client.Client.Send(message, message.Length, SocketFlags.Partial));
        }

        private async Task<string> ReceiveMessage()
        {
            var header = new byte[5];
            await _client.GetStream().ReadAsync(header, 0, 5);

            var len = header[3] * 256 + header[4];

            if (len != 0)
            {
                var message = new byte[len];
                await _client.GetStream().ReadAsync(message, 0, len);

                return $"{Encoding.UTF8.GetString(header.Take(3).ToArray())}|{await _cryptoService.DecryptData(message)}";
            }

            return Encoding.UTF8.GetString(header.Take(3).ToArray());
        }

        private async Task SendMessage(string code, string message)
        {
            var encryptedData = await _cryptoService.EncryptData(message);
            var encryptedMessage = new byte[encryptedData.Length + 5];
            Array.Copy(encryptedData, 0, encryptedMessage, 5, encryptedData.Length);
            Array.Copy(Encoding.UTF8.GetBytes(code), 0, encryptedMessage, 0, 3);
            var len = encryptedData.Length;
            encryptedMessage[3] = (byte)(len / 256);
            encryptedMessage[4] = (byte)(len % 256);
            await _client.GetStream().WriteAsync(encryptedMessage);        
        }

        private async Task SendMessage(string code)
        {
            var encryptedMessage = new byte[5];
            Array.Copy(Encoding.UTF8.GetBytes(code), 0, encryptedMessage, 0, 3);
            encryptedMessage[3] = 0;
            encryptedMessage[4] = 0;
            await _client.GetStream().WriteAsync(encryptedMessage);
        }

        public int Map(long x, long in_min, long in_max, long out_min, long out_max)
        {
            return Convert.ToInt32((x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min);
        }

        public async Task SendEndConnection()
        {
            await SendMessage(UserConnectionCodes.DisconnectFromClient);
        }

        public async Task<bool> SendFiles(List<string> _files)
        {
            try
            {
                var files = new List<string>(_files);

                int filesSent = 0;

                await SendMessage(FileTransmissionCodes.BeginFileTransmission, $"{files.Count}");

                var responseCode = await ReceiveMessage();

                if (responseCode == FileTransmissionCodes.AcceptFileTransmission)
                {
                    foreach (var file in files)
                    {
                        int i = 0;

                        FileInfo fi = new FileInfo(file);

                        int count = Convert.ToInt32(Math.Ceiling(fi.Length / Convert.ToDouble(bufferLen)));

                        using var fileStream = File.OpenRead(file);

                        var encryptionCode = _isEncryptionUsed ? "1" : "0";

                        await SendMessage(FileTransmissionCodes.FileLength, $"{fi.Name}|{fi.Length}|{encryptionCode}");

                        SendFileProgress.Invoke(this, new SendProgressEvent
                        {
                            Value = Map(filesSent++, 0, files.Count, 0, 100),
                            General = true,
                            Receive = false
                        });

                        while (fileStream.Position != fi.Length)
                        {
                            responseCode = await ReceiveMessage();

                            if (responseCode == FileTransmissionCodes.OK)
                            {
                                if (isStopped)
                                {
                                    if (_isEncryptionUsed)
                                    {
                                        var encryptedMessage = new byte[1031];
                                        Array.Copy(Encoding.UTF8.GetBytes(FileTransmissionCodes.FileTransmissionInterrupted), 0, encryptedMessage, 0, 3);
                                        var len = 1023;
                                        encryptedMessage[3] = (byte)(len / 256);
                                        encryptedMessage[4] = (byte)(len % 256);
                                        await _client.GetStream().WriteAsync(encryptedMessage);
                                    }
                                    else
                                    {
                                        await SendMessage(FileTransmissionCodes.FileTransmissionInterrupted, new string('0', 1023));
                                    }

                                    return false;
                                }

                                if (!isPaused)
                                {
                                    var buffer = new byte[bufferLen];

                                    var readLen = await fileStream.ReadAsync(buffer, 0, buffer.Length); //added await

                                    //Debug.WriteLine(buffer.Length);

                                    if (_isEncryptionUsed)
                                    {
                                        await SendData(buffer.Take(readLen).ToArray());
                                    }
                                    else
                                    {
                                        await SendPlainData(buffer.Take(readLen).ToArray());
                                    }
                                }

                                SendFileProgress.Invoke(this, new SendProgressEvent
                                {
                                    Value = Map(++i, 0, count, 0, 100),
                                    General = false,
                                    Receive = false
                                });
                            }
                            else if (responseCode == FileTransmissionCodes.FileTransmissionInterrupted)
                            {
                                MessageBox.Show("Transmisja plików została przerwana przez odbiorcę", "Przerwanie transmisji plików",
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                                return false;
                            }
                        }

                        SendFileProgress.Invoke(this, new SendProgressEvent
                        {
                            Value = Map(filesSent, 0, files.Count, 0, 100),
                            General = true,
                            Receive = false
                        });

                        //var endMessage = $"{FileTransmissionCodes.EndFileTransmission}|";

                        //await SendMessage(FileTransmissionCodes.EndFileTransmission);

                        while (await ReceiveMessage() != FileTransmissionCodes.OK) ;
                    }

                    return false;
                }
                else if (responseCode == FileTransmissionCodes.RejectFileTransmission)
                {
                    MessageBox.Show("Klient odmówił transmisji plików", "Odmowa",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    return false;
                }

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wystąpił błąd podczas transmisji plików", "Błąd transmisji",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
        }

        public void PauseSending()
        {
            isPaused = !isPaused;
        }

        public void StopSending()
        {
            isStopped = true;
        }
    }
}
