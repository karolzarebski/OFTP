using DatabaseLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DatabaseLibrary.DAL.Services
{
    public interface IDatabaseService
    {
        Task MigrateAsync();
        Task SaveChangesAsync();
        Task<List<User>> GetUserDataAsync();
        void AddUserDataAsync(User userData);
        void AddFriend(User user, Friend friend);
        void RemoveFriend(User user, Friend friend);
        Task<User> GetUserByLogin(string login);
    }
}
