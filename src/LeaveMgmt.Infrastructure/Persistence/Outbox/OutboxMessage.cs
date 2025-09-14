// File: LeaveMgmt.Infrastructure/Persistence/Outbox/OutboxMessage.cs
namespace LeaveMgmt.Infrastructure.Persistence.Outbox;
public sealed class OutboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime CreatedUtc { get; init; } = DateTime.UtcNow;
    public string Topic { get; init; } = default!;
    public string Payload { get; init; } = default!;
    public DateTime? DispatchedUtc { get; set; }
}
