using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Application.Abstractions.Security;
using LeaveMgmt.Domain;
using Microsoft.Extensions.Logging;

namespace LeaveMgmt.Application.Commands.Users.Login;

/// <summary>
/// Command request: authenticate a user and return a JWT if successful.
/// </summary>
public sealed record LoginUserCommand(string Email, string Password) : IRequest<Result<string>>;

/// <summary>
/// Marker interface: allows anonymous access (no auth required).
/// </summary>
public interface IAllowAnonymous { }

/// <summary>
/// Handles user login: verifies credentials, issues JWT, and logs events.
/// </summary>
public sealed class LoginUserHandler(
    IUserRepository users,
    IPasswordHasher hasher,
    IJwtTokenService jwt,
    ILogger<LoginUserHandler> logger) : IRequestHandler<LoginUserCommand, Result<string>>
{
    public async Task<Result<string>> Handle(LoginUserCommand cmd, CancellationToken ct)
    {
        var email = cmd.Email.Trim().ToLowerInvariant();
        logger.LogInformation("Login attempt for {Email}", email);

        // 1) Fetch user by email
        var found = await users.GetByEmailAsync(email, ct);
        if (!found.IsSuccess)
        {
            logger.LogError("Failed to fetch user {Email}: {Error}", email, found.Error);
            return Result<string>.Failure(found.Error!);
        }

        var u = found.Value;
        if (u is null)
        {
            logger.LogWarning("Login failed: user {Email} not found", email);
            return Result<string>.Failure("Invalid credentials.");
        }

        // 2) Verify password
        if (!hasher.Verify(cmd.Password, u.PasswordHash, u.PasswordSalt))
        {
            logger.LogWarning("Login failed: invalid password for {Email}", email);
            return Result<string>.Failure("Invalid credentials.");
        }

        // 3) Issue JWT
        var token = jwt.CreateToken(u.Id, u.Email, u.FullName, u.Roles);
        logger.LogInformation("Login successful for {Email}", email);
        return Result<string>.Success(token);
    }
}
