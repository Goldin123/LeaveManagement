using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Domain.Entities;

namespace LeaveMgmt.Infrastructure.InMemory;

public sealed class SystemClock : IDateTime
{
    public DateTime UtcNow => DateTime.UtcNow;
}

public sealed class InMemoryRepository<T> : IRepository<T> where T : class
{
    private readonly Dictionary<Guid,T> _store = new();

    public Task<T?> GetByIdAsync(Guid id, CancellationToken ct)
        => Task.FromResult(_store.TryGetValue(id, out var e) ? e : null);

    public Task AddAsync(T entity, CancellationToken ct)
    {
        var idProp = typeof(T).GetProperty("Id");
        if (idProp is null || idProp.PropertyType != typeof(Guid))
            throw new InvalidOperationException($"Type {typeof(T).Name} must have Guid Id property");

        var id = (Guid) (idProp.GetValue(entity) ?? Guid.Empty);
        if (id == Guid.Empty) { id = Guid.NewGuid(); idProp.SetValue(entity, id); }
        _store[id] = entity;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity, CancellationToken ct)
    {
        var idProp = typeof(T).GetProperty("Id") ?? throw new InvalidOperationException("Missing Id");
        var id = (Guid)(idProp.GetValue(entity) ?? Guid.Empty);
        if (id == Guid.Empty) throw new InvalidOperationException("Id cannot be empty");
        _store[id] = entity;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct)
    {
        _store.Remove(id);
        return Task.CompletedTask;
    }
}
