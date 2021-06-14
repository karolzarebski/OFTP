using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using Assert = NUnit.Framework.Assert;

namespace SmtpLibrary.Services.Tests
{
    [TestFixture]
    public class SmtpServiceTests
    {
        [Test]
        [TestCase("marekmarczewski1234@gmail.com", true)]
        [TestCase("karol.zarebski1@wp.pl", true)]
        [TestCase("karolkarolski25@outlook.com", true)]
        [TestCase("karol.zarebski@student.put.poznan.pl", true)]
        public async Task SendRegistrationEmailTest(string destinationEmail, bool expectedResult)
        {
            bool result;

            var _logger = new Mock<ILogger<SmtpService>>();

            var smtpConfiguration = new SmtpConfiguration
            {
                Port = 587,
                Hostname = "smtp.gmail.com",
                SenderCredentials = new SenderCredentials
                {
                    SenderLogin = "team.oftp@gmail.com",
                    SenderPassword = "321edgeedge123"
                }
            };

            var smtpService = new SmtpService(smtpConfiguration, _logger.Object);

            result = await smtpService.SendRegistrationEmail(destinationEmail);

            Assert.AreEqual(expectedResult, result);
        }
    }
}