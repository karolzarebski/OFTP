using System.Threading.Tasks;
using LoginLibrary.Enums;

namespace LoginLibrary.Services
{
    public interface ILoginService
    {
        Task<bool> RegisterAccount(string login, string password);
        Task<UserLoginSettings> CheckData(string login, string password);
        Task<bool> ChangePassword(string login, string password);
    }
}
