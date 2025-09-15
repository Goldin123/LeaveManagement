using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.Users;
using LeaveMgmt.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LeaveMgmt.Infrastructure.Repositories;

public sealed class UserRepository(LeaveMgmtDbContext db) : IUserRepository
{
    public async Task<Result<User>> CreateAsync(User user, CancellationToken ct = default)
    {
        await db.Users.AddAsync(user, ct);
        await db.SaveChangesAsync(ct);
        return Result<User>.Success(user);
    }

    public async Task<Result<User?>> GetByEmailAsync(string email, CancellationToken ct = default)
        => Result<User?>.Success(await db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower(), ct));

    public async Task<Result<User?>> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Result<User?>.Success(await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct));
}
