namespace LeaveMgmt.Application.Abstractions;

public interface IDateTime
{
    DateTime UtcNow { get; }
}

public interface IRepository<T>
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(T entity, CancellationToken ct);
    Task UpdateAsync(T entity, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}
