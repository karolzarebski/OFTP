using DatabaseLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DatabaseLibrary.Mappers
{
    public class FriendMapper : IEntityTypeConfiguration<Friend>
    {
        public void Configure(EntityTypeBuilder<Friend> builder)
        {
            builder.Property(p => p.Id)
                .ValueGeneratedOnAdd();

            builder.HasIndex(p => p.Username)
                .IsUnique();
        }
    }
}
