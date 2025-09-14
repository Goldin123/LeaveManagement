using LeaveMgmt.Domain.LeaveTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveMgmt.Infrastructure.Persistence.Configurations;

internal sealed class LeaveTypeConfiguration : IEntityTypeConfiguration<LeaveType>
{
    public void Configure(EntityTypeBuilder<LeaveType> b)
    {
        b.ToTable("LeaveTypes");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        // From your domain usage (e.g., range validation)
        b.Property(x => x.MaxDaysPerRequest)
            .IsRequired();

        // If you have a Description field in your builders, persist it too
        if (typeof(LeaveType).GetProperty("Description") is not null)
        {
            b.Property<string>("Description")
             .HasMaxLength(512);
        }

        b.HasIndex(x => x.Name).IsUnique();
    }
}
