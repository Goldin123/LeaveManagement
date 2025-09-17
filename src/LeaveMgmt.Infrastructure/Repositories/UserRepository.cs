using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.Users;
using LeaveMgmt.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeaveMgmt.Infrastructure.Repositories;

/// <summary>
/// Repository for managing User persistence (CRUD operations).
/// Uses EF Core DbContext and logs all important operations.
/// </summary>
public sealed class UserRepository : IUserRepository
{
    private readonly LeaveMgmtDbContext _db;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(LeaveMgmtDbContext db, ILogger<UserRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Create a new user.
    /// </summary>
    public async Task<Result<User>> CreateAsync(User user, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating user {Email}", user.Email);

            await _db.Users.AddAsync(user, ct);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("User {Email} created successfully with Id {UserId}", user.Email, user.Id);

            return Result<User>.Success(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user {Email}", user.Email);
            return Result<User>.Failure($"Failed to create user: {ex.Message}");
        }
    }

    /// <summary>
    /// Get a user by email.
    /// </summary>
    public async Task<Result<User?>> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Fetching user by email {Email}", email);

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower(), ct);

            if (user is null)
                _logger.LogWarning("User with email {Email} not found", email);
            else
                _logger.LogInformation("User {Email} fetched successfully", email);

            return Result<User?>.Success(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch user by email {Email}", email);
            return Result<User?>.Failure($"Failed to get user: {ex.Message}");
        }
    }

    /// <summary>
    /// Get a user by Id.
    /// </summary>
    public async Task<Result<User?>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Fetching user by Id {UserId}", id);

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

            if (user is null)
                _logger.LogWarning("User {UserId} not found", id);
            else
                _logger.LogInformation("User {UserId} fetched successfully", id);

            return Result<User?>.Success(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch user {UserId}", id);
            return Result<User?>.Failure($"Failed to get user: {ex.Message}");
        }
    }

    /// <summary>
    /// Get all users.
    /// </summary>
    public async Task<Result<IReadOnlyList<User>>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Fetching all users");

            var list = await _db.Users.ToListAsync(ct);

            _logger.LogInformation("Successfully fetched {Count} users", list.Count);

            return Result<IReadOnlyList<User>>.Success(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch users");
            return Result<IReadOnlyList<User>>.Failure($"Failed to get users: {ex.Message}");
        }
    }
}
