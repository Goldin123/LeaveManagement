using System.Text.Json;
using LeaveMgmt.Domain.Common;
using LeaveMgmt.Domain.LeaveRequests;
using LeaveMgmt.Domain.LeaveTypes;
using LeaveMgmt.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace LeaveMgmt.Infrastructure.Persistence;

public sealed class LeaveMgmtDbContext : AppDbContext
{
    public LeaveMgmtDbContext(DbContextOptions<LeaveMgmtDbContext> options) : base(options) { }

    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();
    public DbSet<LeaveMgmt.Domain.Users.User> Users => Set<LeaveMgmt.Domain.Users.User>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // AppDbContext already applies configurations
        modelBuilder.ApplyConfiguration(new OutboxConfigurator());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1) Gather & clear domain events from tracked aggregates
        var domainEvents = ChangeTracker.Entries()
            .Select(e => e.Entity)
            .OfType<Entity>()
            .SelectMany(e => e.DequeueDomainEvents())
            .ToList();

        // 2) Convert to outbox messages
        foreach (var evt in domainEvents)
        {
            var topic = evt.GetType().Name;
            var payload = JsonSerializer.Serialize(evt);
            Outbox.Add(new OutboxMessage { Topic = topic, Payload = payload });
        }

        // 3) Commit
        return await base.SaveChangesAsync(cancellationToken);
    }
}
