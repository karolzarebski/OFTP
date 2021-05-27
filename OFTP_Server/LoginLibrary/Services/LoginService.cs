using DatabaseLibrary.DAL.Services;
using DatabaseLibrary.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
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

        private string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private bool IsPasswordSecureEnough(string password) //Minimum ten characters, at least one uppercase letter, one lowercase letter and one number:
        {
            return Regex.IsMatch(password, "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)[a-zA-Z\\d]{10,}$");
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
                    Password = await _cryptoService.EncryptData(password)
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
        public async Task<bool> CheckLoginCredentials(string login, string password)
        {
            var user = (await _storageService.GetUserDataAsync()).FirstOrDefault(u => u.Login == login);

            if (user == null)
            {
                return false;
            }

            try
            {
                if (_cryptoService.CreateHash(password, user.Salt).SequenceEqual(user.Password))
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
                return 201; //RegistrationLoginExists
            }

            try
            {   
                if (IsPasswordSecureEnough(password))
                {
                    var salt = RandomString(8);
                    _storageService.AddUserDataAsync(new User()
                    {
                        Login = login,
                        Password = _cryptoService.CreateHash(password, salt),
                        Salt = salt
                    });

                    return 104; //CorrectRegisterData
                }

                return 202; //RegistrationPasswordWrong
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);

                return 999;
            }
        }
    }
}
