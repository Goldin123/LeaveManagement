using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Application.Abstractions.Security;
using LeaveMgmt.Domain;

namespace LeaveMgmt.Application.Commands.Users.Login;

public sealed record LoginUserCommand(string Email, string Password) : IRequest<Result<string>>;

public sealed class LoginUserHandler(
    IUserRepository users,
    IPasswordHasher hasher,
    IJwtTokenService tokens) : IRequestHandler<LoginUserCommand, Result<string>>
{
    public async Task<Result<string>> Handle(LoginUserCommand cmd, CancellationToken ct)
    {
        try
        {
            var found = await users.GetByEmailAsync(cmd.Email, ct);
            if (!found.IsSuccess || found.Value is null)
                return Result<string>.Failure("Invalid credentials.");

            var u = found.Value!;
            if (!hasher.Verify(cmd.Password, u.PasswordHash, u.PasswordSalt))
                return Result<string>.Failure("Invalid credentials.");

            var jwt = tokens.CreateToken(u.Id, u.Email, u.Roles);
            return Result<string>.Success(jwt);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Login failed: {ex.Message}");
        }
    }
}
