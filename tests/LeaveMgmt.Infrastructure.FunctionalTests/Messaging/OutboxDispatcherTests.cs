using System;
using System.Threading;
using System.Threading.Tasks;
using LeaveMgmt.Infrastructure.Messaging;
using LeaveMgmt.Infrastructure.Persistence;
using LeaveMgmt.Infrastructure.Persistence.Outbox;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace LeaveMgmt.Infrastructure.FunctionalTests.Messaging;

public class OutboxDispatcherTests
{
    private sealed class StubBus : IEventBus
    {
        public int Published { get; private set; }
        public Task PublishAsync<T>(string topic, T payload, CancellationToken ct = default)
        {
            Published++;
            return Task.CompletedTask;
        }
    }

    [Fact] // Functional
    public async Task Dispatcher_Should_Publish_And_Mark_Dispatched()
    {
        // Use ONE in-memory sqlite connection for the whole test
        var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();

        // Seed the database with an outbox message
        var dbOpts = new DbContextOptionsBuilder<LeaveMgmtDbContext>()
            .UseSqlite(conn)
            .Options;

        await using var seedCtx = new LeaveMgmtDbContext(dbOpts);
        await seedCtx.Database.EnsureCreatedAsync();

        var msg = new OutboxMessage { Topic = "LeaveRequestApproved", Payload = "{}" };
        seedCtx.Outbox.Add(msg);
        await seedCtx.SaveChangesAsync();

        // Build a minimal DI container the dispatcher will use per-scope
        var services = new ServiceCollection();

        // Register DbContext against the SAME open connection
        services.AddDbContext<LeaveMgmtDbContext>(o => o.UseSqlite(conn));

        // Register the stub bus so the dispatcher can resolve it in each scope
        var bus = new StubBus();
        services.AddSingleton<IEventBus>(bus);

        // Build provider and get scope factory
        var provider = services.BuildServiceProvider();
        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

        // New ctor: (ILogger<OutboxDispatcher>, IServiceScopeFactory)
        var dispatcher = new OutboxDispatcher(new NullLogger<OutboxDispatcher>(), scopeFactory);

        // Run dispatcher briefly then stop (wait > poll interval: ~2s)
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await dispatcher.StartAsync(cts.Token);

        // Give it one poll tick + buffer
        await Task.Delay(TimeSpan.FromMilliseconds(2500), cts.Token);

        await dispatcher.StopAsync(CancellationToken.None);

        // Verify it published and marked message as dispatched
        Assert.True(bus.Published >= 1);

        await using var verifyCtx = new LeaveMgmtDbContext(dbOpts);
        var first = await verifyCtx.Outbox.FirstAsync();
        Assert.NotNull(first.DispatchedUtc);
    }
}
