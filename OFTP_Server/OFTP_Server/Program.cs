using DatabaseLibrary;
using DatabaseLibrary.DAL;
using DatabaseLibrary.DAL.Services;
using LoginLibrary.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServerLibrary;
using ServerLibrary.Resources;
using ServerLibrary.Services;
using SmtpLibrary;
using SmtpLibrary.Services;
using System.Threading.Tasks;

namespace OFTP_Server
{
    class Program
    {
        private static ServiceProvider _serviceProvider;
        private static IConfiguration _configuration;

        static void Main(string[] args)
        {
            //try
            //{
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var servicesCollection = new ServiceCollection();
            ConfigureServices(servicesCollection);

            _serviceProvider = servicesCollection.BuildServiceProvider();

            Task.Run(() => _serviceProvider.GetRequiredService<IDatabaseService>().MigrateAsync()).Wait();

            _serviceProvider.GetRequiredService<IServerService>().StartServer().Wait();
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Exception occured during application startup {ex.Message}");
            //}
        }

        private static void ConfigureServices(IServiceCollection servicesCollection)
        {
            var databaseConfiguration = _configuration.GetSection("DatabaseConfiguration").Get<DatabaseConfiguration>();
            var serverConfiguration = _configuration.GetSection("ServerConfiguration").Get<ServerConfiguration>();
            var smtpConfiguration = _configuration.GetSection("SmtpConfiguration").Get<SmtpConfiguration>();

            servicesCollection
                .AddSingleton(_configuration)
                .AddSingleton(databaseConfiguration)
                .AddSingleton(serverConfiguration)
                .AddSingleton(smtpConfiguration)
                .AddSingleton<IServerService, ServerService>()
                .AddSingleton<IDatabaseService, DatabaseService>()
                .AddSingleton<ICryptoService, CryptoService>()
                .AddSingleton<ILoginService, LoginService>()
                .AddSingleton<ISmtpService, SmtpService>()
                .AddLogging(builder => builder.AddFile(_configuration.GetSection("Logging")));

            servicesCollection.RegisterDALDependiences();
        }
    }
}
