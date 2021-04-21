using DatabaseLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DatabaseLibrary.Mappers
{
    public class UserMapper : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasIndex(p => p.Login)
                .IsUnique();

            builder.Property(p => p.Login)
                .IsRequired();

            builder.Property(p => p.Password)
                .IsRequired();
        }
    }
}
