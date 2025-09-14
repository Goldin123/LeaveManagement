// File: LeaveMgmt.Infrastructure/Messaging/RedisEventBus.cs
using System.Text.Json;
using StackExchange.Redis;

namespace LeaveMgmt.Infrastructure.Messaging;
public sealed class RedisEventBus(IConnectionMultiplexer mux) : IEventBus
{
    public async Task PublishAsync<T>(string topic, T payload, CancellationToken ct = default)
    {
        var db = mux.GetDatabase();
        var json = JsonSerializer.Serialize(payload);
        _ = await db.PublishAsync(topic, json);
    }
}
