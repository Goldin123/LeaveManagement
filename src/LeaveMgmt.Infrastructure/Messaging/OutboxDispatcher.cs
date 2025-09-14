// File: LeaveMgmt.Infrastructure/Messaging/OutboxDispatcher.cs
using LeaveMgmt.Infrastructure.Persistence;
using LeaveMgmt.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LeaveMgmt.Infrastructure.Messaging;
public sealed class OutboxDispatcher(
    LeaveMgmtDbContext db,
    IEventBus bus,
    ILogger<OutboxDispatcher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var batch = await db.Outbox
                .Where(o => o.DispatchedUtc == null)
                .OrderBy(o => o.CreatedUtc)
                .Take(100)
                .ToListAsync(stoppingToken);

            foreach (var msg in batch)
            {
                await bus.PublishAsync(msg.Topic, msg.Payload, stoppingToken);
                msg.DispatchedUtc = DateTime.UtcNow;
            }

            if (batch.Count > 0)
                await db.SaveChangesAsync(stoppingToken);

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }
}
