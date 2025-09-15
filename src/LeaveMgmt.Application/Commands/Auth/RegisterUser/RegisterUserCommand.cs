// File: src/LeaveMgmt.Application/Commands/Auth/RegisterUser/RegisterUserCommand.cs
using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Abstractions.Identity;
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Application.Abstractions.Security;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.Users; // User domain type (your file) :contentReference[oaicite:3]{index=3}

namespace LeaveMgmt.Application.Commands.Auth.RegisterUser;

public sealed record RegisterUserCommand(string Email, string FullName, string Password)
    : IRequest<Result<string>>; // returns JWT string

public sealed class RegisterUserHandler(
    IUserRepository users,
    IPasswordHasher hasher,
    IJwtTokenService jwt,                     // your interface name :contentReference[oaicite:4]{index=4}
    ITeamRoster roster)
    : IRequestHandler<RegisterUserCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RegisterUserCommand cmd, CancellationToken ct)
    {
        var email = cmd.Email.Trim().ToLowerInvariant();

        // 1) Only allow roster members
        if (!roster.TryGetRolesFor(email, out var roles) || roles.Count == 0)
            return Result<string>.Failure("This email is not authorized to register.");

        // 2) Must not exist already
        var existing = await users.GetByEmailAsync(email, ct);          // your IUserRepository :contentReference[oaicite:5]{index=5}
        if (!existing.IsSuccess) return Result<string>.Failure(existing.Error!);
        if (existing.Value is not null) return Result<string>.Failure("Email already registered.");

        // 3) Hash password + create
        var (hash, salt) = hasher.Hash(cmd.Password);
        var u = new User(email, cmd.FullName, hash, salt, roles);

        var created = await users.CreateAsync(u, ct);
        if (!created.IsSuccess) return Result<string>.Failure(created.Error!);

        // 4) Issue JWT with your service
        var token = jwt.CreateToken(u.Id, u.Email,u.FullName, u.Roles);
        return Result<string>.Success(token);
    }
}
