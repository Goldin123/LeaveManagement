using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Application.Abstractions.Security;
using LeaveMgmt.Application.Common;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.Users;

namespace LeaveMgmt.Application.Commands.Users.Register;

public sealed record RegisterUserCommand(
    string Email, string FullName, string Password, IEnumerable<string>? Roles = null
) : IRequest<Result<Guid>>;

public sealed class RegisterUserHandler(
    IUserRepository users,
    IPasswordHasher hasher) : IRequestHandler<RegisterUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand cmd, CancellationToken ct)
    {
        try
        {
            var exists = await users.GetByEmailAsync(cmd.Email, ct);
            if (exists.IsSuccess && exists.Value is not null)
                return Result<Guid>.Failure("Email already registered.");

            var (hash, salt) = hasher.Hash(cmd.Password);
            var user = new User(cmd.Email, cmd.FullName, hash, salt, cmd.Roles);
            var created = await users.CreateAsync(user, ct);
            return created.IsSuccess
                ? Result<Guid>.Success(user.Id)
                : Result<Guid>.Failure(created.Error!);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Register failed: {ex.Message}");
        }
    }
}
