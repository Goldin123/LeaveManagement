using LeaveMgmt.Domain.LeaveRequests;
using LeaveMgmt.Domain.LeaveTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveMgmt.Infrastructure.Persistence.Configurations;

internal sealed class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> b)
    {
        b.ToTable("LeaveRequests");
        b.HasKey(x => x.Id);

        // Value objects & simple properties
        b.Property(x => x.EmployeeId)
            .HasConversion(Persistence.ValueObjectConverters.EmployeeIdConverter)
            .IsRequired();

        b.Property(x => x.LeaveTypeId)
            .IsRequired();

        b.Property(x => x.Range)
            .HasConversion(Persistence.ValueObjectConverters.DateRangeConverter)
            .HasMaxLength(21) // "yyyy-MM-dd|yyyy-MM-dd"
            .IsRequired();

        b.Property(x => x.Reason)
            .HasMaxLength(512)
            .IsRequired();

        b.Property(x => x.Status)
            .HasConversion<int>() // enum -> int
            .IsRequired();

        b.Property(x => x.ApprovedBy)
            .HasConversion(Persistence.ValueObjectConverters.ManagerIdNullableConverter);

        b.Property(x => x.CreatedUtc).IsRequired();
        b.Property(x => x.SubmittedUtc);
        b.Property(x => x.DecidedUtc);

        // Relations
        b.HasOne<LeaveType>()
            .WithMany()
            .HasForeignKey(x => x.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Helpful indexes
        b.HasIndex(x => x.LeaveTypeId);
        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.EmployeeId);
    }
}
