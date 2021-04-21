using DatabaseLibrary.Context;
using DatabaseLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseLibrary.DAL.Services
{
    public class DatabaseService : IDatabaseService
    {
        public User Users { get; set; }

        private readonly IUserContext _userContext;

        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public DatabaseService(IUserContext userContext)
        {
            _userContext = userContext;

            Users = new User();
        }

        public async void AddUserDataAsync(User userData)
        {
            userData.Id = userData.Id == 0 ? userData.Id : 0;

            _userContext.Users.Add(userData);

            await SaveChangesAsync();
        }

        public async Task<List<User>> GetUserDataAsync()
        {
            await semaphoreSlim.WaitAsync();

            try
            {
                await _userContext.Users.LoadAsync();
            }
            finally
            {
                semaphoreSlim.Release();
            }

            return _userContext.Users.Local.ToList();
        }

        public async Task MigrateAsync()
        {
            await semaphoreSlim.WaitAsync();

            try
            {
                await _userContext.Database.MigrateAsync();
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task SaveChangesAsync()
        {
            await semaphoreSlim.WaitAsync();

            try
            {
                await _userContext.SaveChangesAsync();
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
    }
}
