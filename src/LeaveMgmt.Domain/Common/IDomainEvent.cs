namespace LeaveMgmt.Domain.Common;

public interface IDomainEvent { DateTime OccurredOnUtc { get; } }

public abstract class DomainEventBase : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
