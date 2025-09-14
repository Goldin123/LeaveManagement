// File: LeaveMgmt.Infrastructure/Messaging/IEventBus.cs
namespace LeaveMgmt.Infrastructure.Messaging;
public interface IEventBus
{
    Task PublishAsync<T>(string topic, T payload, CancellationToken ct = default);
}
