using LoginLibrary.Enums;
using Microsoft.Extensions.Logging;
using DatabaseLibrary.DAL.Services;
using DatabaseLibrary.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LoginLibrary.Services
{
    public class LoginService : ILoginService
    {
        private readonly ILogger<LoginService> _logger;
        private readonly IDatabaseService _storageService;
        private readonly ICryptoService _cryptoService;

        public LoginService(ILogger<LoginService> logger, IDatabaseService storageService,
            ICryptoService cryptoService)
        {
            _logger = logger;
            _storageService = storageService;
            _cryptoService = cryptoService;
        }

        private bool IsPasswordSecureEnough(string password)
        {
            return true;
        }

        /// <summary>
        /// Changes password
        /// </summary>
        /// <param name="data"></param>
        /// <returns>True if operation was succesfull</returns>
        public async Task<bool> ChangePassword(string login, string password)
        {
            if (!(await _storageService.GetUserDataAsync()).Any(u => u.Login == login))
            {
                return false;
            }

            try
            {
                _storageService.AddUserDataAsync(new User()
                {
                    Login = login,
                    Password = await _cryptoService.EncryptPassword(password)
                });
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if user is already in database
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Information if user is in database</returns>
        public async Task<bool> CheckData(string login, string password)
        {
            var user = (await _storageService.GetUserDataAsync()).FirstOrDefault(u => u.Login == login);

            if (user == null)
            {
                return false;
            }

            try
            {
                if (await _cryptoService.DecryptPassword(user.Password) == password)
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
            }

            return false;
        }

        /// <summary>
        /// Adds user to database
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Status of registration</returns>
        public async Task<int> RegisterAccount(string login, string password)
        {
            if ((await _storageService.GetUserDataAsync()).Any(u => u.Login == login))
            {
                return 7;
            }

            try
            {
                if (IsPasswordSecureEnough(password))
                {
                    _storageService.AddUserDataAsync(new User()
                    {
                        Login = login,
                        Password = await _cryptoService.EncryptPassword(password)
                    });

                    return 6;
                }

                return 8;
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);

                return 99;
            }
        }
    }
}
