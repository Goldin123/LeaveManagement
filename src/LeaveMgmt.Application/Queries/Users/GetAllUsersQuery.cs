using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Application.DTOs;
using LeaveMgmt.Domain;

namespace LeaveMgmt.Application.Queries.Users;

public sealed record GetAllUsersQuery : IRequest<Result<IReadOnlyList<UserDto>>>;

public sealed class GetAllUsersHandler(IUserRepository users)
    : IRequestHandler<GetAllUsersQuery, Result<IReadOnlyList<UserDto>>>
{
    public async Task<Result<IReadOnlyList<UserDto>>> Handle(GetAllUsersQuery query, CancellationToken ct)
    {
        try
        {
            var res = await users.GetAllAsync(ct);
            if (!res.IsSuccess || res.Value is null)
                return Result<IReadOnlyList<UserDto>>.Failure(res.Error ?? "Failed to load users.");

            var mapped = res.Value.Select(UserDto.FromDomain).ToList();
            return Result<IReadOnlyList<UserDto>>.Success(mapped);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<UserDto>>.Failure($"Query failed: {ex.Message}");
        }
    }
}
