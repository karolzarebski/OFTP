using System.Threading.Tasks;

namespace SmtpLibrary.Services
{
    public interface ISmtpService
    {
        Task<bool> SendAbsenceEmail(string destinationEmail, string senderEmail, string senderUsername);
        Task<bool> SendRegistrationEmail(string destinationEmail);
    }
}
