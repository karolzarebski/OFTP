using System.Threading.Tasks;
using LoginLibrary.Enums;

namespace LoginLibrary.Services
{
    public interface ILoginService
    {
        Task<int> RegisterAccount(string login, string password);
        Task<bool> CheckLoginCredentials(string login, string password);
        Task<bool> ChangePassword(string login, string password);
    }
}
