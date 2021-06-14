using DatabaseLibrary.Mappers;
using DatabaseLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLibrary.Context
{
    public class UserContext : DbContext, IUserContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Friend> Friends { get; set; }


        private readonly DatabaseConfiguration _databaseConfiguration;
        public UserContext(DatabaseConfiguration databaseConfiguration,
            DbContextOptions<UserContext> dbContextOptions) : base(dbContextOptions)
        {
            _databaseConfiguration = databaseConfiguration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder dbContextOptions)
        {
            if (!dbContextOptions.IsConfigured)
            {
                dbContextOptions.UseSqlServer(_databaseConfiguration.ConnectionString);
                base.OnConfiguring(dbContextOptions);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserMapper());
            modelBuilder.ApplyConfiguration(new FriendMapper());
        }
    }
}
