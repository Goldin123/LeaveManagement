using LeaveMgmt.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveMgmt.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("Users");
        b.HasKey(x => x.Id);
        b.Property(x => x.Email).HasMaxLength(320).IsRequired();
        b.HasIndex(x => x.Email).IsUnique();
        b.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        b.Property(x => x.PasswordHash).IsRequired();
        b.Property(x => x.PasswordSalt).IsRequired();
        b.Property(x => x.RolesCsv).HasMaxLength(400).IsRequired();
    }
}
