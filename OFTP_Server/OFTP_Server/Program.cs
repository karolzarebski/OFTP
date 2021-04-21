using System;
using System.Threading.Tasks;
using DatabaseLibrary;
using DatabaseLibrary.DAL;
using DatabaseLibrary.DAL.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServerLibrary;
using ServerLibrary.Services;

namespace OFTP_Server
{
    class Program
    {
        private static ServiceProvider _serviceProvider;
        private static IConfiguration _configuration;

        static void Main(string[] args)
        {
            try
            {
                _configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build();

                var servicesCollection = new ServiceCollection();
                ConfigureServices(servicesCollection);

                _serviceProvider = servicesCollection.BuildServiceProvider();

                Task.Run(() => _serviceProvider.GetRequiredService<IDatabaseService>().MigrateAsync()).Wait();

                _serviceProvider.GetRequiredService<IServerService>().StartServer().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured during application startup {ex.Message}");
            }
        }

        private static void ConfigureServices(IServiceCollection servicesCollection)
        {
            var databaseConfiguration = _configuration.GetSection("DatabaseConfiguration").Get<DatabaseConfiguration>();
            var serverConfiguration = _configuration.GetSection("ServerConfiguration").Get<ServerConfiguration>();

            servicesCollection
                .AddSingleton(_configuration)
                .AddSingleton(databaseConfiguration)
                .AddSingleton(serverConfiguration)
                .AddSingleton<IServerService, ServerService>()
                .AddSingleton<IDatabaseService, DatabaseService>();

            servicesCollection.RegisterDALDependiences();
        }
    }
}
