using LeaveMgmt.Domain;
using LeaveMgmt.Domain.Users;

namespace LeaveMgmt.Application.Abstractions.Repositories;

public interface IUserRepository
{
    Task<Result<User>> CreateAsync(User user, CancellationToken ct = default);
    Task<Result<User?>> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<Result<User?>> GetByIdAsync(Guid id, CancellationToken ct = default);
}
