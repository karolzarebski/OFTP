using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace SmtpLibrary.Services
{
    public class SmtpService : ISmtpService
    {
        private readonly SmtpConfiguration _smtpConfiguration;
        private readonly ILogger<SmtpService> _logger;

        private SmtpClient smtpClient;

        public SmtpService(SmtpConfiguration smtpConfiguration, ILogger<SmtpService> logger)
        {
            _smtpConfiguration = smtpConfiguration;
            _logger = logger;

            ConfigureSmtpClient();
        }

        private void ConfigureSmtpClient()
        {
            smtpClient = new SmtpClient();

            smtpClient.Port = _smtpConfiguration.Port;
            smtpClient.Host = _smtpConfiguration.Hostname;
            smtpClient.EnableSsl = true;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(_smtpConfiguration.SenderCredentials.SenderLogin,
                _smtpConfiguration.SenderCredentials.SenderPassword);
        }

        public async Task<bool> SendAbsenceEmail(string destinationEmail, string senderEmail, string senderUsername)
        {
            try
            {
                using var mailDetails = new MailMessage();
                mailDetails.From = new MailAddress(_smtpConfiguration.SenderCredentials.SenderLogin);
                mailDetails.To.Add(destinationEmail);
                mailDetails.Subject = "Nieodebrana transmisja plików";
                mailDetails.Body = $"{DateTime.Now} - użytkownik {senderUsername} dokonał próby wysłania plików podczas twojej nieobecności\n" +
                $"Możesz skontakować się z nim wysyłając wiadomość na adres email: {senderEmail}";

                _logger.LogInformation($"Trying to send email from {senderUsername} ({senderEmail}) to {destinationEmail}");
                Console.WriteLine($"Trying to send email from {senderUsername} ({senderEmail}) to {destinationEmail}");

                await smtpClient.SendMailAsync(mailDetails);

                _logger.LogInformation("Email sent successfully");
                Console.WriteLine("Email sent successfully");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sending email from {senderUsername} ({senderEmail}) to {destinationEmail} failed");
                Console.WriteLine($"Sending email from {senderUsername} ({senderEmail}) to {destinationEmail} failed");

                return false;
            }
        }

        public async Task<bool> SendRegistrationEmail(string destinationEmail)
        {
            try
            {
                using var mailDetails = new MailMessage();
                mailDetails.From = new MailAddress(_smtpConfiguration.SenderCredentials.SenderLogin);
                mailDetails.To.Add(destinationEmail);
                mailDetails.Subject = "Witamy w OFTP";
                mailDetails.Body = $"Ten adres email został użyty w celu rejestracji konta w systemie OFTP w dniu {DateTime.Now}\n\n" +
                    $"Życzymy pomyślnego korzystania z programu\nPozdrawiamy, zespół OFTP";

                _logger.LogInformation($"Trying to send registration email to {destinationEmail}");
                Console.WriteLine($"Trying to send registration email to { destinationEmail}");

                await smtpClient.SendMailAsync(mailDetails);

                _logger.LogInformation("Email sent successfully");
                Console.WriteLine("Email sent successfully");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sending registration email to {destinationEmail} failed");
                Console.WriteLine($"Sending registration email to { destinationEmail} failed");

                return false;
            }
        }
    }
}
