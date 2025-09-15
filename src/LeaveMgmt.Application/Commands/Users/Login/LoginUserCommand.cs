using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Application.Abstractions.Security;
using LeaveMgmt.Domain;

namespace LeaveMgmt.Application.Commands.Users.Login;

public sealed record LoginUserCommand(string Email, string Password) : IRequest<Result<string>>;

public sealed class LoginUserHandler(
    IUserRepository users,
    IPasswordHasher hasher,
    IJwtTokenService jwt) : IRequestHandler<LoginUserCommand, Result<string>>
{
    public async Task<Result<string>> Handle(LoginUserCommand cmd, CancellationToken ct)
    {
        var email = cmd.Email.Trim().ToLowerInvariant();
        var found = await users.GetByEmailAsync(email, ct);
        if (!found.IsSuccess) return Result<string>.Failure(found.Error!);

        var u = found.Value;
        if (u is null) return Result<string>.Failure("Invalid credentials.");

        if (!hasher.Verify(cmd.Password, u.PasswordHash, u.PasswordSalt))
            return Result<string>.Failure("Invalid credentials.");

        var token = jwt.CreateToken(u.Id, u.Email, u.Roles);
        return Result<string>.Success(token);
    }
}
