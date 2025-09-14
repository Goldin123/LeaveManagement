using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using LeaveMgmt.Infrastructure.Messaging;
using LeaveMgmt.Infrastructure.Persistence;
using LeaveMgmt.Infrastructure.Persistence.Outbox;

namespace LeaveMgmt.Infrastructure.Messaging;

public sealed class OutboxDispatcher(
    ILogger<OutboxDispatcher> logger,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    // how often to drain
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox dispatcher started.");

        // simple loop; use PeriodicTimer to avoid tight spin waits
        using var timer = new PeriodicTimer(PollInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                // create a fresh scope for each iteration to use scoped services safely
                using var scope = scopeFactory.CreateScope();

                var db = scope.ServiceProvider.GetRequiredService<LeaveMgmtDbContext>();
                var bus = scope.ServiceProvider.GetRequiredService<IEventBus>();

                // read a small batch of unsent messages
                var batch = await db.Outbox
                    .Where(x => x.DispatchedUtc == null)
                    .OrderBy(x => x.Id)
                    .Take(50)
                    .ToListAsync(stoppingToken);

                if (batch.Count == 0)
                    continue;

                foreach (var msg in batch)
                {
                    try
                    {
                        // forward to redis (topic = event type name)
                        await bus.PublishAsync(msg.Topic, msg.Payload, stoppingToken);

                        // mark as sent
                        msg.DispatchedUtc = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        // keep it unsent; log and proceed to next
                        logger.LogError(ex, "Failed to publish outbox message {Id} ({Topic})", msg.Id, msg.Topic);
                    }
                }

                // commit marks for the processed batch
                await db.SaveChangesAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // graceful shutdown
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Outbox dispatcher crashed.");
            // swallow to avoid host crash; the host will stop anyway on fatal errors
        }

        logger.LogInformation("Outbox dispatcher stopped.");
    }
}
