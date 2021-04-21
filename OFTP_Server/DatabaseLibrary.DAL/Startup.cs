using DatabaseLibrary.Context;
using DatabaseLibrary.DAL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseLibrary.DAL
{
    public static class Startup
    {
        public static void RegisterDALDependiences(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IDatabaseService, DatabaseService>();
            serviceCollection.AddDbContext<IUserContext, UserContext>(ServiceLifetime.Transient);
        }
    }
}
