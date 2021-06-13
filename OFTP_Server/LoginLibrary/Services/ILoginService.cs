using System.Threading.Tasks;

namespace LoginLibrary.Services
{
    public interface ILoginService
    {
        Task<int> RegisterAccount(string login, string password, string emailAddress);
        Task<bool> CheckLoginCredentials(string login, string password);
    }
}
