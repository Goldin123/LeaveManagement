using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveMgmt.Infrastructure.Persistence.Outbox;
internal sealed class OutboxConfigurator : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> b)
    {
        b.ToTable("Outbox");
        b.HasKey(x => x.Id);
        b.Property(x => x.Topic).HasMaxLength(200).IsRequired();
        b.Property(x => x.Payload).IsRequired();
        b.HasIndex(x => x.DispatchedUtc);
    }
}
