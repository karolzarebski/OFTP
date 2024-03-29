﻿using DatabaseLibrary.Context;
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
                await _userContext.Users.Include(u => u.Friend).LoadAsync();
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

        public void AddFriend(User user, Friend friend)
        {
            user.Friend.Add(friend);
        }

        public async Task<User> GetUserByLogin(string login)
        {
            return (await GetUserDataAsync()).FirstOrDefault(x => x.Login == login);
        }

        public void RemoveFriend(User user, Friend friend)
        {
            var tempFriend = user.Friend.Where(x => x.Username == friend.Username).FirstOrDefault();

            user.Friend.Remove(tempFriend);
        }
    }
}
