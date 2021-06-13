using DatabaseLibrary.DAL.Services;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace LoginLibrary.Services.Tests
{
    [TestFixture]
    public class LoginServiceTests
    {
        [Test]
        [TestCase("123", false)]
        [TestCase("qwerty", false)]
        [TestCase("qwerty123", false)]
        [TestCase("Qwerty123", false)]
        [TestCase("Qwerty", false)]
        [TestCase("Qwertyuiop", false)]
        [TestCase("qwertyuioP", false)]
        [TestCase("QWERTYUIOP", false)]
        [TestCase("Qq123456789", true)]
        [TestCase("q123456789Q", true)]
        public void IsPasswordSecureEnoughTest(string password, bool expectedResult)
        {
            bool result;

            var _storageService = new Mock<IDatabaseService>();
            var _logger = new Mock<ILogger<LoginService>>();
            var _crypto = new Mock<ICryptoService>();

            var loginService = new LoginService(_logger.Object, _storageService.Object, _crypto.Object);

            result = loginService.IsPasswordSecureEnough(password);

            Assert.AreEqual(expectedResult, result);
        }
    }
}