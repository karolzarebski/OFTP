using DatabaseLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DatabaseLibrary.Mappers
{
    public class UserMapper : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(p => p.Id);

            builder.HasIndex(p => p.Login)
                .IsUnique();

            builder.Property(p => p.Login)
                .IsRequired();

            builder.Property(p => p.Password)
                .IsRequired();

            builder.HasMany(p => p.Friend)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
