namespace LeaveMgmt.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    private readonly List<IDomainEvent> _events = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _events;

    protected void Raise(IDomainEvent evt) => _events.Add(evt);
    public void ClearDomainEvents() => _events.Clear();
}
