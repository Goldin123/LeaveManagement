using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Application.Abstractions.Security;
using LeaveMgmt.Application.Common;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.Users;
using Microsoft.Extensions.Logging;

namespace LeaveMgmt.Application.Commands.Users.Register;

/// <summary>
/// Command request: register a new user with email, full name, password, and optional roles.
/// </summary>
public sealed record RegisterUserCommand(
    string Email,
    string FullName,
    string Password,
    IEnumerable<string>? Roles = null
) : IRequest<Result<Guid>>;

/// <summary>
/// Handles user registration. Validates uniqueness, hashes password,
/// creates a new user, persists it, and logs the process.
/// </summary>
public sealed class RegisterUserHandler(
    IUserRepository users,
    IPasswordHasher hasher,
    ILogger<RegisterUserHandler> logger) : IRequestHandler<RegisterUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand cmd, CancellationToken ct)
    {
        try
        {
            logger.LogInformation("Registration attempt for {Email}", cmd.Email);

            // 1) Check if user already exists
            var exists = await users.GetByEmailAsync(cmd.Email, ct);
            if (exists.IsSuccess && exists.Value is not null)
            {
                logger.LogWarning("Registration failed: email {Email} already exists", cmd.Email);
                return Result<Guid>.Failure("Email already registered.");
            }

            // 2) Hash the password
            var (hash, salt) = hasher.Hash(cmd.Password);

            // 3) Create a new user entity
            var user = new User(cmd.Email, cmd.FullName, hash, salt, cmd.Roles);

            // 4) Persist the user
            var created = await users.CreateAsync(user, ct);
            if (created.IsSuccess)
            {
                logger.LogInformation("User {Email} registered successfully with Id {UserId}", cmd.Email, user.Id);
                return Result<Guid>.Success(user.Id);
            }
            else
            {
                logger.LogError("Failed to persist user {Email}: {Error}", cmd.Email, created.Error);
                return Result<Guid>.Failure(created.Error!);
            }
        }
        catch (Exception ex)
        {
            // Catch-all for unexpected runtime exceptions
            logger.LogError(ex, "Unhandled exception during registration for {Email}", cmd.Email);
            return Result<Guid>.Failure($"Register failed: {ex.Message}");
        }
    }
}
