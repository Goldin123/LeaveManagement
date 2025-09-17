using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Application.DTOs;
using LeaveMgmt.Domain;
using Microsoft.Extensions.Logging;

namespace LeaveMgmt.Application.Queries.Users;

/// <summary>
/// Query request: get all users.
/// </summary>
public sealed record GetAllUsersQuery : IRequest<Result<IReadOnlyList<UserDto>>>;

/// <summary>
/// Handles retrieval of all users. Calls the repository,
/// maps domain entities to DTOs, and logs each important step.
/// </summary>
public sealed class GetAllUsersHandler(IUserRepository users, ILogger<GetAllUsersHandler> logger)
    : IRequestHandler<GetAllUsersQuery, Result<IReadOnlyList<UserDto>>>
{
    public async Task<Result<IReadOnlyList<UserDto>>> Handle(GetAllUsersQuery query, CancellationToken ct)
    {
        try
        {
            // Log attempt
            logger.LogInformation("Fetching all users");

            // 1) Fetch from repository
            var res = await users.GetAllAsync(ct);
            if (!res.IsSuccess || res.Value is null)
            {
                logger.LogWarning("Failed to fetch users. Error: {Error}", res.Error);
                return Result<IReadOnlyList<UserDto>>.Failure(res.Error ?? "Failed to load users.");
            }

            // 2) Map to DTOs
            var mapped = res.Value.Select(UserDto.FromDomain).ToList();

            logger.LogInformation("Successfully fetched {Count} users", mapped.Count);

            return Result<IReadOnlyList<UserDto>>.Success(mapped);
        }
        catch (Exception ex)
        {
            // Catch-all for unexpected runtime exceptions
            logger.LogError(ex, "Unhandled exception while fetching all users");
            return Result<IReadOnlyList<UserDto>>.Failure($"Query failed: {ex.Message}");
        }
    }
}
