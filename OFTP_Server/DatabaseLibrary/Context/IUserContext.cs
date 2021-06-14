using DatabaseLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseLibrary.Context
{
    public interface IUserContext
    {
        DbSet<User> Users { get; set; }
        DbSet<Friend> Friends { get; set; }
        DatabaseFacade Database { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
