// File: LeaveMgmt.Infrastructure.FunctionalTests/Messaging/OutboxDispatcherTests.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using LeaveMgmt.Infrastructure.Messaging;
using LeaveMgmt.Infrastructure.Persistence;
using LeaveMgmt.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

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
        // Arrange: in-memory Sqlite for EF
        var opts = new DbContextOptionsBuilder<LeaveMgmtDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        await using var db = new LeaveMgmtDbContext(opts);
        await db.Database.OpenConnectionAsync();
        await db.Database.EnsureCreatedAsync();

        var msg = new OutboxMessage { Topic = "LeaveRequestApproved", Payload = "{}" };
        db.Outbox.Add(msg);
        await db.SaveChangesAsync();

        var bus = new StubBus();
        var dispatcher = new OutboxDispatcher(db, bus, new NullLogger<OutboxDispatcher>());

        // Act: run the dispatcher shortly and cancel
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(3500));
        await dispatcher.StartAsync(cts.Token);
        await Task.Delay(500, cts.Token); // give it one tick
        await dispatcher.StopAsync(CancellationToken.None);

        // Assert: message marked dispatched and published
        Assert.True(bus.Published >= 1);
        Assert.NotNull((await db.Outbox.FirstAsync()).DispatchedUtc);
    }
}
